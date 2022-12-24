using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Models.CodeTree
{
	public interface ILiteralNode:ICodeNode
	{
		public object GetValue();
	}

	public class IntegerLiteral : ILiteralNode
	{
		public int InFileCharIndex { get; set; }
		public long Value { get; set; }
		public object GetValue() => this.Value;
		public IntegerLiteral(long value)
		{
			this.Value = value;
		}
	}

	public class FloatLiteral : ILiteralNode
	{
		public int InFileCharIndex { get; set; }
		public double Value { get; set; }
		public object GetValue() => this.Value;
		public FloatLiteral(double value)
		{
			this.Value = value;
		}
	}

	public class StringLiteral : ILiteralNode
	{
		public int InFileCharIndex { get; set; }
		public string Value { get; set; }
		public object GetValue() => this.Value;
		public StringLiteral(string value)
		{
			this.Value = value;
		}
	}

	public class BoolLiteral:ILiteralNode
	{
		public int InFileCharIndex { get; set; }
		public bool Value { get; set; }
		public object GetValue() => Value ? "true" : "false";
		public BoolLiteral(bool value)
		{
			this.Value = value;
		}
	}
}
