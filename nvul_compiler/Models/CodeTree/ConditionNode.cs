using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.CodeTree
{
	internal class ConditionNode
	{
		public ICodeNode Condition { get; set; }
		public IList<ICodeNode> Childs { get; set; }
		public ConditionNode? NextCondition { get; set; }

		public ConditionNode(ICodeNode condition, IList<ICodeNode> childs, ConditionNode? nextCondition=null)
		{
			this.Condition = condition;
			this.Childs = childs;
			this.NextCondition = nextCondition;
		}
	}
}
