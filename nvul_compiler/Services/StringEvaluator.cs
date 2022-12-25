using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace nvul_compiler.Services
{
	/* В выражении могут быть использованы целые числа, вещественные числа, арифметические операторы: +,-,*,/, 
	* скобки (), а также функции и операторы в соответствии с вариантом. 
	* 18.   sin
	*/
	internal class StringEvaluator
	{
		internal struct PrioritizedOperation
		{
			public string OperationString;
			public int Priority;
		}
		internal struct PrioritizedOperationOnIndex
		{
			public PrioritizedOperation Operation;
			public int Index;

			public bool Equals(PrioritizedOperationOnIndex other)
			{
				return (
						Index.Equals(other.Index)
						&&
						Operation.OperationString.Equals(other.Operation.OperationString)
						&&
						Operation.Priority.Equals(other.Operation.Priority)
					);
			}
		}
		internal struct PrioritizedOperator
		{
			public string OperatorString;
			public int Priority;

			public PrioritizedOperator(string Operator, int Priority)
			{
				this.OperatorString = Operator;
				this.Priority = Priority;
			}
		}
		internal struct ExpressionIndex
		{
			public int Start;
			public int Count;
		}


		internal Dictionary<int, Dictionary<string, Delegate>> FunctionsSignaturesByParametersNumber = new();
		//internal Dictionary<string, Func<double, double>> FunctionsSignatures1 = new(); // 1 param
		//internal Dictionary<string, Func<double, double, double>> FunctionsSignatures2 = new(); // 2 params
		//internal Dictionary<string, Func<double, double, double, double>> FunctionsSignatures3 = new(); // 3 params
		internal Dictionary<string, Func<double, double, double>> OperatorsSignatures = new();
		internal List<PrioritizedOperator> OperatorsByPriorityDescending = new();

		internal Regex isNumberRegex = new Regex(@"^[-]?\d+(\.\d+)?$");


		internal Regex isFunctionCallRegex;
		internal Regex currentFunctionCallRegex => new Regex(@"^((\A|\))[-])?({ALLOWEDFUNCS})[\(](\n|.?)+[\)]$".Replace("{ALLOWEDFUNCS}",
				string.Join('|', FunctionsSignaturesByParametersNumber.Values.SelectMany(v => v.Keys).ToList())));

		public StringEvaluator(bool addDefaultFunctions = true, bool addDefaultOperators = true)
		{
			Debug.WriteLine($"Constructor of {nameof(StringEvaluator)} is being called.");

			if (addDefaultFunctions)
			{
				FunctionsSignaturesByParametersNumber.Add(1, new());
				FunctionsSignaturesByParametersNumber.Add(2, new());

				FunctionsSignaturesByParametersNumber[1].Add("sin", Math.Sin);
				FunctionsSignaturesByParametersNumber[1].Add("cos", Math.Cos);
				FunctionsSignaturesByParametersNumber[1].Add("tan", Math.Tan);
				FunctionsSignaturesByParametersNumber[1].Add("log10", Math.Log10);
				FunctionsSignaturesByParametersNumber[1].Add("log2", Math.Log2);
				FunctionsSignaturesByParametersNumber[1].Add("ln", new Func<double, double>(x => Math.Log(x)));

				FunctionsSignaturesByParametersNumber[2].Add("pow", Math.Pow);
			}
			if (addDefaultOperators)
			{
				OperatorsSignatures.Add("^^", (x, y) => Math.Pow(x, y));
				OperatorsSignatures.Add("/", (x, y) => x / y);
				OperatorsSignatures.Add("*", (x, y) => x * y);
				OperatorsSignatures.Add("-", (x, y) => x - y);
				OperatorsSignatures.Add("+", (x, y) => x + y);

				OperatorsByPriorityDescending.Add(new("^^", 10));
				OperatorsByPriorityDescending.Add(new("/", 5));
				OperatorsByPriorityDescending.Add(new("*", 5));
				OperatorsByPriorityDescending.Add(new("-", 1));
				OperatorsByPriorityDescending.Add(new("+", 1));
			}

			isFunctionCallRegex = currentFunctionCallRegex;
		}

		private void UpdateFuncsRegex()
		{
			isFunctionCallRegex = currentFunctionCallRegex;
		}

		public void AddFunction(string funcName, Func<double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(0))
				FunctionsSignaturesByParametersNumber.Add(0, new());
			FunctionsSignaturesByParametersNumber[0].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(1))
				FunctionsSignaturesByParametersNumber.Add(1, new());
			FunctionsSignaturesByParametersNumber[1].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(2))
				FunctionsSignaturesByParametersNumber.Add(2, new());
			FunctionsSignaturesByParametersNumber[2].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double, double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(3))
				FunctionsSignaturesByParametersNumber.Add(3, new());
			FunctionsSignaturesByParametersNumber[3].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double, double, double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(4))
				FunctionsSignaturesByParametersNumber.Add(4, new());
			FunctionsSignaturesByParametersNumber[4].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double, double, double, double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(5))
				FunctionsSignaturesByParametersNumber.Add(5, new());
			FunctionsSignaturesByParametersNumber[5].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double, double, double, double, double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(6))
				FunctionsSignaturesByParametersNumber.Add(6, new());
			FunctionsSignaturesByParametersNumber[6].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double, double, double, double, double, double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(7))
				FunctionsSignaturesByParametersNumber.Add(7, new());
			FunctionsSignaturesByParametersNumber[7].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double, double, double, double, double, double, double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(8))
				FunctionsSignaturesByParametersNumber.Add(8, new());
			FunctionsSignaturesByParametersNumber[8].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double, double, double, double, double, double, double, double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(9))
				FunctionsSignaturesByParametersNumber.Add(9, new());
			FunctionsSignaturesByParametersNumber[9].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double, double, double, double, double, double, double, double, double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(10))
				FunctionsSignaturesByParametersNumber.Add(10, new());
			FunctionsSignaturesByParametersNumber[10].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double, double, double, double, double, double, double, double, double, double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(11))
				FunctionsSignaturesByParametersNumber.Add(11, new());
			FunctionsSignaturesByParametersNumber[11].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double, double, double, double, double, double, double, double, double, double, double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(12))
				FunctionsSignaturesByParametersNumber.Add(12, new());
			FunctionsSignaturesByParametersNumber[12].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(13))
				FunctionsSignaturesByParametersNumber.Add(13, new());
			FunctionsSignaturesByParametersNumber[13].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(14))
				FunctionsSignaturesByParametersNumber.Add(14, new());
			FunctionsSignaturesByParametersNumber[14].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(15))
				FunctionsSignaturesByParametersNumber.Add(15, new());
			FunctionsSignaturesByParametersNumber[15].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}
		public void AddFunction(string funcName, Func<double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double, double> calcFunction)
		{
			if (!FunctionsSignaturesByParametersNumber.ContainsKey(16))
				FunctionsSignaturesByParametersNumber.Add(16, new());
			FunctionsSignaturesByParametersNumber[16].Add(funcName, calcFunction);
			UpdateFuncsRegex();
		}



		/// <summary>
		/// The default operators priority is: <br/>
		/// '^^' = 10, <br/>FASZZZ
		/// '/' = '*' = 5, <br/>
		/// '-' = '+' = 1.
		/// </summary>
		public void AddOperator(string operatorString, Func<double, double, double> calcFunction, int operatorPriority)
		{
			OperatorsSignatures.Add(operatorString, calcFunction);
			OperatorsByPriorityDescending.Add(new(operatorString, operatorPriority));
			OperatorsByPriorityDescending = OperatorsByPriorityDescending.OrderByDescending(x => x.Priority).ToList();
		}

		internal bool isOnThisIndex(string orig, string search, int startIndex)
		{
			if ((orig.Length-1) < (startIndex + search.Length)) return false;

			for (int i = 0; i < search.Length; i++)
				if (orig[startIndex + i] != search[i]) return false;

			return true;
		}
		internal IEnumerable<PrioritizedOperationOnIndex> GetIndexesOfTopLevelOperators(string s, List<PrioritizedOperator> OrderedByPriority = null!, bool SortByPriority = true)
		{
			if (OrderedByPriority is null) OrderedByPriority = OperatorsByPriorityDescending;
			var OrderedByPriorityThenByLength = OrderedByPriority.OrderByDescending(x => x.OperatorString.Length).ToList();
			List<PrioritizedOperationOnIndex> indexesOfOperators = new();
			int bracketsOpened = 0;
			for (int i = 0; i < s.Length; i++)
			{
				if (s[i] == '(')
				{
					bracketsOpened++;
				}
				else if (s[i] == ')')
				{
					bracketsOpened--;
				}
				else if (bracketsOpened == 0)
				{
					var found = OrderedByPriorityThenByLength.Find(op => isOnThisIndex(s, op.OperatorString, i)).OperatorString;
					if (found is not null)
					{
						if (found == "-" && (i == 0 || s[i - 1] == '(')) continue;
						indexesOfOperators.Add(new PrioritizedOperationOnIndex
						{

							Operation = new()
							{
								OperationString = found,
								Priority = OrderedByPriority.Find(x => x.OperatorString.Equals(found)).Priority
							},
							Index = i
						});
						i += found.Length - 1;
					}
				}
			}

#if DEBUG
			var t = indexesOfOperators.OrderByDescending(x => x.Operation.Priority).ToList();
#endif

			if (SortByPriority)
				return indexesOfOperators.OrderByDescending(x => x.Operation.Priority);
			else
				return indexesOfOperators;
		}

		internal int? GetIndexOfFirstPriorTopOp(string s, out int opLen)
		{
			var t = GetIndexesOfTopLevelOperators(s, SortByPriority: true).ToList();
			if (t.Any())
			{
				opLen = t.First().Operation.OperationString.Length;
				return t.First().Index;
			}
			else
			{
				opLen = -1;
				return null;
			}
		}

		[Obsolete]
		internal void GetExprsForOpAt(string s, int opIndex, int opLen, out string Left, out string Right)
		{
			getIndexesOfExprs(s, opIndex, opLen, out var exprLeft, out var exprRight);
			Left = s.Substring(exprLeft.Start, exprLeft.Count);
			Right = s.Substring(exprRight.Start, exprRight.Count);
			return;
		}
		internal void GetExprsForOpAt(string s, int opIndex, int opLen, out string Left, out string Right, out ExpressionIndex LeftIndex, out ExpressionIndex RightIndex)
		{
			getIndexesOfExprs(s, opIndex, opLen, out LeftIndex, out RightIndex);
			Left = s.Substring(LeftIndex.Start, LeftIndex.Count);
			Right = s.Substring(RightIndex.Start, RightIndex.Count);
			return;
		}
		internal void getIndexesOfExprs(string s, int opIndex, int opLen, out ExpressionIndex Left, out ExpressionIndex Right)
		{
			int? leftClosestOp = null, rightClosestOp = null;
			int? leftClosestOpLen = null, rightClosestOpLen = null;
			var topLevelOps = GetIndexesOfTopLevelOperators(s);
			if (topLevelOps.Any(x => x.Index < opIndex))
			{
				var t = topLevelOps.Where(x => x.Index < opIndex).MaxBy(x => x.Index);

				// if the prev number contains minus in the beginnin
				if (t.Operation.OperationString.Equals("-")
					&& topLevelOps.Any(x => x.Equals(topLevelOps.Where(x => x.Index < opIndex).OrderByDescending(x => x.Index).Skip(1).Take(1))))
					t = topLevelOps.Where(x => x.Index < opIndex).OrderByDescending(x => x.Index).Skip(1).First();

				leftClosestOp = t.Index;
				leftClosestOpLen = t.Operation.OperationString.Length;
			}
			var rightSearchStart = s[opIndex + opLen] == '-' ? (opIndex + opLen + 1) : (opIndex + opLen);
			if (topLevelOps.Any(x => x.Index >= rightSearchStart))
			{
				var t = topLevelOps.Where(x => x.Index >= rightSearchStart).MinBy(x => x.Index);
				rightClosestOp = t.Index;
				rightClosestOpLen = t.Operation.OperationString.Length;
			}

			if (leftClosestOp.HasValue && leftClosestOpLen.HasValue)
				Left = new ExpressionIndex() { Start = leftClosestOp.Value + leftClosestOpLen.Value, Count = opIndex - leftClosestOp.Value - leftClosestOpLen.Value };
			else
				Left = new ExpressionIndex() { Start = 0, Count = opIndex };

			if (rightClosestOp.HasValue && rightClosestOpLen.HasValue)
				Right = new ExpressionIndex() { Start = opIndex + opLen, Count = rightClosestOp.Value - (opIndex + opLen) };
			else
				Right = Right = new ExpressionIndex() { Start = opIndex + opLen, Count = s.Length - (opIndex + opLen) };
		}
		internal int GetParametersOfFuncCall(string s, out List<string> parameters)
		{
			int count = 0;
			var indexOfBracket = s.IndexOf('(');
			int bracketsOpened = 0;
			int prevCommaIndex = indexOfBracket;

			parameters = new();

			if (s[prevCommaIndex + 1] == ')') return 0;

			for (int i = indexOfBracket + 1; i < (s.Length - 1); i++)
			{
				if (s[i] == '(')
				{
					bracketsOpened++;
				}
				else if (s[i] == ')')
				{
					bracketsOpened--;
				}
				else if (bracketsOpened == 0 && s[i] == ',')
				{
					parameters.Add(s.Substring(prevCommaIndex + 1, (i - prevCommaIndex - 1)));
					prevCommaIndex = i;
					count++;
				}
			}
			parameters.Add(s.Substring(prevCommaIndex + 1, (s.Length - prevCommaIndex - 2)));
			count++;
			return count;
		}

		// 1. константа? - вернуть константу
		// 2. есть операторы? разбить по операторам
		// 3. выражение в боковых скобках? - убрать их
		// 4. вызов функции? - вызвать фунцкии, повторить вызов для параметра
		// Порядок выбора случая важен !!!

#if DEBUG
		internal static ulong callNumber = 0;
		internal static string format = new String('0', ulong.MaxValue.ToString().Length);
#endif
		public double Eval2(string s)
		{
#if DEBUG
			Debug.WriteLine($"CALL #{callNumber++.ToString(format)}: evaluation of string \'{s}\'.");
#endif

			if (new Regex(@"\s").IsMatch(s)) throw new ArgumentException("White-spaces is not allowed in the expression");

			if (isNumberRegex.IsMatch(s)) return double.Parse(s);

			var opToCalc = GetIndexOfFirstPriorTopOp(s, out var len);
			if (opToCalc is not null)
			{
				GetExprsForOpAt(s, opToCalc.Value, len, out var left, out var right, out var leftReplace, out var rightReplace);
				var operation = s.Substring(opToCalc.Value, len);

				if (!OperatorsSignatures.TryGetValue(operation, out var myFunc))
					throw new ArgumentException("Unknown operator found.");

				double evaluated = myFunc(Eval2(left), Eval2(right));
#if DEBUG
				Debug.WriteLine($"CALL #{callNumber.ToString(format)}: replacing \'{s.Substring(leftReplace.Start, leftReplace.Count + rightReplace.Count + len)}\' " +
					$"with \'{evaluated.ToString("F99")}\'.");
#endif
				return evaluated is double.NaN ? evaluated :
					Eval2(s.Remove(leftReplace.Start, leftReplace.Count + rightReplace.Count + len).Insert(leftReplace.Start, evaluated.ToString()));
			}

			if (s.StartsWith("-"))
			{
				return Eval2(s.Insert(1, "1*"));
			};

			if (s.StartsWith('(') && s.EndsWith(')'))
			{
				return Eval2(s.Substring(1, s.Length - 2));
			};

			if (isFunctionCallRegex.IsMatch(s))
			{
				var indexOfBracket = s.IndexOf('(');
				var funcName = s.Substring(0, indexOfBracket);
				var numOfParams = GetParametersOfFuncCall(s, out var parameters);

				bool negative = false;
				if (funcName.StartsWith('-'))
				{
					funcName = funcName.Substring(1);
					negative = true;
				}

				if (!FunctionsSignaturesByParametersNumber[numOfParams].TryGetValue(funcName, out var func))
				{
					throw new ArgumentException($"Unknown function \'{funcName}\' with {numOfParams} parameters.");
				}
				else
				{
					object[] paramsToMethod = parameters.Select(p => (object)Eval2(p)).ToArray();
					// too many code required for doing this without dynamic
					return (negative ? -1 : 1) * (double)func.DynamicInvoke(paramsToMethod)!;
				}
			}

			throw new ArgumentException($"Could not determinate the type of expression \'{s}\'.");
		}
	}
}

