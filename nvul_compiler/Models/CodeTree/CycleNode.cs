using nvul_compiler.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.CodeTree
{
	public interface INodeWithConditionAndChilds:ICodeNode
	{
		public ICodeNode Condition { get; set; }
		public IEnumerable<ICodeNode> Childs { get; set; }
		public NvulKeyword? NvulKeyword { get; set; }
	}
	public class CycleNode: INodeWithConditionAndChilds
	{
		public int InFileCharIndex { get; set; }
		public ICodeNode Condition { get; set; }
		public IEnumerable<ICodeNode> Childs { get; set; }
		public NvulKeyword? NvulKeyword { get; set; }

		public CycleNode(ICodeNode condition, IEnumerable<ICodeNode> childs)
		{
			this.Condition = condition;
			this.Childs = childs;
		}
	}
}
