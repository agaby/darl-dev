// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="NonTerminal.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Ast;
using System;
using System.Collections.Generic;

namespace DarlCompiler.Parsing
{

    /// <summary>
    /// Class IntList.
    /// </summary>
    internal class IntList : List<int> { }

    /// <summary>
    /// Class NonTerminal.
    /// </summary>
    public partial class NonTerminal : BnfTerm
    {

        #region constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BnfTerm" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public NonTerminal(string name) : base(name, null) { }  //by default display name is null
        /// <summary>
        /// Initializes a new instance of the <see cref="BnfTerm" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="errorAlias">The error alias.</param>
        public NonTerminal(string name, string errorAlias) : base(name, errorAlias) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="BnfTerm" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="errorAlias">The error alias.</param>
        /// <param name="nodeType">Type of the node.</param>
        public NonTerminal(string name, string errorAlias, Type nodeType) : base(name, errorAlias, nodeType) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="BnfTerm" /> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="errorAlias">The error alias.</param>
        /// <param name="nodeCreator">The node creator.</param>
        public NonTerminal(string name, string errorAlias, AstNodeCreator nodeCreator) : base(name, errorAlias, nodeCreator) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="NonTerminal"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="nodeType">Type of the node.</param>
        public NonTerminal(string name, Type nodeType) : base(name, null, nodeType) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="NonTerminal"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="nodeCreator">The node creator.</param>
        public NonTerminal(string name, AstNodeCreator nodeCreator) : base(name, null, nodeCreator) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="NonTerminal"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="expression">The expression.</param>
        public NonTerminal(string name, BnfExpression expression)
            : this(name)
        {
            Rule = expression;
        }
        #endregion

        #region properties/fields: Rule, ErrorRule

        /// <summary>
        /// The rule
        /// </summary>
        public BnfExpression Rule;
        //Separate property for specifying error expressions. This allows putting all such expressions in a separate section
        // in grammar for all non-terminals. However you can still put error expressions in the main Rule property, just like
        // in YACC
        /// <summary>
        /// The error rule
        /// </summary>
        public BnfExpression ErrorRule;

        //A template for representing ParseTreeNode in the parse tree. Can contain '#{i}' fragments referencing 
        // child nodes by index
        /// <summary>
        /// The node caption template
        /// </summary>
        public string NodeCaptionTemplate;
        //Converted template with index list
        /// <summary>
        /// The _converted template
        /// </summary>
        private string _convertedTemplate;
        /// <summary>
        /// The _caption parameters
        /// </summary>
        private IntList _captionParameters;

        // Productions are used internally by Parser builder
        /// <summary>
        /// The productions
        /// </summary>
        internal ProductionList Productions = new ProductionList();
        #endregion

        #region Events: Reduced
        //Note that Reduced event may be called more than once for a List node 
        /// <summary>
        /// Occurs when [reduced].
        /// </summary>
        public event EventHandler<ReducedEventArgs> Reduced;
        /// <summary>
        /// Called when [reduced].
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="reducedProduction">The reduced production.</param>
        /// <param name="resultNode">The result node.</param>
        internal void OnReduced(ParsingContext context, Production reducedProduction, ParseTreeNode resultNode)
        {
            if (Reduced != null)
                Reduced(this, new ReducedEventArgs(context, reducedProduction, resultNode));
        }
        #endregion

        #region overrides: ToString, Init
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Name;
        }
        /// <summary>
        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public override void Init(GrammarData grammarData)
        {
            base.Init(grammarData);
            if (!string.IsNullOrEmpty(NodeCaptionTemplate))
                ConvertNodeCaptionTemplate();
        }
        #endregion

        #region Grammar hints
        // Adds a hint at the end of all productions
        /// <summary>
        /// Adds the hint to all.
        /// </summary>
        /// <param name="hint">The hint.</param>
        /// <exception cref="System.Exception">Rule property must be set on non-terminal before calling AddHintToAll.</exception>
        public void AddHintToAll(GrammarHint hint)
        {
            if (this.Rule == null)
                throw new Exception("Rule property must be set on non-terminal before calling AddHintToAll.");
            foreach (var plusList in this.Rule.Data)
                plusList.Add(hint);
        }

        #endregion

        #region NodeCaptionTemplate utilities
        //We replace original tag '#{i}'  (where i is the index of the child node to put here)
        // with the tag '{k}', where k is the number of the parameter. So after conversion the template can 
        // be used in string.Format() call, with parameters set to child nodes captions
        /// <summary>
        /// Converts the node caption template.
        /// </summary>
        private void ConvertNodeCaptionTemplate()
        {
            _captionParameters = new IntList();
            _convertedTemplate = NodeCaptionTemplate;
            var index = 0;
            while (index < 100)
            {
                var strParam = "#{" + index + "}";
                if (_convertedTemplate.Contains(strParam))
                {
                    _convertedTemplate = _convertedTemplate.Replace(strParam, "{" + _captionParameters.Count + "}");
                    _captionParameters.Add(index);
                }
                if (!_convertedTemplate.Contains("#{")) return;
                index++;
            }
        }

        /// <summary>
        /// Gets the node caption.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>System.String.</returns>
        public string GetNodeCaption(ParseTreeNode node)
        {
            var paramValues = new string[_captionParameters.Count];
            for (int i = 0; i < _captionParameters.Count; i++)
            {
                var childIndex = _captionParameters[i];
                if (childIndex < node.ChildNodes.Count)
                {
                    var child = node.ChildNodes[childIndex];
                    //if child is a token, then child.ToString returns token.ToString which contains Value + Term; 
                    // in this case we prefer to have Value only
                    paramValues[i] = (child.Token != null ? child.Token.ValueString : child.ToString());
                }
            }
            var result = string.Format(_convertedTemplate, paramValues);
            return result;
        }
        #endregion

    }

    /// <summary>
    /// Class NonTerminalList.
    /// </summary>
    public class NonTerminalList : List<NonTerminal>
    {
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Join(" ", this);
        }
    }

    /// <summary>
    /// Class NonTerminalSet.
    /// </summary>
    [Serializable]
    public class NonTerminalSet : HashSet<NonTerminal>
    {
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.Join(" ", this);
        }
    }


}
