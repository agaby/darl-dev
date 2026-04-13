// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="Scope.cs" company="Dr Andy's IP LLC">
//     Copyright   2015
// </copyright>
// <summary></summary>
// ***********************************************************************
namespace DarlCompiler.Interpreter
{

    /// Class Scope.
    /// </summary>
    public class Scope : ScopeBase
    {
        /// The parameters
        /// </summary>
        public object[] Parameters;
        /// The caller
        /// </summary>
        public Scope Caller;
        /// The creator
        /// </summary>
        public Scope Creator; //either caller or closure parent
        /// The _parent
        /// </summary>
        private Scope _parent; //computed on demand

        /// Initializes a new instance of the <see cref="Scope"/> class.
        /// </summary>
        /// <param name="scopeInfo">The scope information.</param>
        /// <param name="caller">The caller.</param>
        /// <param name="creator">The creator.</param>
        /// <param name="parameters">The parameters.</param>
        public Scope(ScopeInfo scopeInfo, Scope caller, Scope creator, object[] parameters)
            : base(scopeInfo)
        {
            Caller = caller;
            Creator = creator;
            Parameters = parameters;
        }

        /// Gets the parameters.
        /// </summary>
        /// <returns>System.Object[].</returns>
        public object[] GetParameters()
        {
            return Parameters;
        }

        /// Gets the parameter.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns>System.Object.</returns>
        public object GetParameter(int index)
        {
            return Parameters[index];
        }
        /// Sets the parameter.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <param name="value">The value.</param>
        public void SetParameter(int index, object value)
        {
            Parameters[index] = value;
        }

        // Lexical parent, computed on demand
        /// Gets or sets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public Scope Parent
        {
            get
            {
                if (_parent == null)
                    _parent = GetParent();
                return _parent;
            }
            set { _parent = value; }
        }

        /// Gets the parent.
        /// </summary>
        /// <returns>Scope.</returns>
        protected Scope GetParent()
        {
            // Walk along creators chain and find a scope with ScopeInfo matching this.ScopeInfo.Parent
            var parentScopeInfo = Info.Parent;
            if (parentScopeInfo == null)
                return null;
            var current = Creator;
            while (current != null)
            {
                if (current.Info == parentScopeInfo)
                    return current;
                current = current.Creator;
            }
            return null;
        }// method

    }

}
