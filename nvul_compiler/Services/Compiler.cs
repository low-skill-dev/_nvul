using nvul_compiler.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Services
{
	public static class Compiler
	{
		public static string Compile(string nvulCode, NvulConfiguration nvulConfiguration)
		{
			var parser = new Parser(nvulConfiguration);
			var analyzer = new Analyzer(nvulConfiguration);
			var translator = new Translator(nvulConfiguration);
			var codeBuilder = new StringBuilder();

			var enumer = parser.GetParsingEnumerator(nvulCode);
			var rootContext = new CodeContext(null);
			var addingRequired = new Dictionary<string, NvulKeyword>();
			while (enumer.MoveNext())
			{
				var parsed = enumer.Current;
				analyzer.AnalyzeNodeAndUpdateContext(parsed, rootContext);
				codeBuilder.Append(translator.BuildNode(parsed, false, addingRequired));
				codeBuilder.Append(';');
				codeBuilder.AppendLine();
			}

			foreach (var s in addingRequired.Values.Where(x => !string.IsNullOrEmpty(x.UsageRequiresAdding)).Select(x => x.UsageRequiresAdding))
			{
				codeBuilder.AppendLine(s);
			}

			return codeBuilder.ToString();
		}

		public static string Compile(string nvulCode, NvulConfiguration nvulConfiguration, out string parsingResult)
		{
			var parser = new Parser(nvulConfiguration);
			var analyzer = new Analyzer(nvulConfiguration);
			var translator = new Translator(nvulConfiguration);
			var parsingBuilder = new StringBuilder();
			var codeBuilder = new StringBuilder();

			var enumer = parser.GetParsingEnumerator(nvulCode);
			var rootContext = new CodeContext(null);
			var addingRequired = new Dictionary<string, NvulKeyword>();
			parsingBuilder.Append('[');
			while (enumer.MoveNext())
			{
				var parsed = enumer.Current;
				analyzer.AnalyzeNodeAndUpdateContext(parsed, rootContext);
				parsingBuilder.AppendLine(Newtonsoft.Json.JsonConvert.SerializeObject(parsed));
				parsingBuilder.Append(',');
				codeBuilder.Append(translator.BuildNode(parsed, false, addingRequired));
				codeBuilder.Append(';');
				codeBuilder.AppendLine();
			}

			foreach (var s in addingRequired.Values.Where(x => !string.IsNullOrEmpty(x.UsageRequiresAdding)).Select(x => x.UsageRequiresAdding))
			{
				codeBuilder.AppendLine(s);
			}

			parsingBuilder.Remove(parsingBuilder.Length - 1, 1); // last coma
			parsingBuilder.Append(']');
			parsingResult = parsingBuilder.ToString();
			return codeBuilder.ToString();
		}
	}
}
