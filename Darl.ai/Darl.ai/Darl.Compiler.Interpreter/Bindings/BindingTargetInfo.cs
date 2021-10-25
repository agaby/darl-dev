// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="BindingTargetInfo.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
/// <summary>
/// The Interpreter namespace.
/// </summary>
namespace DarlCompiler.Interpreter
{

    public enum BindingTargetType
    {
        /// <summary>
        /// The slot
        /// </summary>
        Slot,
        /// <summary>
        /// The built in object
        /// </summary>
        BuiltInObject,
        /// <summary>
        /// The special form
        /// </summary>
        SpecialForm,
        /// <summary>
        /// The color interop
        /// </summary>
        ClrInterop,
        /// <summary>
        /// The custom
        /// </summary>
        Custom, // any special non-standard type for specific language
    }


    /// <summary>
    /// Class BindingTargetInfo.
    /// </summary>
    public class BindingTargetInfo
    {
        /// <summary>
        /// The symbol
        /// </summary>
        public readonly string Symbol;
        /// <summary>
        /// The type
        /// </summary>
        public readonly BindingTargetType Type;
        /// <summary>
        /// Initializes a new instance of the <see cref="BindingTargetInfo"/> class.
        /// </summary>
        /// <param name="symbol">The symbol.</param>
        /// <param name="type">The type.</param>
        public BindingTargetInfo(string symbol, BindingTargetType type)
        {
            Symbol = symbol;
            Type = type;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return Symbol + "/" + Type.ToString();
        }

    }


}
