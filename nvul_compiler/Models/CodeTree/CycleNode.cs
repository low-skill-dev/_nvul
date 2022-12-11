using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.CodeTree
{
	internal class CycleNode
	{
		public ICodeNode Condition { get; set; }
		public IList<ICodeNode> Childs { get; set; }
	}
}
