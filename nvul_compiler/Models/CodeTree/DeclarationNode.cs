using nvul_compiler.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.CodeTree
{
	public class DeclarationNode:ICodeNode
	{
		public string VariableType { get; set; }
		public string VariableName { get; set; }
		public NvulKeyword? NvulKeyword { get; set; }
		public DeclarationNode(string variableType, string variableName)
		{
			this.VariableType = variableType;
			this.VariableName = variableName;
		}
	}
}
