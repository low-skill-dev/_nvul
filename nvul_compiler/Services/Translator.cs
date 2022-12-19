using nvul_compiler.Models.CodeTree;
using nvul_compiler.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Services
{
	internal class Translator
	{
		protected NvulConfiguration _configuration;

		public Translator(NvulConfiguration configuration)
		{
			_configuration = configuration;
		}


		public string BuildNode(ICodeNode node)
		{
			if(node is DeclarationNode)
			{
				var realNode = (DeclarationNode)node;
				if (realNode.NvulKeyword is null) throw new ArgumentNullException("Translation must be known on the build stage.");
				return $"{realNode.NvulKeyword.TranslationString} {realNode.VariableName}";
			}
			if(node is AssignmentNode)
			{
				var realNode = (AssignmentNode)node;
				return $"{realNode.VariableName} = {BuildNode(realNode.AssignedValue)}";
			}
			if(node is ConditionNode || node is CycleNode) // do not use INodeWithConditionAndChilds, show what cases are catched here
			{
				var realNode = (INodeWithConditionAndChilds)node;
				if (realNode.NvulKeyword is null) throw new ArgumentNullException("Translation must be known on the build stage.");
				return $"{realNode.NvulKeyword.TranslationString}({BuildNode(realNode.Condition)}) {{{TranslateNvulNodes(realNode.Childs)}}}";
			}
			if(node is FunctionCallNode)
			{
				var realNode = (FunctionCallNode)node;
				if (realNode.NvulFunction is null) throw new ArgumentNullException("Translation must be known on the build stage.");
				return $"{realNode.NvulFunction.TranslationString}({string.Join(", ",realNode.Arguments.Select(x=> BuildNode(x)))})";
			}
			if(node is ILiteralNode)
			{
				var realNode = (ILiteralNode)node;
				return realNode.GetValue()?.ToString() 
					?? throw new ArgumentNullException("Literal cannot contain null value.");
			}
			if(node is OperatorNode)
			{
				var realNode = (OperatorNode)node;
				if (realNode.NvulOperator is null) throw new ArgumentNullException("Translation must be known on the build stage.");
				return $"({BuildNode(realNode.Left)} {realNode.NvulOperator.TranslationString ?? realNode.NvulOperator.OperatorString} {BuildNode(realNode.Right)})";
			}
			if(node is VariableRefNode)
			{
				var realNode = (VariableRefNode)node;
				return realNode.VariableName;
			}

			throw new NotImplementedException($"The passed code node has an unkown type: {((object)node).GetType().FullName}.");
		}

		public IEnumerable<string> TranslateNvulNodes(IEnumerable<ICodeNode> nodes)
		{
			foreach(var node in nodes)
			{
				yield return BuildNode(node);
			}

			yield break;
		}

		public string BuildNvulCode(IEnumerable<ICodeNode> nodes)
		{
			StringBuilder sb = new();

			foreach(string line in TranslateNvulNodes(nodes))
			{
				sb.Append(line);
				sb.Append(';');
				sb.AppendLine();
			}

			return sb.ToString();
		}
	}
}
