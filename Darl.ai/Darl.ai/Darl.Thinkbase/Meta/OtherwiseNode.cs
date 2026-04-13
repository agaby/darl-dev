/// <summary>
/// OtherwiseNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Interpreter.Ast;
using DarlCompiler.Parsing;

namespace Darl.Thinkbase.Meta
{
    public class OtherwiseNode : RuleNode
    {
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            ChildNodes.Clear(); //get rid of parents child node parsing attempts
            var nodes = treeNode.GetMappedChildNodes();
            conditions = (AddChild("-", nodes[1]) as DarlMetaNode)! ;
            ruleOutput = (AddChild("-", nodes[2]) as DarlMetaIdentifierNode)!;
            rhs = (AddChild("-", nodes[3]) as DarlMetaNode)!;
            confidenceNode = nodes.Count > 4 ? (AddChild("-", nodes[4]) as ConfidenceNode)! : new ConfidenceNode();
            AsString = "Otherwise";
            ChildNodes[ChildNodes.Count - 1].Flags |= AstNodeFlags.IsTail;
            IsUnknown = true;
        }
        public override string preamble
        {
            get
            {
                return "otherwise if ";
            }
        }

    }
}