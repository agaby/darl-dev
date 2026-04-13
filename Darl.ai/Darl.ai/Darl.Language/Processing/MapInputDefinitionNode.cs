/// <summary>
/// MapInputDefinitionNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Ast;
using DarlCompiler.Parsing;


namespace DarlLanguage.Processing
{
    /// <summary>
    /// Implements a map input definition
    /// </summary>
    public class MapInputDefinitionNode : DarlNode
    {
        /// <summary>
        /// Gets the name of the input.
        /// </summary>
        /// <value>
        /// The name of the input.
        /// </value>
        public string Name { get; set; }
        /// <summary>
        /// Gets the path of the input.
        /// </summary>
        /// <value>
        /// The path of the input.
        /// </value>
        public string Path { get; private set; }
        /// <summary>
        /// Gets the salience.
        /// </summary>
        /// <value>
        /// The salience.
        /// </value>
        public double Salience { get; internal set; }

        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public override void Init(AstContext context, ParseTreeNode treeNode)
        {
            base.Init(context, treeNode);
            var nodes = treeNode.GetMappedChildNodes();
            Name = (string)nodes[0].Token.Value;
            if (nodes.Count == 2)
                Path = (string)nodes[1].Token.Value;
        }

        /// <summary>
        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentRuleSet">The current rule set.</param>
        /// <param name="currentOutput">The current output.</param>
        public override void WalkSaliences(double saliency, MapRootNode root, string currentRuleSet, string currentOutput)
        {
            Salience += saliency;
        }

        /// <summary>
        /// prototype input for GP
        /// </summary>
        public InputDefinitionNode inputPrototype { get; set; }

        public override string preamble
        {
            get
            {
                return $"mapinput {Name};";
            }
        }
    }
}
