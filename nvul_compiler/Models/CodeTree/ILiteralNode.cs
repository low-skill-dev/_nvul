using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.CodeTree
{
	internal interface ILiteralNode:ICodeNode
	{

	}

	internal class IntegerLiteral : ILiteralNode
	{
		public int Value { get; set; }
		public IntegerLiteral(int value)
		{
			this.Value = value;
		}
	}

	internal class FloatLiteral : ILiteralNode
	{
		public double Value { get; set; }
		public FloatLiteral(double value)
		{
			this.Value = value;
		}
	}

	internal class StringLiteral : ILiteralNode
	{
		public string Value { get; set; }
		public StringLiteral(string value)
		{
			this.Value = value;
		}
	}

	internal class BoolLiteral:ILiteralNode
	{
		public bool Value { get; set; }
		public BoolLiteral(bool value)
		{
			this.Value = value;
		}
	}
}
