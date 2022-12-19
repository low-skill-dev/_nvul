using nvul_compiler.Models.CodeTree;
using nvul_compiler.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Services
{
	/* Данный класс хранит данные о существующей в данном контексте переменной.
	 * Имя используется для проверки существования переменной в случае ссылки на неё.
	 * Тип используется для возможности проверки использования.
	 * Последнее присвоенное значение используется для анализа возможных некорректных 
	 * выражений, вроде 'myLogic = false; if(myLogic) { ... }' - always false expression.
	 */
	internal class VariableContext
	{
		public string Name { get; protected set; }
		public string Type { get; protected set; }
		public ICodeNode? LastAssignedValue { get; set; }

		public VariableContext(string name, string type, ICodeNode? lastAssignedValue = null)
		{
			this.Name = name;
			this.Type = type;
			this.LastAssignedValue = lastAssignedValue;
		}
	}

	/* Контекст кода. При проходе по коду контекст наполняется данными о том, что было пройдено,
	 * какие переменные объявлены и т.д. При переходе на уровень вниз создается контекст, который
	 * ссылается на вышестоящий. Таким образом возможно разделение, например, переменных, объявленных
	 * вне условного оператора и объявленных внутри него тела. После выхода из блока кода "наверх", 
	 * созданный контекст уничтожается и продолжается работа с контекстом текущего уровня.
	 * При проверке существования переменной в текущем контексте в случае её использования - проверяются
	 * рекурсивно все вышестоящие контексты до первого совпадения. В случае объявления переменной
	 * просматриваются все вышестоящие контексты на предмет отсутствия дублирования.
	 */

	internal class CodeContext
	{
		public CodeContext? FatherContext { get; set; }

		public CodeContext(CodeContext? fatherContext)
		{
			VariablesContext = new();
			this.FatherContext = fatherContext;
		}

		protected List<VariableContext> VariablesContext { get; set; }

		public IEnumerable<VariableContext> GetVariablesContextsRecursively()
		{
			foreach (var varCtx in this.VariablesContext) yield return varCtx;

			if (this.FatherContext is null) yield break;

			foreach (var varCtx in this.FatherContext.GetVariablesContextsRecursively()) yield return varCtx;
		}

		public void AddVariable(string varName, string varType)
		{
			this.VariablesContext.Add(new(varName, varType));
		}
		public void AddVariable(DeclarationNode node) => this.AddVariable(node.VariableName, node.VariableType);

		public void AssignVariable(string varName, ICodeNode value)
		{
			var found = this.VariablesContext.Find(x => x.Name.Equals(varName));
			if (found is null) throw new ArgumentOutOfRangeException($"Variable with name \'{varName}\' not found.");
			found.LastAssignedValue = value;
		}

		public void AssignVariable(AssignmentNode node) => this.AssignVariable(node.VariableName, node.AssignedValue);

		[Obsolete]
		public CodeContext CreateChild() => new CodeContext(this);
	}
	internal class Analyzer
	{
		protected NvulConfiguration _configuration;

		public Analyzer(NvulConfiguration configuration)
		{
			_configuration = configuration;
		}

		/* Проверяет возможно импилисит-преобразования типа
		 */
		public bool CanBeImplicitlyConverted(string fromType, string toType)
		{
			if (fromType.Equals(toType)) return true;

			return this._configuration.Implicits
				.FirstOrDefault(x => x.Vartype.Equals(fromType) && x.ImplicitTo.Equals(toType)) is not null;
		}

		/* Получается все типы, в которые переданный тип может быть скрыто преобразован.
		 */
		public IEnumerable<string> GetAllImplicitVariants(string fromType)
		{
			return this._configuration.Implicits
				.Where(x => x.Vartype.Equals(fromType)).Select(x => x.ImplicitTo).Append(fromType);
		}

		/* Данный метод проводит валидацию типов и возвращает результирующий тип выражения.
		 * Например, выражение внутри условного оператора должно быть валидироно на
		 * возвращение типа logical (boolean у нормальных людей).
		 */
		public string? ValidateUsageAndGetResultingType(ICodeNode node, CodeContext? currentContext)
		{
			// Ноды, которые не имеют возвращаемого типа
			if (node is DeclarationNode ||
				node is AssignmentNode ||
				node is CycleNode ||
				node is ConditionNode
			)
				return null;

			// Ноды, имеющие точно известный заранее тип
			if (node is ILiteralNode)
			{
				if (node is IntegerLiteral)
					return "integer";
				if (node is FloatLiteral)
					return "float";
				if (node is BoolLiteral)
					return "logical";
				if (node is StringLiteral)
					return "charseq";
			}

			/* Возвращаемый оператором тип может зависить от его операндов.
			 * Однако данный блок также осуществляет валидацию операндов для операторов,
			 * возвращаемый тип которых заранее известен, например для оператора == (logical).
			 */
			if (node is OperatorNode)
			{
				var realNode = (OperatorNode)node;
				var leftResultingType = ValidateUsageAndGetResultingType(realNode.Left, currentContext);
				var rightResultingType = ValidateUsageAndGetResultingType(realNode.Right, currentContext);


				/* ноды-операнды не могут быть исполняемыми командами без возвращаемого значения
				 * (например нодами присвоения). В этом случае выбрасывается исключение.
				 */
				if (leftResultingType is null || rightResultingType is null)
				{
					string notEvaledError;
					if (leftResultingType is null && rightResultingType is null) notEvaledError = "(left and right)";
					else if (leftResultingType is null) notEvaledError = "(left)";
					else if (rightResultingType is null) notEvaledError = "(right)";
					else throw new Exception("This exception should be never thrown.");
					throw new ArgumentException($"One of the expressions {notEvaledError} is not a value-type and cannot be used as an operand.");
				}

				// Если удалось найти оператор, выполняющий операции с данными типами.
				var exactMatch = this._configuration.Operators.FirstOrDefault(x =>
					x.OperatorString.Equals(realNode.Operator)
					&&
					x.LeftType.Equals(leftResultingType)
					&&
					x.RightType.Equals(rightResultingType)
					);

				realNode.NvulOperator = exactMatch;
				if (exactMatch is not null)
					return exactMatch.EvaluatesTo;

				/* Если удалось найти оператор, выполняющий операции с типами, 
				 * к которым можно скрытно привести данные типы.
				 */
				var implicitMatch = this._configuration.Operators.FirstOrDefault(x =>
					x.OperatorString.Equals(realNode.Operator)
					&&
					GetAllImplicitVariants(leftResultingType).Any(t => x.LeftType.Equals(t))
					&&
					GetAllImplicitVariants(rightResultingType).Any(t => x.RightType.Equals(t))
					);


				realNode.NvulOperator = implicitMatch;
				if (implicitMatch is not null)
					return implicitMatch.EvaluatesTo;

				throw new ArgumentException($"The operator \'{realNode.Operator}\' cannot be used with operands types \'{leftResultingType}\' and \'{rightResultingType}\'");
			}

			/* Возвращаемый тип функции может зависить от конкретной её перегрузки.
			 */
			if (node is FunctionCallNode)
			{
				var realNode = (FunctionCallNode)node;
				var callParamsTypes = realNode.Arguments.Select(x => ValidateUsageAndGetResultingType(x, currentContext)).ToList();

				var foundByName = this._configuration.NvulFunctions.Where(x => x.FunctionName.Equals(realNode.FunctionName));
				var found = foundByName.FirstOrDefault(x =>
				{
					var types = (x.Arguments ?? new NvulFunctionParameters(Array.Empty<string>(), Array.Empty<string>())).ParametersTypes;

					if (callParamsTypes.Count != types.Count) return false;

					for (int i = 0; i < types.Count; i++)
					{
						if (callParamsTypes[i] == null || !CanBeImplicitlyConverted(callParamsTypes[i]!, types[i]))
							return false;
					}

					return true;
				});

				//var found = this._configuration.NvulFunctions.Where(x=> x.FunctionName.Equals(realNode.FunctionName)).FirstOrDefault(x => 
				//	(x.Arguments ?? new NvulFunctionParameters(Array.Empty<string>(), Array.Empty<string>())).ParametersTypes.SequenceEqual(callParamsTypes));

				if (found is null)
				{
					throw new ArgumentException($"There is no function exists with name \'{realNode.FunctionName}\' and parameters types [{string.Join(",", callParamsTypes)}].");
				}

				realNode.NvulFunction = found;

				return found.EvaluatesTo;
			}

			/* Тип, возвращаемый ссылкой на переменную может быть определен только из контекста, который хранит
			 * данные о том, как была объявлена данная перменная.
			 */
			if (node is VariableRefNode)
			{
				if (currentContext is null)
				{
					throw new ArgumentNullException("The code context should be not null while parsing variable reference node.");
				}

				var realNode = (VariableRefNode)node;
				var found = currentContext.GetVariablesContextsRecursively().FirstOrDefault(x => x.Name.Equals(realNode.VariableName));

				if (found is null)
				{
					throw new ArgumentException($"Variable with name \'{((VariableRefNode)node).VariableName}\' was not declared.");
				}

				if (found.LastAssignedValue is null)
				{
					throw new NullReferenceException($"You are trying to use a variable with name \'{found.Name}\' which was not assigned.");
				}

				return found.Type;
			}

			throw new NotImplementedException($"The passed code node has an unkown type: {((object)node).GetType().FullName}.");
		}


		// try not to use this! very slow if too much calls
		[Obsolete]
		public bool CanBeConvertedToTheType(ICodeNode node, string targetType, CodeContext? context = null)
		{
			var resultingType = ValidateUsageAndGetResultingType(node, context);
			if (resultingType is null)
			{
				throw new ArgumentException("Passed node has no resulting type, so cannot be converted to any.");
			}
			return GetAllImplicitVariants(resultingType).Any(x => x.Equals(targetType));
		}

		/* С учетом уже написанного кода, большУю часть валидации осуществляет GetResultingType.
		 * Данный метод должен работать только с основной сутью выражения, обновляя контекст и 
		 * делигируя дальнейшую валидацию. Лучше не использовать здесь рекурсию, иначе ломается
		 * суть валидации одного дискретного куска кода.
		 */
		public bool AnalyzeNodeAndUpdateContext(ICodeNode node, CodeContext context)
		{
			if (node is null) throw new ArgumentNullException("Internal analyzer error."); // should be never thrown

			if (node is DeclarationNode)
			{
				var realNode = (DeclarationNode)node;
				if (context.GetVariablesContextsRecursively().Any(x => x.Name.Equals(realNode.VariableName)))
				{
					throw new ArgumentException($"Variable with name \'{realNode.VariableName}\' was already declared.");
				}
				context.AddVariable(realNode);

				return true;
			}
			if (node is AssignmentNode)
			{
				var realNode = (AssignmentNode)node;
				var found = context.GetVariablesContextsRecursively().FirstOrDefault(x => x.Name.Equals(realNode.VariableName));

				if (found is null)
				{
					throw new ArgumentException($"Cannot assign variable with name \'{realNode.VariableName}\' because it was not declared.");
				}

				var assignedType = ValidateUsageAndGetResultingType(realNode.AssignedValue, context);

				if (assignedType is null)
				{
					throw new ArgumentException($"You are trying to assign non-value type to the variable with name \'{found.Name}\'");
				}

				if (found.Type != assignedType && !GetAllImplicitVariants(assignedType).Any(x => x.Equals(found.Type)))
				{
					throw new ArgumentException($"You are trying to assign value of type \'{assignedType}\' to the variable with name \'{realNode.VariableName}\' " +
						$"and type {found.Type}. No implicit conversion exists.");
				}

				found.LastAssignedValue = realNode.AssignedValue;

				return true;
			}
			if (node is INodeWithConditionAndChilds)
			{
				var realNode = (INodeWithConditionAndChilds)node;

				var conditionResultingType = ValidateUsageAndGetResultingType(realNode.Condition, context);
				if (conditionResultingType is null)
				{
					throw new ArgumentException("You are trying to use non-value expression as the condition.");
				}
				if (!conditionResultingType.Equals("logical"))
				{
					throw new ArgumentException("You are trying to use non-logical-value expression as the condition.");
				}

				var childs = realNode.Childs;

				AnalyzeNvulNodes(childs, context);

				return true;
			}

			if (node is FunctionCallNode)
			{
				ValidateUsageAndGetResultingType(node, context);

				return true;
			}

			if (node is ILiteralNode)
			{
				throw new ArgumentException("Literals cannot exists as a self parts of the code.");
			}

			if (node is OperatorNode)
			{
				throw new ArgumentException("Operators cannot exists as a self parts of the code.");
			}

			if (node is VariableRefNode)
			{
				throw new ArgumentException("Variable references cannot exists as a self parts of the code.");
			}

			throw new NotImplementedException($"The passed code node has an unkown type: {((object)node).GetType().FullName}.");
		}

		public bool AnalyzeNvulNodes(IEnumerable<ICodeNode> nodes, CodeContext? fatherContext = null)
		{
			CodeContext codeCtx = new(fatherContext);

			var result = true;

			foreach (var node in nodes) result = result && AnalyzeNodeAndUpdateContext(node, codeCtx);

			return result;
		}
	}
}
