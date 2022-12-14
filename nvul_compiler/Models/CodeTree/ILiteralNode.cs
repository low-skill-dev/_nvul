using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.CodeTree
{
	public interface ILiteralNode:ICodeNode
	{

	}

	public class IntegerLiteral : ILiteralNode
	{
		public long Value { get; set; }
		public IntegerLiteral(long value)
		{
			this.Value = value;
		}
	}

	public class FloatLiteral : ILiteralNode
	{
		public double Value { get; set; }
		public FloatLiteral(double value)
		{
			this.Value = value;
		}
	}

	public class StringLiteral : ILiteralNode
	{
		public string Value { get; set; }
		public StringLiteral(string value)
		{
			this.Value = value;
		}
	}

	public class BoolLiteral:ILiteralNode
	{
		public bool Value { get; set; }
		public BoolLiteral(bool value)
		{
			this.Value = value;
		}
	}
}
