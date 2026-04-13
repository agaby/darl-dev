/// <summary>
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Interpreter;
using DarlCompiler.Parsing;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class NetworkComponentNode : DarlMetaNode
    {
        public DarlMetaNode lineageNode { get; set; }
        public string lineage { get; set; }

        public string typeword { get; set; }

        public string nodeId { get; set; }
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            var lin = nodes.Last();
            if (lin.Term.Name == "lineageLiteral")
            {
                //get lineage and check it.
                var linLineage = (string)lin.Token.Value;
                var validity = Darl.Lineage.LineageLibrary.CheckLineageWithTypeWord(linLineage);
                if (!validity.Item1)
                {
                    var str = ((DarlMetaGrammar)context.Language.Grammar).structure;
                    var tword = lin.Token.Text.Replace('\"', ' ').Trim();
                    if (str.CommonLineages.ContainsKey(tword))
                    {
                        typeword = lin.Token.Text;
                        lineage = str.CommonLineages[tword];
                    }
                    else
                    {
                        context.AddMessage(DarlCompiler.ErrorLevel.Error, lin.Token.Location, $"'{linLineage}' is not a valid lineage.");
                    }
                }
                else
                {
                    typeword = validity.Item2;
                    lineageNode = lin.AstNode as DarlMetaNode;
                }
            }
            else if (lin.Term.Name == "lineage_constant")
            {
                lineageNode = lin.AstNode as DarlMetaNode;
                //can only convert here if built in.
                if (((DarlMetaGrammar)context.Language.Grammar).structure != null)
                {
                    var str = ((DarlMetaGrammar)context.Language.Grammar).structure;
                    if (str.CommonLineages.ContainsKey(lin.Token.Text))
                    {
                        typeword = lin.Token.Text;
                        lineage = str.CommonLineages[lin.Token.Text];
                    }
                    else
                    {
                        context.AddMessage(DarlCompiler.ErrorLevel.Error, lin.Token.Location, $"lineage {lin.Token.Text} is not amongst pre-defined lineages. Please add a lineage definition such as \"lineage {lin.Token.Text} \"<your lineage>\";\"");
                    }
                }
            }
            nodeId = (string)nodes.First().Token.Value;
        }

        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            return base.DoEvaluate(thread);
        }

        public override string preamble => $"\"{nodeId}\" {lineageNode.TermToDarl()}";
    }
}
