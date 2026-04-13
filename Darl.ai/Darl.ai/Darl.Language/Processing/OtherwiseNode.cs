/// <summary>
/// OtherwiseNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Interpreter.Ast;
using DarlCompiler.Parsing;

namespace DarlLanguage.Processing
{
    public class OtherwiseNode : RuleNode
    {

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            ChildNodes.Clear(); //get rid of parents child node parsing attempts
            var nodes = treeNode.GetMappedChildNodes();
            conditions = AddChild("-", nodes[1]) as DarlNode;
            ruleOutput = AddChild("-", nodes[2]) as DarlIdentifierNode;
            rhs = AddChild("-", nodes[3]) as DarlNode;
            confidenceNode = AddChild("-", nodes[4]) as ConfidenceNode;
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