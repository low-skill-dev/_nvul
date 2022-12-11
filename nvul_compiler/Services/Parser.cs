using nvul_compiler.Models.CodeTree;
using nvul_compiler.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace nvul_compiler.Services
{
	internal class Parser
	{
		internal struct IndexRange
		{
			public int Start;
			public int Count;
			public int End => Start + Count;

			public IndexRange(int start, int count)
			{
				this.Start = start;
				this.Count = count;
			}
		}

		public char lineDelimiter { get; init; } = ';';

		protected NvulConfiguration _configuration;
		protected Dictionary<ExpressionTypes, Regex> _regexForExpressionType = new();

		public Parser(NvulConfiguration configuration)
		{
			this._configuration = configuration;
		}

		public void CreateRegexesForExpressions()
		{
			throw new NotImplementedException();
		}

		internal enum ExpressionTypes
		{
			AssignmentNode,
			ConditionNode,
			CycleNode,
			DeclarationNode,
			FunctionCallNode,
			LiteralNode
		}

		public ICodeNode ParseLine(string nvulCode)
		{
			throw new NotImplementedException();
		}

		public IEnumerable<ICodeNode> Parse(string nvulCode)
		{
			var lines = nvulCode.Split(lineDelimiter, StringSplitOptions.RemoveEmptyEntries);
			return lines.Select(ln => ParseLine(ln));
		}
	}
}
