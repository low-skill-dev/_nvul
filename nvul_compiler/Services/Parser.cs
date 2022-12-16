using nvul_compiler.Models.CodeTree;
using nvul_compiler.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("nvul_compiler.tests")]
namespace nvul_compiler.Services
{
	internal class Parser
	{
		internal struct IndexRange
		{
			public int Start;
			public int Count;
			public int End => Start + Count;

			public IndexRange(int start, int count)
			{
				this.Start = start;
				this.Count = count;
			}
		}

		public char lineDelimiter { get; protected set; } = ';';

		protected readonly NvulConfiguration _configuration;
		protected Dictionary<ExpressionTypes, Regex> _regexForExpressionType = new();

		public Parser(NvulConfiguration configuration)
		{
			this._configuration = configuration;

			//var varDeclarationRegex = new Regex(@"(?<=\s*)(TYPENAMEREPLACE)\s+([A-Za-zА-Яа-я]+[A-Za-zА-Яа-я1-9]*)(?=\s*)"
			//	.Replace(@"TYPENAMEREPLACE", string.Join('|', _configuration.Keywords.Where(x => x.Type.Equals("vartype")))));

			this._operatorsByPriorityDescending = this._configuration.Operators.OrderByDescending(x => x.OperatorPriority).ToList();
			this._operatorsEvaluator = new(false, false);
			this._operatorsByPriorityDescending.ForEach(x => { try { this._operatorsEvaluator.AddOperator(x.OperatorString, (x, y) => 0, x.OperatorPriority); } catch { } });
			this._configuration.NvulFunctions.ToList().ForEach(x => this._operatorsEvaluator.AddFunction(x.FunctionName, (y) => 0));
		}

		protected const string varNameRegexGroupName = "VARNAMEGROUPNAME";
		protected const string varValueRegexGroupName = "ASSIGNEDVALUE";


		//	protected readonly Regex varDeclarationRegex;
		protected bool VariableNameIsOk(string varname) => varname.Length > 0 && char.IsLetter(varname[0]) && varname.All(x => char.IsLetterOrDigit(x));

		protected bool IsDeclarationString(string line, out DeclarationNode? Node)
		{
			Node = null;

			var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 2) return false; // type name;

			var varTypes = _configuration.Keywords.Where(x => x.Type.Equals("vartype")).Select(x => x.Word);
			var varType = varTypes.FirstOrDefault(t => parts[0].Equals(t));
			if (varType is null) return false;

			var varName = parts[1];
			if (!VariableNameIsOk(varName))
			{
				throw new Exception($"Variable name {parts[1]} is not allowed. Variable name must start with letter and may contain only letters or digits.");
				//return true;
			}

			Node = new DeclarationNode(varType, varName);

			return true;
		}

		protected bool IsAssignmentString(string line, out AssignmentNode? Node)
		{
			Node = null;

			var parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 2) return false; // name = val.

			var varName = parts[0].Trim();
			var varVal = parts[1].Trim();
			if (!VariableNameIsOk(varName))
			{
				return false;
				throw new Exception($"Variable name {parts[0]} is not allowed. Variable name must start with letter and may contain only letters or digits.");
				return true;
			}

			Node = new AssignmentNode(varName, ParseLine(varVal));

			return true;
		}

		protected readonly char[] opening = new char[] { '(', '[', '{' };
		protected readonly char[] closing = new char[] { ')', ']', '}' };

		protected int FindBalancingBracketIndex(string line, int openIndex)
		{
			if (!(opening.Contains(line[openIndex])))
				throw new ArgumentException("Opening bracket not found on index.");

			Stack<char> Brackets = new();

			for (int i = openIndex; i < line.Length; i++)
			{
				if (opening.Contains(line[i]))
				{
					Brackets.Push(line[i]);
				}
				var clbr = (closing as IList<char>).IndexOf(line[i]);
				if (clbr != -1)
				{
					var opbr = (opening as IList<char>).IndexOf(Brackets.Peek());
					if (clbr != opbr)
					{
						goto M;
					}
					else
					{
						Brackets.Pop();
					}
				}

				if (Brackets.Count == 0) return i;
			}

		M:
			throw new ArgumentException($"Cannot find balancing bracket for \'{Brackets.Peek()}\' in the expression \'{line}\'");
		}

		protected bool IsCycleOrSimpleConditionalString(string line, out INodeWithConditionAndChilds? Node)
		{
			Node = null;

			var cycleOperators = this._configuration.Keywords.Where(x => x.Type.Equals("cycleOperator") || x.Type.Equals("conditionalOperator"));
			var cycleOperator = cycleOperators.FirstOrDefault(x => line.StartsWith(x.Word));
			if (cycleOperator == null)
			{
				return false;
			}
			var conditionNodeStartIndex = line.IndexOf('(');
			var conditionNodeEndIndex = FindBalancingBracketIndex(line, conditionNodeStartIndex);

			if (conditionNodeEndIndex == -1)
			{
				throw new ArgumentException($"Unable to find bracket closing \'{cycleOperator.Word}\' operator condition.");
			}

			int childNodeStartIndex = -1;
			int childNodeEndIndex = 1;
			for (int i = conditionNodeEndIndex + 1; i < line.Length; i++)
			{
				if (line[i] == '{')
				{
					childNodeStartIndex = i;
					childNodeEndIndex = FindBalancingBracketIndex(line, childNodeStartIndex);
					break;
				}
			}

			if (childNodeEndIndex != line.Length - 1 && cycleOperator.Type.Equals("cycleOperator"))
			{
				throw new ArgumentException("There is characters left after parsing cycle childs.");
			}
			//else if (cycleOperator.Type.Equals("conditionalOperator"))
			//{
			//	return false;
			//}

			var conditionNode = ParseLine(line.Substring(conditionNodeStartIndex + 1, conditionNodeEndIndex - conditionNodeStartIndex - 1).Trim());
			var childLines = ParseNvulCode(line.Substring(childNodeStartIndex + 1, childNodeEndIndex - childNodeStartIndex - 1).Trim());

			if (cycleOperator.Type.Equals("cycleOperator"))
				Node = new CycleNode(conditionNode, childLines);
			else if (cycleOperator.Type.Equals("conditionalOperator"))
				Node = new ConditionNode(conditionNode, childLines);

			return true;
		}

		protected readonly List<NvulOperator> _operatorsByPriorityDescending;
		protected readonly StringEvaluator _operatorsEvaluator;
		protected bool IsOperatorString(string line, out OperatorNode? Node)
		{
			Node = null;

			var operatorIndex = _operatorsEvaluator.GetIndexOfFirstPriorTopOp(line, out var opLen);
			if (operatorIndex is null) return false;

			Node = new OperatorNode(
				ParseLine(line.Substring(0, operatorIndex.Value).Trim()),
				ParseLine(line.Substring(operatorIndex.Value + opLen).Trim()),
				line.Substring(operatorIndex.Value, opLen));

			return true;
		}

		protected bool IsNumericLiteralString(string line, out ILiteralNode? Node)
		{
			Node = null;

			if (long.TryParse(line, out var ivalue))
			{
				Node = new IntegerLiteral(ivalue);
				return true;
			}
			if (double.TryParse(line, out var dvalue))
			{
				Node = new FloatLiteral(dvalue);
				return true;
			}

			return false;
		}

		protected bool IsVariableRefString(string line, out VariableRefNode? Node)
		{
			Node = null;

#if DEBUG
			var t = VariableNameIsOk(line);
			var t2 = char.IsLetterOrDigit('2');
#endif

			if (VariableNameIsOk(line))
			{
				Node = new VariableRefNode(line);
				return true;
			}

			return false;
		}

		protected bool IsFunctionCallString(string line, out FunctionCallNode? Node)
		{
			Node = null;

			int firstOpenIndex = line.IndexOf('(');
			string callName = line.Substring(0, firstOpenIndex);
			int lastDotBeforeOpeningIndex = callName.LastIndexOf('.');
			bool isStaticCall = lastDotBeforeOpeningIndex == -1;

			var paramsNumber = _operatorsEvaluator.GetParametersOfFuncCall(line, out var parametersOfCall);
			var functionNameOnly = callName.Substring(lastDotBeforeOpeningIndex + 1);

			if (this._operatorsEvaluator.FunctionsSignaturesByParametersNumber.TryGetValue(1, out var funcsByParams))
			{
				if (funcsByParams.ContainsKey(functionNameOnly))
				{

				}
				else
				{
					// should never throw.
					throw new ArgumentException($"No functions exists with name \'{callName}\' and {paramsNumber} parameters.");
				}
			}
			else
			{
				throw new ArgumentException($"No functions exists with {paramsNumber} parameters.");
			}

			Node = new FunctionCallNode(isStaticCall ? null : callName.Substring(0, lastDotBeforeOpeningIndex), functionNameOnly, parametersOfCall.Select(x => ParseLine(x)).ToList());
			return true;
		}

		public void CreateRegexesForExpressions()
		{



			throw new NotImplementedException();
		}

		internal enum ExpressionTypes
		{
			DeclarationNode,
			AssignmentNode,
			AssignmentWithDeclarationNode,
			ConditionNode,
			CycleNode,
			FunctionCallNode,
			LiteralNode
		}

		public ICodeNode ParseLine(string nvulCode)
		{
			//ICodeNode? Node = null;

			if (nvulCode.StartsWith('(') && nvulCode.EndsWith(')'))
			{
				if (FindBalancingBracketIndex(nvulCode, 0) == (nvulCode.Length - 1))
					return ParseLine(nvulCode.Substring(1, nvulCode.Length - 2));
			}


			if (IsDeclarationString(nvulCode, out var declarNode))
				return declarNode!;
			if (IsAssignmentString(nvulCode, out var assignNode))
				return assignNode!;
			if (IsCycleOrSimpleConditionalString(nvulCode, out var cycleNode))
				return cycleNode!;
			if (IsOperatorString(nvulCode, out var operNode))
				return operNode!;
			if (IsNumericLiteralString(nvulCode, out var numLiteral))
				return numLiteral!;
			if (IsVariableRefString(nvulCode, out var variableRefNode))
				return variableRefNode!;
			if (IsFunctionCallString(nvulCode, out var funcCallNode))
				return funcCallNode!;


			throw new ArgumentException($"Could not determinate the type of expression \'{nvulCode}\'");
		}

		public List<IndexRange> FindTopLevelLines(string nvulCode)
		{
			var result = new List<IndexRange>();
			int lastStart = 0;
			for (int i = 0; i < nvulCode.Length; i++)
			{
				if (opening.Contains(nvulCode[i]))
				{
					i = FindBalancingBracketIndex(nvulCode, i);
				}
				else if (nvulCode[i] == ';' || i == (nvulCode.Length - 1))
				{
					result.Add(new IndexRange(lastStart, i - lastStart));
					lastStart = i + 1;
				}
			}
			return result;
		}

		public IEnumerable<ICodeNode> ParseNvulCode(string nvulCode)
		{
			List<ICodeNode> result = new List<ICodeNode>();

			foreach (var ln in FindTopLevelLines(nvulCode))
			{
				result.Add(ParseLine(nvulCode.Substring(ln.Start, ln.Count).Trim()));
			}

			return result;
		}
	}
}
