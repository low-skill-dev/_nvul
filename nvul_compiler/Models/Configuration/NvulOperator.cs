using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.Configuration
{
	public class NvulOperator
	{
		public string OperatorString { get; set; }
		public int OperatorPriority { get; set; } // higher priority = evaluates later!!! 1 - top priority, 1024 - lowest prior
		public string LeftType { get; set; }
		public string RightType { get; set; }
		public string EvaluatesTo { get; set; }

		public NvulOperator(string operatorString, int operatorPriority, string leftType, string rightType, string evaluatesTo)
		{
			this.OperatorString = operatorString;
			this.OperatorPriority = operatorPriority;
			this.LeftType = leftType;
			this.RightType = rightType;
			this.EvaluatesTo = evaluatesTo;
		}
	}
}
