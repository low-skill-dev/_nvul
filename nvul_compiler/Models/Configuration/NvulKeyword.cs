using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.Configuration
{
	internal class NvulKeyword
	{
		public string Word { get; set; }
		public string Alias { get; set; }
		public string Type { get; set; }
		public IList<NvulFunctionParameters>? CtorArguments { get; set; }

		public NvulKeyword(string word, string alias, string type, IList<NvulFunctionParameters>? ctorArguments = null)
		{
			this.Word = word;
			this.Alias = alias;
			this.Type = type;
			this.CtorArguments = ctorArguments;
		}
	}
}
