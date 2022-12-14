using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.CodeTree
{
	internal class VariableRefNode:ICodeNode
	{
		public string VariableName { get; set; }

		public VariableRefNode(string variableName)
		{
			this.VariableName = variableName;
		}
	}
}
