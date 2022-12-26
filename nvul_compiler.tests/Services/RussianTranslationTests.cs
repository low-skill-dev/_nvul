using Newtonsoft.Json;
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
	public class RussianTranslatorTests
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
		public RussianTranslatorTests(ITestOutputHelper output)
		{
			_output = output;

			//this.nvulKeywords = Utf8Json.JsonSerializer.Deserialize<NvulKeyword[]>(File.ReadAllText("./Configuration/Russian/nvulKeywords.json"));
			//this.nvulOperators = Utf8Json.JsonSerializer.Deserialize<NvulOperator[]>(File.ReadAllText("./Configuration/Russian/nvulOperators.json"));
			//this.nvulFunctions = Utf8Json.JsonSerializer.Deserialize<NvulFunction[]>(File.ReadAllText("./Configuration/Russian/nvulFunctions.json"));
			//this.nvulImplicits = Utf8Json.JsonSerializer.Deserialize<NvulImplicit[]>(File.ReadAllText("./Configuration/Russian/nvulImplicits.json"));

			this.nvulConfiguration = Newtonsoft.Json.JsonConvert.DeserializeObject<NvulConfiguration>(File.ReadAllText("./Configuration/Russian/fullConfig.json"));
			this.parser = new(nvulConfiguration);
			this.analyzer = new(nvulConfiguration);
			this.translator = new(nvulConfiguration);
		}

		[Fact]
		public void CanTranslateCode1()
		{
			var testString = "целое ц1;\r\nдействительное д1;\r\nц1 = 5;\r\nд1 = 6;\r\n\r\nwhile((д1-ц1) > 0){\r\n    ц1 = ц1 + 1;\r\n}";

			var parsed = parser.ParseNvulCode(testString).ToList();
			analyzer.AnalyzeNvulNodes(parsed, new(null));
			var result = translator.BuildNvulCode(parsed, isTopLevel: true);

			_output.WriteLine(result);
		}

		[Fact]
		public void CanTranslateCode2()
		{
			/*
				целое ц1;
				действительное д1;
				ц1 = 1;
				д1 = 10;
				while((д1-ц1) > 0)
				{
					ц1 = ц1 + 1;
					ВывестиЗначение(д1-ц1);
				};

				целое Сторона;
				Сторона = ВвестиЦелоеЗначение();
				матрицаЦелых Мтр;
				Мтр = МатрицаЦелых.СоздатьВводом(Сторона,Сторона);
				Мтр.УстановитьЗачениеНаАдрес(Сторона,0,0);
				Мтр.ОтсортироватьСтроки();

				ВывестиЗначение(Мтр);
			 */
			var testString = "целое ц1;\r\nдействительное д1;\r\nц1 = 1;\r\nд1 = 10;\r\nwhile((д1-ц1) > 0)\r\n{\r\n    ц1 = ц1 + 1;\r\n    ВывестиЗначение(д1-ц1);\r\n};\r\n\r\nцелое Сторона;\r\nСторона = ВвестиЦелоеЗначение();\r\nматрицаЦелых Мтр;\r\nМтр = МатрицаЦелых.СоздатьВводом(Сторона,Сторона);\r\nМтр.УстановитьЗачениеНаАдрес(Сторона,0,0);\r\nМтр.ОтсортироватьСтроки();\r\n\r\nВывестиЗначение(Мтр);";

			_output.WriteLine(Newtonsoft.Json.JsonConvert.SerializeObject(parser.ParseNvulCode(testString)));

			var parsed = parser.ParseNvulCode(testString).ToList();
			analyzer.AnalyzeNvulNodes(parsed, new(null));
			var result = translator.BuildNvulCode(parsed, isTopLevel: true);

			_output.WriteLine(result);
		}
	}
}
