using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System;
using System.Linq;

namespace Darl.Thinkbase.Meta
{
    public class InputDefinitionNode : IODefinitionNode
    {
        /// <summary>
        /// Permissible input types
        /// </summary>
        public enum InputTypes
        {
            /// <summary>
            /// The numeric_input
            /// </summary>
            numeric_input,
            /// <summary>
            /// The categorical_input
            /// </summary>
            categorical_input,
            /// <summary>
            /// The textual_input
            /// </summary>
            textual_input,
            /// <summary>
            /// The arity_input
            /// </summary>
            arity_input,
            /// <summary>
            /// The presence_input
            /// </summary>
            presence_input,
            /// <summary>
            /// temporal input
            /// </summary>
            temporal_input,
            ///<summary>
            /// dynamic categorical input
            ///</summary>
            dynamic_categorical_input
        };

        /// <summary>
        /// Gets the type of the input.
        /// </summary>
        /// <value>
        /// The type of the input.
        /// </value>
        public InputTypes iType { get; private set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        /// <value>
        /// The value.
        /// </value>
        public DarlResult Value { get; set; }


        public DarlMetaNode LineageNode { get; set; }

        /// <summary>
        /// Gets the salience.
        /// </summary>
        /// <value>
        /// The salience.
        /// </value>
        public double Salience { get; internal set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="InputDefinitionNode"/> class.
        /// </summary>
        public InputDefinitionNode()
        {
            Value = new DarlResult(0.0, true);
        }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            //edit to store node tree as well, so that toDarl can differentiate between string literals and identifiers, since both are permitted.
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            iType = (InputTypes)Enum.Parse(typeof(InputTypes), nodes[0].Term.Name, true);
            name = (string)nodes[0].Token.Value;
            switch (iType)
            {
                case InputTypes.numeric_input:
                case InputTypes.arity_input:
                case InputTypes.temporal_input:
                    if (context.Values.Count > 0)
                    {
                        foreach (object key in context.Values.Keys)
                        {
                            sets.Add((string)key, (DarlResult)context.Values[key]);
                        }
                        context.Values.Clear();
                    }
                    break;
                case InputTypes.categorical_input:
                    if (nodes.Count > 1)
                    {
                        if (nodes[1].ChildNodes.Any() && nodes[1].ChildNodes[0].AstNode is AttributeNode)
                        {
                            var attributeNode = nodes[1].ChildNodes[0].AstNode as AttributeNode;
                            LineageNode = attributeNode.ChildNodes[0] as DarlMetaNode;
                        }
                        else
                        {
                            foreach (var catdef in nodes[1].ChildNodes)
                            {
                                string name = (string)catdef.Token.Value;
                                categories.Add(name);
                                catsAsIdentifiers.Add(name, (catdef.AstNode is DarlMetaIdentifierNode));
                            }
                        }
                    }
                    break;
            }
        }

        /// <summary>
        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentOutput">The current output.</param>
        public override void WalkSaliences(double saliency, MetaRootNode root)
        {
            Salience += saliency;
        }
    }
}