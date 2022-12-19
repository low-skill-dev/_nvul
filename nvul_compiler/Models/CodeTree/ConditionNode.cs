using nvul_compiler.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.CodeTree
{
	public class ConditionNode : INodeWithConditionAndChilds
	{
		public ICodeNode Condition { get; set; }
		public IEnumerable<ICodeNode> Childs { get; set; }
		public ConditionNode? NextCondition { get; set; }
		public NvulKeyword? NvulKeyword { get; set; }

		public ConditionNode(ICodeNode condition, IEnumerable<ICodeNode> childs, ConditionNode? nextCondition=null)
		{
			this.Condition = condition;
			this.Childs = childs;
			this.NextCondition = nextCondition;
		}
	}
}
