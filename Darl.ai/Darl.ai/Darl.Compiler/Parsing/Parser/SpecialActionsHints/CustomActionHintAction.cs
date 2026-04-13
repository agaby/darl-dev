/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="CustomActionHintAction.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.ai;
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{

    //These two delegates define custom methods that Grammar can implement to execute custom action
    /// Delegate PreviewActionMethod
    /// </summary>
    /// <param name="action">The action.</param>
    public delegate void PreviewActionMethod(CustomParserAction action);
    /// Delegate ExecuteActionMethod
    /// </summary>
    /// <param name="context">The context.</param>
    /// <param name="action">The action.</param>
    public delegate void ExecuteActionMethod(ParsingContext context, CustomParserAction action);

    /// Class CustomActionHint.
    /// </summary>
    public class CustomActionHint : GrammarHint
    {
        /// The _execute method
        /// </summary>
        private readonly ExecuteActionMethod _executeMethod;
        /// The _preview method
        /// </summary>
        private readonly PreviewActionMethod _previewMethod;

        /// Initializes a new instance of the <see cref="CustomActionHint"/> class.
        /// </summary>
        /// <param name="executeMethod">The execute method.</param>
        /// <param name="previewMethod">The preview method.</param>
        public CustomActionHint(ExecuteActionMethod executeMethod, PreviewActionMethod? previewMethod = null)
        {
            _executeMethod = executeMethod;
            _previewMethod = previewMethod;
        }

        /// Gives a chance to a custom code in hint to interfere in parser automaton construction.
        /// </summary>
        /// <param name="language">The LanguageData instance.</param>
        /// <param name="owner">The LRItem that "owns" the hint.</param>
        /// <remarks>The most common purpose of this method (it's overrides) is to resolve the conflicts
        /// by adding specific actions into State.Actions dictionary.
        /// The owner parameter represents the position in the grammar expression where the hint
        /// is found. The parser state is available through owner.State property.</remarks>
        public override void Apply(LanguageData language, Construction.LRItem owner)
        {
            //Create custom action and put it into state.Actions table
            var state = owner.State;
            var action = new CustomParserAction(language, state, _executeMethod);
            if (_previewMethod != null)
                _previewMethod(action);
            if (!state.BuilderData.IsInadequate) // adequate state, with a single possible action which is DefaultAction
                state.DefaultAction = action;
            else if (owner.Core.Current != null) //shift action
                state.Actions[owner.Core.Current] = action;
            else foreach (var lkh in owner.Lookaheads)
                    state.Actions[lkh] = action;
            //We consider all conflicts handled by the action
            state.BuilderData.Conflicts.Clear();
        }

    }


    // CustomParserAction is in fact action selector: it allows custom Grammar code to select the action to execute from a set of 
    // shift/reduce actions available in this state.
    /// Class CustomParserAction.
    /// </summary>
    public class CustomParserAction : ParserAction
    {
        /// The language
        /// </summary>
        public LanguageData Language;
        /// The state
        /// </summary>
        public ParserState State;
        /// The execute reference
        /// </summary>
        public ExecuteActionMethod ExecuteRef;
        /// The conflicts
        /// </summary>
        public TerminalSet Conflicts = new TerminalSet();
        /// The shift actions
        /// </summary>
        public IList<ShiftParserAction> ShiftActions = new List<ShiftParserAction>();
        /// The reduce actions
        /// </summary>
        public IList<ReduceParserAction> ReduceActions = new List<ReduceParserAction>();
        /// The custom data
        /// </summary>
        public object CustomData;

        /// Initializes a new instance of the <see cref="CustomParserAction"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        /// <param name="state">The state.</param>
        /// <param name="executeRef">The execute reference.</param>
        public CustomParserAction(LanguageData language, ParserState state,
                                                ExecuteActionMethod executeRef)
        {
            Language = language;
            State = state;
            ExecuteRef = executeRef;
            Conflicts.UnionWith(state.BuilderData.Conflicts);
            // Create default shift and reduce actions
            foreach (var shiftItem in state.BuilderData.ShiftItems)
                ShiftActions.Add(new ShiftParserAction(shiftItem));
            foreach (var item in state.BuilderData.ReduceItems)
                ReduceActions.Add(ReduceParserAction.Create(item.Core.Production));
        }

        /// Executes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        public override void Execute(ParsingContext context)
        {
            if (context.TracingEnabled)
                context.AddTrace(Resources.MsgTraceExecCustomAction);
            //States with DefaultAction do NOT read input, so we read it here
            if (context.CurrentParserInput == null)
                context.Parser.ReadInput();
            // Remember old state and input; if they don't change after custom action - it is error, we may fall into an endless loop
            var oldState = context.CurrentParserState;
            var oldInput = context.CurrentParserInput;
            ExecuteRef(context, this);
            //Prevent from falling into an infinite loop 
            if (context.CurrentParserState == oldState && context.CurrentParserInput == oldInput)
            {
                context.AddParserError(Resources.MsgErrorCustomActionDidNotAdvance);
                context.Parser.RecoverFromError();
            }
        }

        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return "CustomParserAction";
        }
    }
}
