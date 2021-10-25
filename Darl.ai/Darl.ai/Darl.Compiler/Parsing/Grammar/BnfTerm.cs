// ***********************************************************************
// Assembly         : DarlCompiler
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="BnfTerm.cs" company="Dr Andy's IP LLC">
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
    /// Enum TermFlags
    /// </summary>
    [Flags]
    public enum TermFlags
    {
        /// <summary>
        /// The none
        /// </summary>
        None = 0,
        /// <summary>
        /// The is operator
        /// </summary>
        IsOperator = 0x01,
        /// <summary>
        /// The is open brace
        /// </summary>
        IsOpenBrace = 0x02,
        /// <summary>
        /// The is close brace
        /// </summary>
        IsCloseBrace = 0x04,
        /// <summary>
        /// The is brace
        /// </summary>
        IsBrace = IsOpenBrace | IsCloseBrace,
        /// <summary>
        /// The is literal
        /// </summary>
        IsLiteral = 0x08,

        /// <summary>
        /// The is constant
        /// </summary>
        IsConstant = 0x10,
        /// <summary>
        /// The is punctuation
        /// </summary>
        IsPunctuation = 0x20,
        /// <summary>
        /// The is delimiter
        /// </summary>
        IsDelimiter = 0x40,
        /// <summary>
        /// The is reserved word
        /// </summary>
        IsReservedWord = 0x080,
        /// <summary>
        /// The is member select
        /// </summary>
        IsMemberSelect = 0x100,
        /// <summary>
        /// The inherit precedence
        /// </summary>
        InheritPrecedence = 0x200, // Signals that non-terminal must inherit precedence and assoc values from its children. 
        // Typically set for BinOp nonterminal (where BinOp.Rule = '+' | '-' | ...) 

        /// <summary>
        /// The is non scanner
        /// </summary>
        IsNonScanner = 0x01000,  // indicates that tokens for this terminal are NOT produced by scanner 
        /// <summary>
        /// The is non grammar
        /// </summary>
        IsNonGrammar = 0x02000,  // if set, parser would eliminate the token from the input stream; terms in Grammar.NonGrammarTerminals have this flag set
        /// <summary>
        /// The is transient
        /// </summary>
        IsTransient = 0x04000,  // Transient non-terminal - should be replaced by it's child in the AST tree.
        /// <summary>
        /// The is not reported
        /// </summary>
        IsNotReported = 0x08000,  // Exclude from expected terminals list on syntax error

        //calculated flags
        /// <summary>
        /// The is nullable
        /// </summary>
        IsNullable = 0x010000,
        /// <summary>
        /// The is visible
        /// </summary>
        IsVisible = 0x020000,
        /// <summary>
        /// The is keyword
        /// </summary>
        IsKeyword = 0x040000,
        /// <summary>
        /// The is multiline
        /// </summary>
        IsMultiline = 0x100000,
        //internal flags
        /// <summary>
        /// The is list
        /// </summary>
        IsList = 0x200000,
        /// <summary>
        /// The is list container
        /// </summary>
        IsListContainer = 0x400000,
        //Indicates not to create AST node; mainly to suppress warning message on some special nodes that AST node type is not specified
        //Automatically set by MarkTransient method
        /// <summary>
        /// The no ast node
        /// </summary>
        NoAstNode = 0x800000,
        //A flag to suppress automatic AST creation for child nodes in global AST construction. Will be used to supress full 
        // "compile" of method bodies in modules. The module might be large, but the running code might 
        // be actually using only a few methods or global members; so in this case it makes sense to "compile" only global/public
        // declarations, including method headers but not full bodies. The body will be compiled on the first call. 
        // This makes even more sense when processing module imports. 
        /// <summary>
        /// The ast delay children
        /// </summary>
        AstDelayChildren = 0x1000000,

    }

    //Basic Backus-Naur Form element. Base class for Terminal, NonTerminal, BnfExpression, GrammarHint
    /// <summary>
    /// Class BnfTerm.
    /// </summary>
    public abstract class BnfTerm
    {
        #region consructors
        /// <summary>
        /// Initializes a new instance of the <see cref="BnfTerm"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        public BnfTerm(string name) : this(name, name) { }
        /// <summary>
        /// Initializes a new instance of the <see cref="BnfTerm"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="errorAlias">The error alias.</param>
        /// <param name="nodeType">Type of the node.</param>
        public BnfTerm(string name, string errorAlias, Type nodeType)
            : this(name, errorAlias)
        {
            AstConfig.NodeType = nodeType;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BnfTerm"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="errorAlias">The error alias.</param>
        /// <param name="nodeCreator">The node creator.</param>
        public BnfTerm(string name, string errorAlias, AstNodeCreator nodeCreator)
            : this(name, errorAlias)
        {
            AstConfig.NodeCreator = nodeCreator;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="BnfTerm"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="errorAlias">The error alias.</param>
        public BnfTerm(string name, string errorAlias)
        {
            Name = name;
            ErrorAlias = errorAlias;
            _hashCode = (_hashCounter++).GetHashCode();
        }
        #endregion

        #region virtuals and overrides
        /// <summary>
        /// Initializes the specified grammar data.
        /// </summary>
        /// <param name="grammarData">The grammar data.</param>
        public virtual void Init(GrammarData grammarData)
        {
            GrammarData = grammarData;
        }

        /// <summary>
        /// Gets the parse node caption.
        /// </summary>
        /// <param name="node">The node.</param>
        /// <returns>System.String.</returns>
        public virtual string GetParseNodeCaption(ParseTreeNode node)
        {
            if (GrammarData != null)
                return GrammarData.Grammar.GetParseNodeCaption(node);
            else
                return Name;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Name;
        }

        //Hash code - we use static counter to generate hash codes
        /// <summary>
        /// The _hash counter
        /// </summary>
        private static int _hashCounter;
        /// <summary>
        /// The _hash code
        /// </summary>
        private readonly int _hashCode;
        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }
        #endregion

        /// <summary>
        /// The no precedence
        /// </summary>
        public const int NoPrecedence = 0;

        #region properties: Name, DisplayName, Key, Options
        /// <summary>
        /// The name
        /// </summary>
        public string Name;

        //ErrorAlias is used in error reporting, e.g. "Syntax error, expected <list-of-display-names>". 
        /// <summary>
        /// The error alias
        /// </summary>
        public string ErrorAlias;
        /// <summary>
        /// The flags
        /// </summary>
        public TermFlags Flags;
        /// <summary>
        /// The grammar data
        /// </summary>
        protected GrammarData GrammarData;
        /// <summary>
        /// The precedence
        /// </summary>
        public int Precedence = NoPrecedence;
        /// <summary>
        /// The associativity
        /// </summary>
        public Associativity Associativity = Associativity.Neutral;

        /// <summary>
        /// Gets the grammar.
        /// </summary>
        /// <value>The grammar.</value>
        public Grammar Grammar
        {
            get { return GrammarData.Grammar; }
        }
        /// <summary>
        /// Sets the flag.
        /// </summary>
        /// <param name="flag">The flag.</param>
        public void SetFlag(TermFlags flag)
        {
            SetFlag(flag, true);
        }
        /// <summary>
        /// Sets the flag.
        /// </summary>
        /// <param name="flag">The flag.</param>
        /// <param name="value">if set to <c>true</c> [value].</param>
        public void SetFlag(TermFlags flag, bool value)
        {
            if (value)
                Flags |= flag;
            else
                Flags &= ~flag;
        }

        #endregion

        #region events: Shifting
        /// <summary>
        /// Occurs when [shifting].
        /// </summary>
        public event EventHandler<ParsingEventArgs> Shifting;
        /// <summary>
        /// Occurs when [ast node created].
        /// </summary>
        public event EventHandler<AstNodeEventArgs> AstNodeCreated; //an event fired after AST node is created. 

        /// <summary>
        /// Handles the <see cref="E:Shifting" /> event.
        /// </summary>
        /// <param name="args">The <see cref="ParsingEventArgs"/> instance containing the event data.</param>
        protected internal void OnShifting(ParsingEventArgs args)
        {
            if (Shifting != null)
                Shifting(this, args);
        }

        /// <summary>
        /// Called when [ast node created].
        /// </summary>
        /// <param name="parseNode">The parse node.</param>
        protected internal void OnAstNodeCreated(ParseTreeNode parseNode)
        {
            if (this.AstNodeCreated == null || parseNode.AstNode == null) return;
            AstNodeEventArgs args = new AstNodeEventArgs(parseNode);
            AstNodeCreated(this, args);
        }

        #endregion

        #region AST node creations: AstNodeType, AstNodeCreator, AstNodeCreated
        //We autocreate AST config on first GET;
        /// <summary>
        /// Gets or sets the ast configuration.
        /// </summary>
        /// <value>The ast configuration.</value>
        public AstNodeConfig AstConfig
        {
            get
            {
                if (_astConfig == null)
                    _astConfig = new Ast.AstNodeConfig();
                return _astConfig;
            }
            set { _astConfig = value; }

        }
        AstNodeConfig _astConfig;

        /// <summary>
        /// Determines whether [has ast configuration].
        /// </summary>
        /// <returns><c>true</c> if [has ast configuration]; otherwise, <c>false</c>.</returns>
        public bool HasAstConfig()
        {
            return _astConfig != null;
        }

        #endregion


        #region Kleene operator Q()
        /// <summary>
        /// The _q
        /// </summary>
        NonTerminal _q;
        /// <summary>
        /// qs this instance.
        /// </summary>
        /// <returns>BnfExpression.</returns>
        public BnfExpression Q()
        {
            if (_q != null)
                return _q;
            _q = new NonTerminal(this.Name + "?");
            _q.Rule = this | Grammar.CurrentGrammar.Empty;
            return _q;
        }
        #endregion

        #region Operators: +, |, implicit
        /// <summary>
        /// Implements the +.
        /// </summary>
        /// <param name="term1">The term1.</param>
        /// <param name="term2">The term2.</param>
        /// <returns>The result of the operator.</returns>
        public static BnfExpression operator +(BnfTerm term1, BnfTerm term2)
        {
            return Op_Plus(term1, term2);
        }
        /// <summary>
        /// Implements the +.
        /// </summary>
        /// <param name="term1">The term1.</param>
        /// <param name="symbol2">The symbol2.</param>
        /// <returns>The result of the operator.</returns>
        public static BnfExpression operator +(BnfTerm term1, string symbol2)
        {
            return Op_Plus(term1, Grammar.CurrentGrammar.ToTerm(symbol2));
        }
        /// <summary>
        /// Implements the +.
        /// </summary>
        /// <param name="symbol1">The symbol1.</param>
        /// <param name="term2">The term2.</param>
        /// <returns>The result of the operator.</returns>
        public static BnfExpression operator +(string symbol1, BnfTerm term2)
        {
            return Op_Plus(Grammar.CurrentGrammar.ToTerm(symbol1), term2);
        }

        //Alternative 
        /// <summary>
        /// Implements the |.
        /// </summary>
        /// <param name="term1">The term1.</param>
        /// <param name="term2">The term2.</param>
        /// <returns>The result of the operator.</returns>
        public static BnfExpression operator |(BnfTerm term1, BnfTerm term2)
        {
            return Op_Pipe(term1, term2);
        }
        /// <summary>
        /// Implements the |.
        /// </summary>
        /// <param name="term1">The term1.</param>
        /// <param name="symbol2">The symbol2.</param>
        /// <returns>The result of the operator.</returns>
        public static BnfExpression operator |(BnfTerm term1, string symbol2)
        {
            return Op_Pipe(term1, Grammar.CurrentGrammar.ToTerm(symbol2));
        }
        /// <summary>
        /// Implements the |.
        /// </summary>
        /// <param name="symbol1">The symbol1.</param>
        /// <param name="term2">The term2.</param>
        /// <returns>The result of the operator.</returns>
        public static BnfExpression operator |(string symbol1, BnfTerm term2)
        {
            return Op_Pipe(Grammar.CurrentGrammar.ToTerm(symbol1), term2);
        }

        //BNF operations implementation -----------------------
        // Plus/sequence
        /// <summary>
        /// Op_s the plus.
        /// </summary>
        /// <param name="term1">The term1.</param>
        /// <param name="term2">The term2.</param>
        /// <returns>BnfExpression.</returns>
        internal static BnfExpression Op_Plus(BnfTerm term1, BnfTerm term2)
        {
            //Check term1 and see if we can use it as result, simply adding term2 as operand
            BnfExpression expr1 = term1 as BnfExpression;
            if (expr1 == null || expr1.Data.Count > 1) //either not expression at all, or Pipe-type expression (count > 1)
                expr1 = new BnfExpression(term1);
            expr1.Data[expr1.Data.Count - 1].Add(term2);
            return expr1;
        }

        //Pipe/Alternative
        //New version proposed by the codeplex user bdaugherty
        /// <summary>
        /// Op_s the pipe.
        /// </summary>
        /// <param name="term1">The term1.</param>
        /// <param name="term2">The term2.</param>
        /// <returns>BnfExpression.</returns>
        internal static BnfExpression Op_Pipe(BnfTerm term1, BnfTerm term2)
        {
            BnfExpression expr1 = term1 as BnfExpression;
            if (expr1 == null)
                expr1 = new BnfExpression(term1);
            BnfExpression expr2 = term2 as BnfExpression;
            if (expr2 == null)
                expr2 = new BnfExpression(term2);
            expr1.Data.AddRange(expr2.Data);
            return expr1;
        }


        #endregion


    }

    /// <summary>
    /// Class BnfTermList.
    /// </summary>
    public class BnfTermList : List<BnfTerm> { }
    /// <summary>
    /// Class BnfTermSet.
    /// </summary>
    [Serializable]
    public class BnfTermSet : HashSet<BnfTerm> { }



}

