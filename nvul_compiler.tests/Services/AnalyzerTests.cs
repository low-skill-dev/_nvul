using Newtonsoft.Json;
using nvul_compiler.Models.CodeTree;
using nvul_compiler.Models.Configuration;
using nvul_compiler.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using Xunit.Sdk;

namespace nvul_compiler.tests.Services
{
	public class AnalyzerTests
	{
		internal NvulKeyword[] nvulKeywords;
		internal NvulOperator[] nvulOperators;
		internal NvulFunction[] nvulFunctions;
		internal NvulImplicit[] nvulImplicits;
		internal NvulConfiguration nvulConfiguration;
		internal Parser parser;
		internal Analyzer analyzer;

		internal ITestOutputHelper _output;
		public AnalyzerTests(ITestOutputHelper output)
		{
			_output = output;

			this.nvulKeywords = Utf8Json.JsonSerializer.Deserialize<NvulKeyword[]>(File.ReadAllText("./Configuration/nvulKeywords.json"));
			this.nvulOperators = Utf8Json.JsonSerializer.Deserialize<NvulOperator[]>(File.ReadAllText("./Configuration/nvulOperators.json"));
			this.nvulFunctions = Utf8Json.JsonSerializer.Deserialize<NvulFunction[]>(File.ReadAllText("./Configuration/nvulFunctions.json"));
			this.nvulImplicits = Utf8Json.JsonSerializer.Deserialize<NvulImplicit[]>(File.ReadAllText("./Configuration/nvulImplicits.json"));

			this.nvulConfiguration = new(nvulFunctions, nvulImplicits, nvulKeywords, null, nvulOperators);
			this.parser = new(nvulConfiguration);
			this.analyzer = new(nvulConfiguration);
		}

		[Fact]
		public void CanCreateAnalyzer()
		{
			Analyzer t = new Analyzer(this.nvulConfiguration);
			Assert.NotNull(t);
		}

		[Fact]
		public void CanAnalyzeSimpleNode()
		{
			var nodes = this.parser.ParseNvulCode("integer myI; myI = 200;");

			var result = this.analyzer.AnalyzeNvulNodes(nodes);

			Assert.True(result);
		}

		[Fact]
		public void CanRejectUndeclaredVariable1()
		{
			var nodes = this.parser.ParseNvulCode("integer v1; v2 = 200;");

			var analyzeAction = () => { this.analyzer.AnalyzeNvulNodes(nodes); };

			Assert.Throws<ArgumentException>(analyzeAction);
			try { analyzeAction(); } catch (Exception ex) { _output.WriteLine(ex.ToString()); }
		}

		[Fact]
		public void CanRejectUndeclaredVariable2()
		{
			var nodes = this.parser.ParseNvulCode("integer v1; while(v2==2) { integer v2; }");
			string json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
			_output.WriteLine(json);

			var analyzeAction = () => { this.analyzer.AnalyzeNvulNodes(nodes); };

			Assert.Throws<ArgumentException>(analyzeAction);
			try { analyzeAction(); } catch(Exception ex) { _output.WriteLine(ex.ToString()); }
		}

		[Fact]
		public void CanAcceptImplicitType()
		{
			var nodes = this.parser.ParseNvulCode("float f1; f1 = 123.00; f1=123; integer i1; i1 = 123; f1 = i1;");
			string json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
			_output.WriteLine(json);

			var analyzed = this.analyzer.AnalyzeNvulNodes(nodes);

			Assert.True(analyzed);
		}

		[Fact]
		public void CanRejectInconsistentType1()
		{
			var nodes = this.parser.ParseNvulCode("float f1; f1 = 123.00; logical l1; l1 = f1;");
			string json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
			_output.WriteLine(json);

			var analyzeAction = () => { this.analyzer.AnalyzeNvulNodes(nodes); };

			Assert.Throws<ArgumentException>(analyzeAction);
			try { analyzeAction(); } catch (Exception ex) { _output.WriteLine(ex.ToString()); }
		}

		[Fact]
		public void CanRejectInconsistentType2()
		{
			var nodes = this.parser.ParseNvulCode("float f1; f1 = true;");
			string json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
			_output.WriteLine(json);

			var analyzeAction = () => { this.analyzer.AnalyzeNvulNodes(nodes); };

			Assert.Throws<ArgumentException>(analyzeAction);
			try { analyzeAction(); } catch (Exception ex) { _output.WriteLine(ex.ToString()); }
		}


		[Fact]
		public void CanRejectInconsistentType3()
		{
			// should accept
			var nodes = this.parser.ParseNvulCode("while(true) { }");
			Assert.True(analyzer.AnalyzeNvulNodes(nodes));

			// should reject
			nodes = this.parser.ParseNvulCode("while(123) { }");
			string json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
			_output.WriteLine(json);

			var analyzeAction = () => { this.analyzer.AnalyzeNvulNodes(nodes); };

			Assert.Throws<ArgumentException>(analyzeAction);
			try { analyzeAction(); } catch (Exception ex) { _output.WriteLine(ex.ToString()); }
		}

		[Fact]
		public void CanRejectRedefining()
		{
			// should accept
			var nodes = this.parser.ParseNvulCode("integer i1; while(true) {" +
				" while(true){ while(true){ while(true){ while(true){ i1=2;  while(true){ i1 = 3; }  } } } } }");
			Assert.True(analyzer.AnalyzeNvulNodes(nodes));

			// should reject
			nodes = this.parser.ParseNvulCode("integer i1; while(true) {" +
				" while(true){ while(true){ while(true){ while(true){ i1=2;  while(true){ integer i1; i1 = 3; }  } } } } } ");
			string json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
			_output.WriteLine(json);

			var analyzeAction = () => { this.analyzer.AnalyzeNvulNodes(nodes); };

			Assert.Throws<ArgumentException>(analyzeAction);
			try { analyzeAction(); } catch (Exception ex) { _output.WriteLine(ex.ToString()); }
		}

		[Fact]
		public void CanRejectUnassignedVariable()
		{
			// should accept
			var nodes = this.parser.ParseNvulCode("integer i1; i1 = 123; integer i2; i2 = i1+10;");
			Assert.True(analyzer.AnalyzeNvulNodes(nodes));

			// should reject
			nodes = this.parser.ParseNvulCode("integer i1; integer i2; i2 = i1+10;");
			string json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
			_output.WriteLine(json);

			var analyzeAction = () => { this.analyzer.AnalyzeNvulNodes(nodes); };

			Assert.Throws<NullReferenceException>(analyzeAction);
			try { analyzeAction(); } catch (Exception ex) { _output.WriteLine(ex.ToString()); }
		}

		[Fact]
		public void CanAcceptAssigningByFunction()
		{
			// should accept
			var nodes = this.parser.ParseNvulCode("float f1; f1 = InputIntegerValue(); PrintNumericValue(f1); PrintNumericValue(InputIntegerValue());");
			Assert.True(analyzer.AnalyzeNvulNodes(nodes));

			// should reject (type inconsist)
			nodes = this.parser.ParseNvulCode("logical l1; l1 = InputLogicalValue(); PrintNumericValue(l1);");
			string json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
			_output.WriteLine(json);

			var analyzeAction = () => { this.analyzer.AnalyzeNvulNodes(nodes); };

			Assert.Throws<ArgumentException>(analyzeAction);
			try { analyzeAction(); } catch (Exception ex) { _output.WriteLine(ex.ToString()); }

			// should reject (type inconsist)
			nodes = this.parser.ParseNvulCode("PrintNumericValue(InputLogicalValue());");
			json = JsonConvert.SerializeObject(nodes, Formatting.Indented);
			_output.WriteLine(json);

			analyzeAction = () => { this.analyzer.AnalyzeNvulNodes(nodes); };

			Assert.Throws<ArgumentException>(analyzeAction);
			try { analyzeAction(); } catch (Exception ex) { _output.WriteLine(ex.ToString()); }
		}

		[Fact]
		public void CanDeterminateFunctionOverload()
		{
			var testString = "matrix mtx; mtx = InputMatrixValue(123);";
			var parsed = parser.ParseNvulCode(testString).ToList();
			analyzer.AnalyzeNvulNodes(parsed, new(null));
			var assignedFunction = (FunctionCallNode)((AssignmentNode)parsed.Last()).AssignedValue;
			Assert.NotNull(assignedFunction.NvulFunction);
		}
	}
}
