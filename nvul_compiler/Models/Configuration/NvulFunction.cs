using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.Configuration
{
	internal class NvulFunction
	{
		public string FunctionName { get; set; }
		public string AliasName { get; set; }
		public IList<string>? ApplicableTypes { get; set; }
		public NvulFunctionParameters? Arguments { get; set; }

		public NvulFunction(string functionName, string aliasName, IList<string>? applicableTypes=null, NvulFunctionParameters? arguments=null)
		{
			this.FunctionName = functionName;
			this.AliasName = aliasName;
			this.ApplicableTypes = applicableTypes;
			this.Arguments = arguments;
		}
	}
}
