// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="AstNode.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

using System.Collections.Generic;
using System.Linq.Expressions;
using DarlCompiler.Ast;
using DarlCompiler.Parsing;
using System.Threading.Tasks;

namespace DarlCompiler.Interpreter.Ast
{

    /// <summary>
    /// Class CustomExpressionTypes.
    /// </summary>
    public static class CustomExpressionTypes
    {
        /// <summary>
        /// The not an expression
        /// </summary>
        public const ExpressionType NotAnExpression = (ExpressionType)(-1);
    }

    /// <summary>
    /// Class AstNodeList.
    /// </summary>
    public class AstNodeList : List<AstNode> { }

    //Base AST node class
    /// <summary>
    /// Class AstNode.
    /// </summary>
    public partial class AstNode : IAstNodeInit, IBrowsableAstNode, IVisitableNode
    {
        /// <summary>
        /// The parent
        /// </summary>
        public AstNode Parent;
        /// <summary>
        /// The term
        /// </summary>
        public BnfTerm Term;
        /// <summary>
        /// Gets or sets the span.
        /// </summary>
        /// <value>The span.</value>
        public SourceSpan Span { get; set; }
        /// <summary>
        /// The flags
        /// </summary>
        public AstNodeFlags Flags;
        /// <summary>
        /// The expression type
        /// </summary>
        protected ExpressionType ExpressionType = CustomExpressionTypes.NotAnExpression;
        //Used for pointing to error location. For most nodes it would be the location of the node itself.
        // One exception is BinExprNode: when we get "Division by zero" error evaluating 
        //  x = (5 + 3) / (2 - 2)
        // it is better to point to "/" as error location, rather than the first "(" - which is the start 
        // location of binary expression. 
        /// <summary>
        /// The error anchor
        /// </summary>
        public SourceLocation ErrorAnchor;
        //UseType is set by parent
        /// <summary>
        /// The use type
        /// </summary>
        public NodeUseType UseType = NodeUseType.Unknown;
        // Role is a free-form string used as prefix in ToString() representation of the node. 
        // Node's parent can set it to "property name" or role of the child node in parent's node currentFrame.Context. 
        /// <summary>
        /// The role
        /// </summary>
        public string Role;
        // Default AstNode.ToString() returns 'Role: AsString', which is used for showing node in AST tree. 
        /// <summary>
        /// Gets or sets as string.
        /// </summary>
        /// <value>As string.</value>
        public virtual string AsString { get; protected set; }
        /// <summary>
        /// The child nodes
        /// </summary>
        public readonly AstNodeList ChildNodes = new AstNodeList();  //List of child nodes

        //Reference to Evaluate method implementation. Initially set to DoEvaluate virtual method. 
        /// <summary>
        /// The evaluate
        /// </summary>
        public EvaluateMethod Evaluate;

        // Public default constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="AstNode"/> class.
        /// </summary>
        public AstNode()
        {
            this.Evaluate = DoEvaluate;
        }
        /// <summary>
        /// Gets the location.
        /// </summary>
        /// <value>The location.</value>
        public SourceLocation Location { get { return Span.Location; } }

        #region IAstNodeInit Members
        /// <summary>
        /// Initializes the specified context.
        /// </summary>
        /// <param name="context">The context.</param>
        /// <param name="treeNode">The tree node.</param>
        public virtual void Init(AstContext context, ParseTreeNode treeNode)
        {
            this.Term = treeNode.Term;
            Span = treeNode.Span;
            ErrorAnchor = this.Location;
            treeNode.AstNode = this;
            AsString = (Term == null ? this.GetType().Name : Term.Name);
        }
        #endregion

        //ModuleNode - computed on demand
        /// <summary>
        /// Gets or sets the module node.
        /// </summary>
        /// <value>The module node.</value>
        public AstNode ModuleNode
        {
            get
            {
                if (_moduleNode == null)
                {
                    _moduleNode = (Parent == null) ? this : Parent.ModuleNode;
                }
                return _moduleNode;
            }
            set { _moduleNode = value; }
        }
        /// <summary>
        /// The _module node
        /// </summary>
        AstNode _moduleNode;


        #region virtual methods: DoEvaluate, SetValue, IsConstant, SetIsTail, GetDependentScopeInfo
        /// <summary>
        /// Resets this instance.
        /// </summary>
        public virtual void Reset()
        {
            _moduleNode = null;
            Evaluate = DoEvaluate;
            foreach (var child in ChildNodes)
                child.Reset();
        }

        //By default the Evaluate field points to this method.
        /// <summary>
        /// Does the evaluate.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>System.Object.</returns>
        protected virtual Task<object> DoEvaluate(ScriptThread thread)
        {
            //These 2 lines are standard prologue/epilogue statements. Place them in every Evaluate and SetValue implementations.
            thread.CurrentNode = this;  //standard prologue
            thread.CurrentNode = Parent; //standard epilogue
            return null;
        }

        /// <summary>
        /// Does the set value.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <param name="value">The value.</param>
        public virtual void DoSetValue(ScriptThread thread, object value)
        {
            //Place the prologue/epilogue lines in every implementation of SetValue method (see DoEvaluate above)
        }

        /// <summary>
        /// Determines whether this instance is constant.
        /// </summary>
        /// <returns><c>true</c> if this instance is constant; otherwise, <c>false</c>.</returns>
        public virtual bool IsConstant()
        {
            return false;
        }

        /// <summary>
        /// Sets a flag indicating that the node is in tail position. The value is propagated from parent to children.
        /// Should propagate this call to appropriate children.
        /// </summary>
        public virtual void SetIsTail()
        {
            Flags |= AstNodeFlags.IsTail;
        }

        /// <summary>
        /// Dependent scope is a scope produced by the node. For ex, FunctionDefNode defines a scope
        /// </summary>
        /// <value>The dependent scope information.</value>
        public virtual ScopeInfo DependentScopeInfo
        {
            get { return _dependentScope; }
            set { _dependentScope = value; }
        }
        /// <summary>
        /// The _dependent scope
        /// </summary>
        ScopeInfo _dependentScope;

        #endregion

        #region IBrowsableAstNode Members
        /// <summary>
        /// Gets the child nodes.
        /// </summary>
        /// <returns>System.Collections.IEnumerable.</returns>
        public virtual System.Collections.IEnumerable GetChildNodes()
        {
            return ChildNodes;
        }
        /// <summary>
        /// Gets the position.
        /// </summary>
        /// <value>The position.</value>
        public int Position
        {
            get { return Span.Location.Position; }
        }
        #endregion

        #region Visitors, Iterators
        //the first primitive Visitor facility
        /// <summary>
        /// Accepts the visitor.
        /// </summary>
        /// <param name="visitor">The visitor.</param>
        public virtual void AcceptVisitor(IAstVisitor visitor)
        {
            visitor.BeginVisit(this);
            if (ChildNodes.Count > 0)
                foreach (AstNode node in ChildNodes)
                    node.AcceptVisitor(visitor);
            visitor.EndVisit(this);
        }

        //Node traversal 
        /// <summary>
        /// Gets all.
        /// </summary>
        /// <returns>IEnumerable&lt;AstNode&gt;.</returns>
        public IEnumerable<AstNode> GetAll()
        {
            AstNodeList result = new AstNodeList();
            AddAll(result);
            return result;
        }
        /// <summary>
        /// Adds all.
        /// </summary>
        /// <param name="list">The list.</param>
        private void AddAll(AstNodeList list)
        {
            list.Add(this);
            foreach (AstNode child in this.ChildNodes)
                if (child != null)
                    child.AddAll(list);
        }
        #endregion

        #region overrides: ToString
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return string.IsNullOrEmpty(Role) ? AsString : Role + ": " + AsString;
        }
        #endregion

        #region Utility methods: AddChild, HandleError

        /// <summary>
        /// Adds the child.
        /// </summary>
        /// <param name="role">The role.</param>
        /// <param name="childParseNode">The child parse node.</param>
        /// <returns>AstNode.</returns>
        protected AstNode AddChild(string role, ParseTreeNode childParseNode)
        {
            return AddChild(NodeUseType.Unknown, role, childParseNode);
        }

        /// <summary>
        /// Adds the child.
        /// </summary>
        /// <param name="useType">Type of the use.</param>
        /// <param name="role">The role.</param>
        /// <param name="childParseNode">The child parse node.</param>
        /// <returns>AstNode.</returns>
        protected AstNode AddChild(NodeUseType useType, string role, ParseTreeNode childParseNode)
        {
            var child = (AstNode)childParseNode.AstNode;
            if (child == null)
                child = new NullNode(childParseNode.Term); //put a stub to throw an exception with clear message on attempt to evaluate. 
            child.Role = role;
            child.Parent = this;
            ChildNodes.Add(child);
            return child;
        }

        #endregion

    }
}
