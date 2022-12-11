using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.Configuration
{
	internal class NvulImplicit
	{
		public string Vartype { get; set; }
		public string ImplicitTo { get; set; }

		public NvulImplicit(string vartype, string implicitTo)
		{
			this.Vartype = vartype;
			this.ImplicitTo = implicitTo;
		}
	}
}
