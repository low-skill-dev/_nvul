using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestPlatform.Utilities;
using nvul_compiler;
using nvul_compiler.Models.Configuration;
using nvul_compiler.Services;
using Xunit.Abstractions;
using nvul_compiler.Models;
using nvul_compiler.Models.CodeTree;
using Newtonsoft.Json;

namespace nvul_compiler.tests.Services
{
	//public class TypeNameSerializationBinder : SerializationBinder
	//{
	//	public string TypeFormat { get; private set; }

	//	public TypeNameSerializationBinder(string typeFormat)
	//	{
	//		TypeFormat = typeFormat;
	//	}

	//	public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
	//	{
	//		assemblyName = null;
	//		typeName = serializedType.Name;
	//	}

	//	public override Type BindToType(string assemblyName, string typeName)
	//	{
	//		var resolvedTypeName = string.Format(TypeFormat, typeName);
	//		return Type.GetType(resolvedTypeName, true);
	//	}
	//}


	public class ParserTests
	{
		//internal TypeNameSerializationBinder binder = new("ConsoleApplication.{0}, ConsoleApplication");

		internal NvulKeyword[] nvulKeywords;
		internal NvulOperator[] nvulOperators;
		internal NvulFunction[] nvulFunctions;
		internal NvulConfiguration nvulConfiguration;
		internal Parser parser;

		internal ITestOutputHelper _output;
		public ParserTests(ITestOutputHelper output)
		{
			_output = output;

			this.nvulKeywords = Utf8Json.JsonSerializer.Deserialize<NvulKeyword[]>(File.ReadAllText("./Configuration/nvulKeywords.json"));
			this.nvulOperators = Utf8Json.JsonSerializer.Deserialize<NvulOperator[]>(File.ReadAllText("./Configuration/nvulOperators.json"));
			this.nvulFunctions = Utf8Json.JsonSerializer.Deserialize<NvulFunction[]>(File.ReadAllText("./Configuration/nvulFunctions.json"));
			this.nvulConfiguration = new(nvulFunctions, null, nvulKeywords, null, nvulOperators);
			this.parser = new(nvulConfiguration);
		}

		[Fact]
		public void CanReadConfig()
		{
			string kw = System.IO.File.ReadAllText("./Configuration/nvulKeywords.json");
			string op = System.IO.File.ReadAllText("./Configuration/nvulOperators.json");

			Assert.NotEmpty(kw);
			Assert.NotEmpty(op);

			_output.WriteLine($"kw:\'{kw}\'");
			_output.WriteLine($"op:\'{op}\'");

			var keywords = Utf8Json.JsonSerializer.Deserialize<NvulKeyword[]>(kw);
			var operators = Utf8Json.JsonSerializer.Deserialize<NvulOperator[]>(op);

			Assert.NotEmpty(keywords);
			Assert.NotEmpty(operators);
		}

		[Fact]
		public void CanCreateParser()
		{
			var parser = new Parser(nvulConfiguration);
		}

		[Fact]
		public void CanParseIntegerLiteralString()
		{
			var testString = "123";

			var parsed = parser.ParseLine(testString);

			Assert.IsType<IntegerLiteral>(parsed);
			Assert.Equal(123, ((IntegerLiteral)parsed).Value);
		}

		[Fact]
		public void CanParseFloatLiteralString()
		{
			var testString = "123.15";

			var parsed = parser.ParseLine(testString);

			Assert.IsType<FloatLiteral>(parsed);
			Assert.Equal(123.15, ((FloatLiteral)parsed).Value, 2);
		}

		[Fact]
		public void CanParseDeclarationString()
		{
			var testString = "float myFloat";

			var parsed = parser.ParseLine(testString);

			Assert.IsType<DeclarationNode>(parsed);
			Assert.Equal("float", ((DeclarationNode)parsed).VariableType);
			Assert.Equal("myFloat", ((DeclarationNode)parsed).VariableName);
		}

		[Fact]
		public void CanParseAssignmentString()
		{
			var testString = "myFloat = 123";

			var parsed = parser.ParseLine(testString);

			Assert.IsType<AssignmentNode>(parsed);
			Assert.Equal(123, ((IntegerLiteral)((AssignmentNode)parsed).AssignedValue).Value);
			Assert.Equal("myFloat", ((AssignmentNode)parsed).VariableName);


			testString = "myFloat = 123.15";

			parsed = parser.ParseLine(testString);

			Assert.IsType<AssignmentNode>(parsed);
			Assert.Equal(123.15, ((FloatLiteral)((AssignmentNode)parsed).AssignedValue).Value, 2);
			Assert.Equal("myFloat", ((AssignmentNode)parsed).VariableName);
		}

		[Fact]
		public void CanParseOperatorAssignmentString1()
		{
			var testString = "myFloat = myAnotherFloat1 + myAnotherFloat2";

			var parsed = parser.ParseLine(testString);

			Assert.IsType<AssignmentNode>(parsed);
			var result = (AssignmentNode)parsed;

			Assert.IsType<OperatorNode>(result.AssignedValue);
			var assignedVal = (OperatorNode)result.AssignedValue;

			Assert.IsType<VariableRefNode>(assignedVal.Left);
			Assert.IsType<VariableRefNode>(assignedVal.Right);
			var left = (VariableRefNode)assignedVal.Left;
			var right = (VariableRefNode)assignedVal.Right;

			Assert.Equal("myFloat", ((AssignmentNode)parsed).VariableName);
			Assert.Equal("+", assignedVal.Operator);
			Assert.Equal("myAnotherFloat1", left.VariableName);
			Assert.Equal("myAnotherFloat2", right.VariableName);
		}

		[Fact]
		public void CanParseOperatorAssignmentString2()
		{
			var testString = "myFloat = myAnotherFloat1 + 123";

			var parsed = parser.ParseLine(testString);

			Assert.IsType<AssignmentNode>(parsed);
			var result = (AssignmentNode)parsed;

			Assert.IsType<OperatorNode>(result.AssignedValue);
			var assignedVal = (OperatorNode)result.AssignedValue;

			Assert.IsType<VariableRefNode>(assignedVal.Left);
			Assert.IsType<IntegerLiteral>(assignedVal.Right);
			var left = (VariableRefNode)assignedVal.Left;
			var right = (IntegerLiteral)assignedVal.Right;

			Assert.Equal("myFloat", ((AssignmentNode)parsed).VariableName);
			Assert.Equal("+", assignedVal.Operator);
			Assert.Equal("myAnotherFloat1", left.VariableName);
			Assert.Equal(123, right.Value);
		}

		[Fact]
		public void CanParseOperatorAssignmentString3()
		{
			var testString = "myFloat = 255.0015 + 123";

			var parsed = parser.ParseLine(testString);

			Assert.IsType<AssignmentNode>(parsed);
			var result = (AssignmentNode)parsed;

			Assert.IsType<OperatorNode>(result.AssignedValue);
			var assignedVal = (OperatorNode)result.AssignedValue;

			Assert.IsType<FloatLiteral>(assignedVal.Left);
			Assert.IsType<IntegerLiteral>(assignedVal.Right);
			var left = (FloatLiteral)assignedVal.Left;
			var right = (IntegerLiteral)assignedVal.Right;

			Assert.Equal("myFloat", ((AssignmentNode)parsed).VariableName);
			Assert.Equal("+", assignedVal.Operator);
			Assert.Equal(255.0015, left.Value, 4);
			Assert.Equal(123, right.Value);
		}

		[Fact] // just for eyes-testing
		public void CanParseOperatorAssignmentString4()
		{
			var testString = "myFloat = (255.0015-AnFloat*2) + 123";

			var parsed = parser.ParseLine(testString);

			string json = JsonConvert.SerializeObject(parsed, Formatting.Indented);

			_output.WriteLine(json);

			Assert.True(true);
		}

		[Fact]
		public void CanParseСycleString()
		{
			var testString = "while(11==11) { myF1 = myV1+1; myF2 = myV2+2; }";

			var parsed = parser.ParseLine(testString);

			Assert.IsType<CycleNode>(parsed);
			var result = (CycleNode)parsed;

			Assert.IsType<OperatorNode>(result.Condition);
			var condition = (OperatorNode)result.Condition;

			Assert.Equal(2, result.Childs.Count());
			Assert.IsType<AssignmentNode>(result.Childs.First());
			Assert.IsType<AssignmentNode>(result.Childs.Skip(1).First());

			string json = JsonConvert.SerializeObject(parsed, Formatting.Indented);
			_output.WriteLine(json);
		}

		[Fact]
		public void CanParseFunctionCallString()
		{
			var testString = "myF.Print()";

			var parsed = parser.ParseLine(testString);

			Assert.IsType<FunctionCallNode>(parsed);
			var result = (FunctionCallNode)parsed;
			Assert.False(result.isStatic);
			Assert.Equal("myF", result.VariableName);
			Assert.Equal("Print",result.FunctionName);
			Assert.Empty(result.Arguments);

			string json = JsonConvert.SerializeObject(parsed, Formatting.Indented);
			_output.WriteLine(json);
		}

		[Fact]
		public void CanParseComplexStringString()
		{
			var testString = "float myF; while(myF-0 == 0) { myF.Print(); myF = myF*2 +1; } ";

			var parsed = parser.ParseNvulCode(testString);

			string json = JsonConvert.SerializeObject(parsed, Formatting.Indented,
				new JsonSerializerSettings()
				{
					TypeNameHandling = TypeNameHandling.Auto
				});

			_output.WriteLine(json);
		}

		[Fact]
		public void CanParseConditionalString()
		{
			var testString = "if(myF-0 == 0) { myF.Print(); } ";

			var parsed = parser.ParseLine(testString);

			Assert.IsType<ConditionNode>(parsed);
			var result = (ConditionNode)parsed;

			Assert.IsType<OperatorNode>(result.Condition);
			Assert.Single(result.Childs);
			Assert.IsType<FunctionCallNode>(result.Childs.First());
			string json = JsonConvert.SerializeObject(parsed, Formatting.Indented/*,
				new JsonSerializerSettings()
				{
					TypeNameHandling = TypeNameHandling.Auto
				}*/);
			_output.WriteLine(json);
		}

		[Fact]
		public void CanParseFunctionCallNode()
		{
			var testString = "if(IsNumbersNotEqual(myF,123)) { myF.Print(); } ";

			var parsed = parser.ParseLine(testString);

			Assert.IsType<ConditionNode>(parsed);
			var result = (ConditionNode)parsed;

			Assert.IsType<FunctionCallNode>(result.Condition);
			var condition = (FunctionCallNode)result.Condition;
			Assert.IsType<VariableRefNode>(condition.Arguments.First());
			Assert.IsType<IntegerLiteral>(condition.Arguments.Last());

			string json = JsonConvert.SerializeObject(parsed, Formatting.Indented/*,
				new JsonSerializerSettings()
				{
					TypeNameHandling = TypeNameHandling.Auto
				}*/);
			_output.WriteLine(json);
		}
	}
}
