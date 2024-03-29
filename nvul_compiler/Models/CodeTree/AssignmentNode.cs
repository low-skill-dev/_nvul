﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.CodeTree
{
	public class AssignmentNode:ICodeNode
	{
		public int InNodeCharIndex { get; set; }

		public string VariableName { get; set; }
		public ICodeNode AssignedValue { get; set; }

		public AssignmentNode(string variableName, ICodeNode assignedValue)
		{
			this.VariableName = variableName;
			this.AssignedValue = assignedValue;
		}
	}
}
