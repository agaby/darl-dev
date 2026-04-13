/// <summary>
/// DarlMetaNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Interpreter;
using DarlCompiler.Interpreter.Ast;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Darl.Thinkbase.Meta
{
    public abstract class DarlMetaNode : AstNode
    {
        /// <summary>
        /// Gets the preamble.
        /// </summary>
        /// <value>
        /// The preamble, used to reconstruct the source code.
        /// </value>
        public virtual string preamble { get { return string.Empty; } }
        /// <summary>
        /// Gets the midamble.
        /// </summary>
        /// <value>
        /// The midamble, used to reconstruct the source code.
        /// </value>
        public virtual string midamble { get { return string.Empty; } }
        /// <summary>
        /// Gets the postamble.
        /// </summary>
        /// <value>
        /// The postamble, used to reconstruct the source code.
        /// </value>
        public virtual string postamble { get { return string.Empty; } }

        public virtual string GetName()
        {
            return String.Empty;
        }

        /// <summary>
        /// Establishes dependencies and initializes constants
        /// </summary>
        /// <param name="dependencies">list of dependencies discovered</param>
        /// <param name="currentOutput">output for the rule being walked</param>
        /// <param name="context">The context.</param>
        public virtual void WalkDependencies(List<IntraSetDependency> dependencies, DarlMetaNode? currentOutput, ConstantContext context, IGraphModel model, GraphObject currentNode)
        {

        }

        /// <summary>
        /// Walks the saliences.
        /// </summary>
        /// <param name="saliency">The incoming saliency.</param>
        /// <param name="root">The map root.</param>
        /// <param name="currentOutput">The current output.</param>
        public virtual void WalkSaliences(double saliency, MetaRootNode root)
        {
        }

        public void Prologue(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            thread.CheckpointExecution();
        }

        public void Epilogue(ScriptThread thread, DarlResult res)
        {
            thread.RecordExecution(res, this);
            thread.CurrentNode = Parent;
        }
    }
}
