using Microsoft.AspNetCore.Mvc;
using nvul_compiler.Models.Configuration;
using nvul_compiler.Services;
using nvul_server.Controllers;
using nvul_server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace nvul_server.tests.Endpoints
{
	public class CompilerTests
	{
		internal NvulKeyword[] nvulKeywords;
		internal NvulOperator[] nvulOperators;
		internal NvulFunction[] nvulFunctions;
		internal NvulImplicit[] nvulImplicits;
		internal NvulConfiguration nvulConfiguration;
		internal Parser parser;
		internal Analyzer analyzer;
		internal Translator translator;

		internal CompilerContoller contoller;


		internal ITestOutputHelper _output;
		public CompilerTests(ITestOutputHelper output)
		{
			_output = output;

			this.nvulKeywords = Utf8Json.JsonSerializer.Deserialize<NvulKeyword[]>(File.ReadAllText("./Configuration/nvulKeywords.json"));
			this.nvulOperators = Utf8Json.JsonSerializer.Deserialize<NvulOperator[]>(File.ReadAllText("./Configuration/nvulOperators.json"));
			this.nvulFunctions = Utf8Json.JsonSerializer.Deserialize<NvulFunction[]>(File.ReadAllText("./Configuration/nvulFunctions.json"));
			this.nvulImplicits = Utf8Json.JsonSerializer.Deserialize<NvulImplicit[]>(File.ReadAllText("./Configuration/nvulImplicits.json"));

			this.nvulConfiguration = new(nvulFunctions, nvulImplicits, nvulKeywords, nvulOperators);
			this.parser = new(nvulConfiguration);
			this.analyzer = new(nvulConfiguration);
			this.translator = new(nvulConfiguration);

			this.contoller = new(null!,null);
		}

		[Fact]
		public async void CanCompile()
		{
			var testString = "matrix myMtr; myMtr = IntMatrix.FactoryInputCreate(2,2);" +
				"integer c1; c1=1; while(c1>0) { myMtr.SetVal(c1,0,0); }; PrintMatrix2(myMtr);";
			var expectedResult = "IntegerMatrix myMtr;\r\nmyMtr = IntegerMatrix.InputCreate(2, 2);\r\nlong c1;\r\nc1 = 1;\r\nwhile((c1 > 0))\r\n{\r\nmyMtr.SetValue(c1, 0, 0);\r\n};\r\nConsole.WriteLine(myMtr);\r\n";
			var result = await this.contoller.CompileCode(new(testString, Utf8Json.JsonSerializer.ToJsonString(this.nvulConfiguration)));

			Assert.IsType<OkObjectResult>(result);
			var responseValue = ((OkObjectResult)result).Value;

			Assert.IsType<CompilationResponse>(responseValue);
			Assert.NotNull(responseValue);
			var compilationResponse = (CompilationResponse)responseValue!;

			Assert.StartsWith(expectedResult,compilationResponse.CompilationResult);
		}
	}
}
