using DarlCompiler.Ast;
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
            if (TimeSpan.TryParse(nodes[1].Token.Value as string, out parsedVal))
                Value = parsedVal;
            else
                context.AddMessage(DarlCompiler.ErrorLevel.Error, this.ErrorAnchor, $"Could not parse period {nodes[1].Token.Value} bad format.", null);
            name = nodes[0].Token.Text;
        }
    }
}