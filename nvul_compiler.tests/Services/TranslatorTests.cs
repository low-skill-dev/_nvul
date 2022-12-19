using Newtonsoft.Json;
using nvul_compiler.Models.Configuration;
using nvul_compiler.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace nvul_compiler.tests.Services
{
	public class TranslatorTests
	{
		internal NvulKeyword[] nvulKeywords;
		internal NvulOperator[] nvulOperators;
		internal NvulFunction[] nvulFunctions;
		internal NvulImplicit[] nvulImplicits;
		internal NvulConfiguration nvulConfiguration;
		internal Parser parser;
		internal Analyzer analyzer;
		internal Translator translator;


		internal ITestOutputHelper _output;
		public TranslatorTests(ITestOutputHelper output)
		{
			_output = output;

			this.nvulKeywords = Utf8Json.JsonSerializer.Deserialize<NvulKeyword[]>(File.ReadAllText("./Configuration/nvulKeywords.json"));
			this.nvulOperators = Utf8Json.JsonSerializer.Deserialize<NvulOperator[]>(File.ReadAllText("./Configuration/nvulOperators.json"));
			this.nvulFunctions = Utf8Json.JsonSerializer.Deserialize<NvulFunction[]>(File.ReadAllText("./Configuration/nvulFunctions.json"));
			this.nvulImplicits = Utf8Json.JsonSerializer.Deserialize<NvulImplicit[]>(File.ReadAllText("./Configuration/nvulImplicits.json"));

			this.nvulConfiguration = new(nvulFunctions, nvulImplicits, nvulKeywords, null, nvulOperators);
			this.parser = new(nvulConfiguration);
			this.analyzer = new(nvulConfiguration);
			this.translator = new(nvulConfiguration);
		}

		[Fact]
		public void CanCreateTranslator()
		{
			var translator = new Translator(nvulConfiguration);
			Assert.NotNull(translator);
		}

		[Fact]
		public void CanTranslateIntegerLiteral()
		{
			string testString = "123";

			var result = translator.BuildNode(parser.ParseLine(testString));

			Assert.Equal("123", result);
		}

		[Fact]
		public void CanTranslateFloatLiteral()
		{
			string testString = "123.123";

			var result = translator.BuildNode(parser.ParseLine(testString));

			Assert.Equal("123.123", result);
		}

		[Fact]
		public void CanTranslateBooleanLiteral()
		{
			string testString = "true";
			var result = translator.BuildNode(parser.ParseLine(testString));
			Assert.Equal("true", result);

			testString = "false";
			result = translator.BuildNode(parser.ParseLine(testString));
			Assert.Equal("false", result);
		}

		[Fact]
		public void CanTranslateDeclaration()
		{
			string testString = "integer myInt";
			var result = translator.BuildNode(parser.ParseLine(testString));
			Assert.Equal("long myInt", result);

			testString = "float myFloat";
			result = translator.BuildNode(parser.ParseLine(testString));
			Assert.Equal("double myFloat", result);

			testString = "logical myLogical";
			result = translator.BuildNode(parser.ParseLine(testString));
			Assert.Equal("bool myLogical", result);

			testString = "matrix myMatrix";
			result = translator.BuildNode(parser.ParseLine(testString));
			Assert.Equal("Matrix myMatrix", result);
		}

		[Fact]
		public void CanTranslateAssignment()
		{
			string testString = "myInt = 123";
			var result = translator.BuildNode(parser.ParseLine(testString));
			Assert.Equal("myInt = 123", result);

			testString = "myFloat = 123.123";
			result = translator.BuildNode(parser.ParseLine(testString));
			Assert.Equal("myFloat = 123.123", result);

			testString = "myLogical = true";
			result = translator.BuildNode(parser.ParseLine(testString));
			Assert.Equal("myLogical = true", result);


			// The exact function overload to be called is being determined
			// on the analyzing step, so it is need to be performed here.
			// ToList() is required because Parser is actually an enumerator,
			// its not returning an actual collection.
			testString = "matrix mtx; mtx = InputMatrixValue(123);";
			var parsed = parser.ParseNvulCode(testString).ToList();
			analyzer.AnalyzeNvulNodes(parsed,new(null));
			result = translator.BuildNvulCode(parsed);
			Assert.Equal($"Matrix mtx;{Environment.NewLine}mtx = InputMatrix(123);{Environment.NewLine}", result);
		}

		[Fact]
		public void CanTranslateOperator()
		{
			// The exact operator overload to be called is being determined
			// on the analyzing step, so it is need to be performed here.
			// ToList() is required because Parser is actually an enumerator,
			// its not returning an actual collection.
			var expr = "1-123+222*4/2+225/2+1+1+1/2+1*3+1-(222*4/2+225/2-222*4/2+225/2-(222*4/2+225/2)-(1/1/1/1/(1)))";
			var testString = $"float fl; fl = {expr};";
			var parsed = parser.ParseNvulCode(testString).ToList();
			analyzer.AnalyzeNvulNodes(parsed, new(null));
			var result = translator.BuildNvulCode(parsed);

			var translatedExpr = new string(result.Substring(result.IndexOf('(')).Where(x => !char.IsWhiteSpace(x)).SkipLast(1).ToArray());
			StringEvaluator evaler = new();

			_output.WriteLine(translatedExpr);

			// equations are same
			Assert.Equal(evaler.Eval2(expr), evaler.Eval2(translatedExpr),10);
		}
	}
}
