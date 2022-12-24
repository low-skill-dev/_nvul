using nvul_compiler.Models.CodeTree;
using nvul_compiler.Models.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvul_compiler.Services
{
	public class Translator
	{
		protected NvulConfiguration _configuration;

		public Translator(NvulConfiguration configuration)
		{
			_configuration = configuration;
		}


		internal string BuildNode(ICodeNode node, bool isTopLevel=false, Dictionary<string, NvulKeyword>? vartypesWithAddingRequired=null)
		{
			if (node is DeclarationNode)
			{
				var realNode = (DeclarationNode)node;
				if (realNode.NvulKeyword is null) throw new ArgumentNullException("Translation must be known on the build stage.");
				if (!string.IsNullOrEmpty(realNode.NvulKeyword.UsageRequiresAdding))
					vartypesWithAddingRequired?.TryAdd(realNode.VariableType, realNode.NvulKeyword);
				return $"{realNode.NvulKeyword.TranslationString} {realNode.VariableName}";
			}
			if (node is AssignmentNode)
			{
				var realNode = (AssignmentNode)node;
				return $"{realNode.VariableName} = {BuildNode(realNode.AssignedValue,isTopLevel,vartypesWithAddingRequired)}";
			}
			if (node is ConditionNode || node is CycleNode) // do not use INodeWithConditionAndChilds, show what cases are catched here
			{
				var realNode = (INodeWithConditionAndChilds)node;
				if (realNode.NvulKeyword is null) throw new ArgumentNullException("Translation must be known on the build stage.");
				return $"{realNode.NvulKeyword.TranslationString}({BuildNode(realNode.Condition)}){Environment.NewLine}" +
					$"{{{Environment.NewLine}" +
					$"{BuildNvulCode(realNode.Childs,true, false, vartypesWithAddingRequired)}" +
					$"}}";
			}
			if (node is FunctionCallNode)
			{
				var realNode = (FunctionCallNode)node;
				if (realNode.NvulFunction is null) throw new ArgumentNullException("Translation must be known on the build stage.");
				var varRef = realNode.isStatic ? string.Empty : $"{realNode.VariableName}.";
				return $"{varRef}{realNode.NvulFunction.TranslationString}({string.Join(", ", realNode.Arguments.Select(x => BuildNode(x, isTopLevel, vartypesWithAddingRequired)))})";
			}
			if (node is ILiteralNode)
			{
				var realNode = (ILiteralNode)node;
				return realNode.GetValue()?.ToString()
					?? throw new ArgumentNullException("Literal cannot contain null value.");
			}
			if (node is OperatorNode)
			{
				var realNode = (OperatorNode)node;
				if (realNode.NvulOperator is null) throw new ArgumentNullException("Translation must be known on the build stage.");
				return $"({BuildNode(realNode.Left, isTopLevel, vartypesWithAddingRequired)} {realNode.NvulOperator.TranslationString ?? realNode.NvulOperator.OperatorString} {BuildNode(realNode.Right, isTopLevel, vartypesWithAddingRequired)})";
			}
			if (node is VariableRefNode)
			{
				var realNode = (VariableRefNode)node;
				return realNode.VariableName;
			}

			throw new NotImplementedException($"The passed code node has an unkown type: {((object)node).GetType().FullName}.");
		}

		protected IEnumerator<string> GetTranslatingEnumerator(IEnumerable<ICodeNode> nodes, bool isTopLevel, Dictionary<string, NvulKeyword> vartypesWithAddingRequired)
		{
			foreach (var node in nodes)
			{
				string? builded = null!;
				try
				{
					builded = BuildNode(node,isTopLevel,vartypesWithAddingRequired);
				}
				catch (Exception ex)
				{
					throw new ArgumentException($"Translating error in node starting at char {node.InFileCharIndex}: \'{ex.Message}\'.");
				}
				yield return builded;
			}

			yield break;
		}



		public string BuildNvulCode(IEnumerable<ICodeNode> nodes, bool breakLines = true, bool isTopLevel = false, Dictionary<string, NvulKeyword>? vartypesWithAddingRequired = null)
		{
			StringBuilder sb = new();

			if (vartypesWithAddingRequired is null) vartypesWithAddingRequired = new();

			var enumer = GetTranslatingEnumerator(nodes,isTopLevel,vartypesWithAddingRequired);

			while (enumer.MoveNext())
			{
				var line = enumer.Current;
				sb.Append(line);
				sb.Append(';');
				if (breakLines) sb.AppendLine();
			}

			if (isTopLevel)
			{
				foreach (var s in vartypesWithAddingRequired.Values.Where(x=> !string.IsNullOrEmpty(x.UsageRequiresAdding)).Select(x=> x.UsageRequiresAdding))
				{
					sb.AppendLine(s);
				}
			}

			return sb.ToString();
		}
	}
}
