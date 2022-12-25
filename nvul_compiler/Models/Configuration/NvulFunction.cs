using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.Configuration
{
	public class NvulFunction
	{
		public string FunctionName { get; set; }
		[Obsolete]
		public string? AliasName { get; set; }
		public string? EvaluatesTo { get; set; }
		public IList<string>? ApplicableTypes { get; set; }
		public NvulFunctionParameters? Arguments { get; set; }
		public string TranslationString { get; set; }

		public NvulFunction(string functionName, /*string aliasName,*/string evaluatesTo, IList<string>? applicableTypes = null,NvulFunctionParameters? arguments = null)
		{
			this.FunctionName = functionName;
			/*this.AliasName = aliasName;*/
			this.EvaluatesTo = evaluatesTo;
			this.ApplicableTypes = applicableTypes;
			this.Arguments = arguments;
		}
	}
}
