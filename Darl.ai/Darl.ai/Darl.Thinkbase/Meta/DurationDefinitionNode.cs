/// <summary>
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System;

namespace Darl.Thinkbase.Meta
{
    public class DurationDefinitionNode : DarlMetaNode
    {
        /// <summary>
        /// Gets the value of the constant.
        /// </summary>
        /// <value>
        /// The value of the constant.
        /// </value>
        public TimeSpan Value { get; private set; }

        /// <summary>
        /// Gets the name of the constant.
        /// </summary>
        /// <value>
        /// The name of the constant.
        /// </value>
        public string name { get; private set; }


        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            TimeSpan parsedVal;
            var durText = nodes[1].Token.Value as string;
            if (TimeSpan.TryParse(durText, out parsedVal))
                Value = parsedVal;
            else if (durText!.ToUpper().Contains('Y'))
            {
                int ypos = durText.ToUpper().IndexOf("Y");
                var numPart = durText.Substring(0, ypos);
                if (int.TryParse(numPart, out int years))
                {
                    Value = new TimeSpan(years * 365, 0, 0, 0, 0);
                }
                else
                    context.AddMessage(DarlCompiler.ErrorLevel.Error, this.ErrorAnchor, $"Could not parse period {nodes[1].Token.Value} bad year format.", null);
            }
            else
                context.AddMessage(DarlCompiler.ErrorLevel.Error, this.ErrorAnchor, $"Could not parse period {nodes[1].Token.Value} bad format.", null);
            name = nodes[0].Token.Text;
        }
    }
}