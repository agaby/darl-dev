// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="Binding.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Interpreter.Ast;
using System.Threading.Tasks;

namespace DarlCompiler.Interpreter
{


    // Binding is a link between a variable in the script (for ex, IdentifierNode) and a value storage  - 
    // a slot in local or module-level Scope. Binding to internal variables is supported by SlotBinding class. 
    // Alternatively a symbol can be bound to external CLR entity in imported namespace - class, function, property, etc.
    // Binding is produced by Runtime.Bind method and allows read/write operations through GetValueRef and SetValueRef methods. 
    /// <summary>
    /// Class Binding.
    /// </summary>
    public class Binding
    {
        /// <summary>
        /// The target information
        /// </summary>
        public readonly BindingTargetInfo TargetInfo;
        /// <summary>
        /// The get value reference
        /// </summary>
        public EvaluateMethod GetValueRef;     // ref to Getter method implementation
        /// <summary>
        /// The set value reference
        /// </summary>
        public ValueSetterMethod SetValueRef;  // ref to Setter method implementation
        /// <summary>
        /// Gets or sets a value indicating whether this instance is constant.
        /// </summary>
        /// <value><c>true</c> if this instance is constant; otherwise, <c>false</c>.</value>
        public bool IsConstant { get; protected set; }
        /// <summary>
        /// Initializes a new instance of the <see cref="Binding"/> class.
        /// </summary>
        /// <param name="targetInfo">The target information.</param>
        public Binding(BindingTargetInfo targetInfo)
        {
            TargetInfo = targetInfo;
        }
        /// <summary>
        /// Initializes a new instance of the <see cref="Binding"/> class.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="targetType">Type of the target.</param>
        public Binding(string symbol, BindingTargetType targetType)
        {
            TargetInfo = new BindingTargetInfo(symbol, targetType);
        }
        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return "{Binding to + " + TargetInfo.ToString() + "}";
        }
    }

    

}
