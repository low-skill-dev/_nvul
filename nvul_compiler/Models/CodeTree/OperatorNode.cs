using nvul_compiler.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.CodeTree
{
	public class OperatorNode : ICodeNode
	{
		public ICodeNode Left {get;set;}
		public ICodeNode Right { get; set; }
		public string Operator { get; set; }
		public NvulOperator? NvulOperator { get; set; }
		public OperatorNode(ICodeNode left, ICodeNode right, string @operator)
		{
			this.Left = left;
			this.Right = right;
			this.Operator = @operator;
		}
	}
}
