using nvul_compiler.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.CodeTree
{
	public class FunctionCallNode:ICodeNode
	{
	//	[Obsolete]
		public bool isStatic => this.VariableName is null;
		// null if method is static!
		public string? VariableName { get; set; }
		public string FunctionName { get; set; }
		public IList<ICodeNode> Arguments { get; set; }
		public NvulFunction? NvulFunction { get; set; }


#if DEBUG
		private static int Id = 0;
		public int FuncCallId { get; init; } = Id++;
#endif

		public FunctionCallNode(string? variableName, string functionName, IList<ICodeNode> arguments)
		{
			this.VariableName = variableName;
			this.FunctionName = functionName;
			this.Arguments = arguments;
		}
	}
}
