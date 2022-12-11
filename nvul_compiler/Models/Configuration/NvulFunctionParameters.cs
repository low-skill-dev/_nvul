using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.Configuration
{
	internal class NvulFunctionParameters
	{
		public IList<string> ParametersTypes { get; set; }
		public IList<string> ParametersDescription { get; set; }

		public NvulFunctionParameters(IList<string> parametersTypes, IList<string> parametersDescription)
		{
			this.ParametersTypes = parametersTypes;
			this.ParametersDescription = parametersDescription;
		}
	}
}
