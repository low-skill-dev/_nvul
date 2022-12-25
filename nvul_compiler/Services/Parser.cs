using Newtonsoft.Json;
using nvul_compiler.Models.CodeTree;
using nvul_compiler.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("nvul_compiler.tests")]
namespace nvul_compiler.Services
{
	/* Задача парсера - построить дерево кода.
	 * В рамках принципа единственной ответственности осуществляется 
	 * вынужденное частичное дублирование функционала анализатора - 
	 * в части проверки объявления переменной, но для других целей.
	 */
	public sealed class Parser
	{
		/* Структура служит для выделения последовательности символов в строке.
		 */
		internal struct IndexRange
		{
			public readonly int Start;
			public readonly int Count;
			public int End => Start + Count;

			public IndexRange(int start, int count)
			{
				this.Start = start;
				this.Count = count;
			}
		}

		public const char LineDelimiter = ';';
		internal readonly NvulConfiguration _configuration;

		internal readonly List<NvulKeyword> _varTypes;
		internal readonly List<NvulKeyword> _cycleAndConditionalOperators;
		internal readonly List<NvulOperator> _operatorsByPriorityDescending;
		internal readonly StringEvaluator _operatorsEvaluator;

		public Parser(NvulConfiguration configuration)
		{
			this._configuration = configuration;

			/* Для парсинга операторов и функций используется функционал ранее написанного класса,
			 * не предназначенного для этого изначально. В идеальном случае требуется написать фасад.
			 * Но у нас нет времени на реализацию паттернов. Технический долг не достигнет критических
			 * значений до выкидывания данного проекта.
			 */
			this._operatorsByPriorityDescending = this._configuration.Operators.OrderByDescending(x => x.OperatorPriority).ToList();
			this._operatorsEvaluator = new(false, false);
			this._operatorsByPriorityDescending.ForEach(x => { try { this._operatorsEvaluator.AddOperator(x.OperatorString, (x, y) => 0, x.OperatorPriority); } catch { } });
			this._configuration.NvulFunctions.ToList().ForEach(x => this._operatorsEvaluator.AddFunction(x.FunctionName, (y) => 0));

			// Некоторые коллекции энумеруются заранее для устранения множественного повторения данного процесса.
			this._varTypes = this._configuration.Keywords.Where(x => x.Type.Equals("vartype")).ToList();
			this._cycleAndConditionalOperators = this._configuration.Keywords
				.Where(x => x.Type.Equals("cycleOperator") || x.Type.Equals("conditionalOperator")).ToList();
		}

		// Имена перменных начинаются на букву, продолжаются на букву или цифру.
		internal bool VariableNameIsOk(string varname) => !string.IsNullOrEmpty(varname)
			&& char.IsLetter(varname[0])
			&& varname.Skip(1).All(x => char.IsLetterOrDigit(x));

		// Строка соотвествует синтаксису декларации 'type name;'.
		internal bool IsDeclarationString(string line, out DeclarationNode? Node, IList<string>? declaredNames = null)
		{
			Node = null;

			var parts = line.Split(' ', StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 2) return false; // type name;

			var varType = parts[0];
			var varName = parts[1];

			var foundType = _varTypes.FirstOrDefault(t => varType.Equals(t.Word));
			if (foundType is null) return false;

			if (!VariableNameIsOk(varName))
				throw new Exception($"Variable name {varName} is not allowed. Variable name must start with letter and may contain only letters or digits.");

			Node = new DeclarationNode(varType, varName) { NvulKeyword = foundType };
			declaredNames?.Add(varName);
			return true;
		}

		// Строка соответствует синтаксису присвоения 'name = value;'.
		internal bool IsAssignmentString(string line, out AssignmentNode? Node, IList<string>? declaredNames = null)
		{
			Node = null;

			var indexOfOp = line.IndexOf('=');
			// Удостовериться, что не существует оператора, с таким же началом, например == (оператор сравнения).
			if (indexOfOp == -1 || this._configuration.Operators.Any(x => _operatorsEvaluator.isOnThisIndex(line, x.OperatorString, indexOfOp)))
				return false;

			var parts = line.Split('=', 2, StringSplitOptions.RemoveEmptyEntries);
			if (parts.Length != 2) return false; // name = value;

			var varName = parts[0].Trim();
			var varVal = parts[1].Trim();

			if (!VariableNameIsOk(varName))
				return false;

			Node = new AssignmentNode(varName, ParseLine(varVal, declaredNames));
			return true;
		}

		internal readonly char[] opening = new char[] { '(', '[', '{' };
		internal readonly char[] closing = new char[] { ')', ']', '}' };
		internal int FindBalancingBracketIndex(string line, int openIndex)
		{
			if (!(opening.Contains(line[openIndex])))
				throw new ArgumentException("Opening bracket not found on index.");

			Stack<char> Brackets = new();
			for (int i = openIndex; i < line.Length; i++)
			{
				if (opening.Contains(line[i]))
				{
					Brackets.Push(line[i]);
					continue;
				}
				var closingBrId = (closing as IList<char>).IndexOf(line[i]);
				if (closingBrId != -1)
				{
					var openingBrId = (opening as IList<char>).IndexOf(Brackets.Peek());
					if (closingBrId != openingBrId) break;
					else Brackets.Pop();
					if (Brackets.Count == 0) return i;
				}
			}

			throw new ArgumentException($"Cannot find balancing bracket for \'{Brackets.Peek()}\' in the expression \'{line}\'");
		}

		// Строка соответствует синтаксису 'operator(condition){childs}'.
		internal bool IsCycleOrSimpleConditionalString(string line, out INodeWithConditionAndChilds? Node, IList<string>? declaredNames = null)
		{
			Node = null;

			var cycleOperator = this._cycleAndConditionalOperators.FirstOrDefault(x => line.StartsWith(x.Word));
			if (cycleOperator is null) return false;
			
			var conditionNodeStartIndex = line.IndexOf('(');
			var conditionNodeEndIndex = FindBalancingBracketIndex(line, conditionNodeStartIndex);

			if (conditionNodeEndIndex == -1)
				throw new ArgumentException($"Unable to find bracket closing \'{cycleOperator.Word}\' operator condition.");
		
			int childNodeStartIndex = -1;
			int childNodeEndIndex = -1;
			for (int i = conditionNodeEndIndex + 1; i < line.Length; i++)
			{
				if (line[i] == '{')
				{
					childNodeStartIndex = i;
					childNodeEndIndex = FindBalancingBracketIndex(line, childNodeStartIndex);
					break;
				}
			}

			if (childNodeStartIndex==-1 || childNodeEndIndex==-1)
			{
				throw new ArgumentException("Unable to find child code block after condition. It must appear in \'operator(condition){...childs}\' format.");
			}

			var conditionNode = ParseLine(line.Substring(conditionNodeStartIndex + 1, conditionNodeEndIndex - conditionNodeStartIndex - 1).Trim(), declaredNames);
			var childLines = ParseNvulCode(line.Substring(childNodeStartIndex + 1, childNodeEndIndex - childNodeStartIndex - 1).Trim(), declaredNames);

			if (cycleOperator.Type.Equals("cycleOperator"))
				Node = new CycleNode(conditionNode, childLines);
			else if (cycleOperator.Type.Equals("conditionalOperator"))
				Node = new ConditionNode(conditionNode, childLines);
			else // should be never thrown but added for the future
				throw new NotImplementedException($"Unknown operator type \'{cycleOperator.Type}\'.");


			Node.NvulKeyword = cycleOperator;
			return true;
		}

		// Строка соотвествует синтаксису 'val1 operator val2'.
		internal bool IsOperatorString(string line, out OperatorNode? Node, IList<string>? declaredNames = null)
		{
			Node = null;

			// Без сортировки по приоритету возникает ошибка с неочевидной причиной. Легаси код не будет правится.
			var operatorsIndexes = _operatorsEvaluator.GetIndexesOfTopLevelOperators(line, SortByPriority: true);
			if (!operatorsIndexes.Any()) return false;
			var startPrior = operatorsIndexes.First().Operation.Priority;

			// Взять крайний справа оператор, приоритет которого соответствует
			// минимальному в данном выражении (- - * + /) - взять +
			var foundOp = operatorsIndexes.Where(x => x.Operation.Priority == startPrior).MaxBy(x => x.Index);
			var operatorIndex = foundOp.Index;
			var opLen = foundOp.Operation.OperationString.Length;
			try
			{
				Node = new OperatorNode(
					ParseLine(line.Substring(0, operatorIndex).Trim(), declaredNames),
					ParseLine(line.Substring(operatorIndex + opLen).Trim(), declaredNames),
					line.Substring(operatorIndex, opLen));
			}
			catch(Exception ex)
			{
				throw new ArgumentException($"Operands of \'{line.Substring(operatorIndex, opLen)}\' parsing problem: {ex.Message}.");
			}
			return true;
		}

		internal bool IsNumericLiteralString(string line, out ILiteralNode? Node)
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
		internal bool IsBooleanLiteralString(string line, out BoolLiteral? Node)
		{
			Node = null;

			if (line.Equals("true") || line.Equals("истина"))
			{
				Node = new BoolLiteral(true);
				return true;
			}
			if (line.Equals("false") || line.Equals("ложь"))
			{
				Node = new BoolLiteral(false);
				return true;
			}

			return false;
		}

		internal bool IsVariableRefString(string line, out VariableRefNode? Node)
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

		internal bool IsFunctionCallString(string line, out FunctionCallNode? Node, IList<string>? declaredNames)
		{
			Node = null;

			int firstOpenIndex = line.IndexOf('(');
			if (firstOpenIndex == -1) return false;

			string callName = line.Substring(0, firstOpenIndex);
			int firstDotIndex = callName.IndexOf('.');
			string functionName = null!;
			string? variableName = null;
			if (firstDotIndex == -1)
			{
				functionName = callName;
			}
			else
			{
				var callerVarName = callName.Substring(0, firstDotIndex);
				var foundCaller = declaredNames?.FirstOrDefault(name => name.Equals(callerVarName));

				if (foundCaller is null)
				{
					functionName = callName;
				}
				else
				{
					variableName = foundCaller;
					functionName = callName.Substring(firstDotIndex + 1);
				}
			}

			var callParams = _operatorsEvaluator.GetParametersOfFuncCall(line, out var parametersOfCall);

			// 1 parameter is a constant for legacy code support
			if (this._operatorsEvaluator.FunctionsSignaturesByParametersNumber.TryGetValue(1, out var funcsByParams))
			{
				if (funcsByParams.ContainsKey(functionName))
				{

				}
				else
				{
					throw new ArgumentException($"No functions exists with name \'{callName}\' and {callParams} parameters.");
				}
			}
			else
			{
				throw new ArgumentException($"No functions exists with {callParams} parameters.");
			}

			Node = new FunctionCallNode(variableName, functionName, parametersOfCall.Select(x => ParseLine(x, declaredNames)).ToList());
			return true;
		}

		public ICodeNode ParseLine(string nvulCode, IList<string>? declaredNames = null)
		{
			if (nvulCode.StartsWith('(') && nvulCode.EndsWith(')'))
			{
				if (FindBalancingBracketIndex(nvulCode, 0) == (nvulCode.Length - 1))
					return ParseLine(nvulCode.Substring(1, nvulCode.Length - 2), declaredNames);
			}

			// Порядок проверки важен
			if (IsDeclarationString(nvulCode, out var declarNode, declaredNames))
				return declarNode!;
			if (IsCycleOrSimpleConditionalString(nvulCode, out var cycleNode, declaredNames))
				return cycleNode!;
			if (IsAssignmentString(nvulCode, out var assignNode, declaredNames))
				return assignNode!;
			if (IsOperatorString(nvulCode, out var operNode, declaredNames))
				return operNode!;
			if (IsNumericLiteralString(nvulCode, out var numLiteral))
				return numLiteral!;
			if (IsBooleanLiteralString(nvulCode, out var boolLiteral))
				return boolLiteral!;
			if (IsVariableRefString(nvulCode, out var variableRefNode))
				return variableRefNode!;
			if (IsFunctionCallString(nvulCode, out var funcCallNode, declaredNames))
				return funcCallNode!;

			throw new ArgumentException($"Could not determinate the type of expression \'{nvulCode}\'");
		}


		// 
		internal IEnumerable<IndexRange> FindTopLevelLines(string nvulCode)
		{
			var result = new List<IndexRange>();
			int lastStart = 0;
			int i = 0;
			for (; i < nvulCode.Length; i++)
			{
				if (opening.Contains(nvulCode[i]))
				{
					i = FindBalancingBracketIndex(nvulCode, i);
				}
				else if (nvulCode[i] == LineDelimiter)
				{
					yield return new IndexRange(lastStart, i - lastStart);
					lastStart = i + 1;
				}
			}

			if (nvulCode.Last() != LineDelimiter)
			{
				yield return new IndexRange(lastStart, nvulCode.Length - lastStart);
			}

			yield break;
		}

		public List<ICodeNode> ParseNvulCode(string nvulCode, IList<string>? declaredNames = null)
		{
			if (string.IsNullOrEmpty(nvulCode)) return Array.Empty<ICodeNode>().ToList();

			List<ICodeNode> result = new();

			var enumer = GetParsingEnumerator(nvulCode, declaredNames);

			while (enumer.MoveNext()) result.Add(enumer.Current);

			return result;
		}

		internal IEnumerable<ICodeNode> ParseNvulCodeYield(string nvulCode, IList<string>? declaredNames = null)
		{
			var enumer = GetParsingEnumerator(nvulCode, declaredNames);

			while (enumer.MoveNext()) yield return enumer.Current;
		}

		public IEnumerator<ICodeNode> GetParsingEnumerator(string nvulCode, IList<string>? declaredNames = null)
		{
			if (string.IsNullOrEmpty(nvulCode)) yield break;

			if (declaredNames is null) declaredNames = new List<string>();

			ICodeNode t;
			foreach (var ln in FindTopLevelLines(nvulCode))
			{
				try
				{
					t = ParseLine(nvulCode.Substring(ln.Start, ln.Count).Trim(), declaredNames);
				}
				catch(Exception ex)
				{
					throw new ArgumentException(ex.Message);
				}
				t.InFileCharIndex = ln.Start;
				yield return t;
			}

			yield break;
		}

	}
}
