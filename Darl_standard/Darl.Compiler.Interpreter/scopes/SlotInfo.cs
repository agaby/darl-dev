// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="SlotInfo.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;
namespace DarlCompiler.Interpreter
{

    /// <summary>
    /// Enum SlotType
    /// </summary>
    public enum SlotType
    {
        /// <summary>
        /// The value
        /// </summary>
        Value,     //local or property value
        /// <summary>
        /// The parameter
        /// </summary>
        Parameter, //function parameter
        /// <summary>
        /// The function
        /// </summary>
        Function,
        /// <summary>
        /// The closure
        /// </summary>
        Closure,
    }

    /// <summary>
    /// Describes a variable.
    /// </summary>
    public class SlotInfo
    {
        /// <summary>
        /// The scope information
        /// </summary>
        public readonly ScopeInfo ScopeInfo;
        /// <summary>
        /// The type
        /// </summary>
        public readonly SlotType Type;
        /// <summary>
        /// The name
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The index
        /// </summary>
        public readonly int Index;
        /// <summary>
        /// The is public
        /// </summary>
        public bool IsPublic = true; //for module-level slots, indicator that symbol is "exported" and visible by code that imports the module
        /// <summary>
        /// Initializes a new instance of the <see cref="SlotInfo"/> class.
        /// </summary>
        /// <param name="scopeInfo">The scope information.</param>
        /// <param name="type">The type.</param>
        /// <param name="name">The name.</param>
        /// <param name="index">The index.</param>
        internal SlotInfo(ScopeInfo scopeInfo, SlotType type, string name, int index)
        {
            ScopeInfo = scopeInfo;
            Type = type;
            Name = name;
            Index = index;
        }
    }

    /// <summary>
    /// Class SlotInfoDictionary.
    /// </summary>
    [Serializable]
    public class SlotInfoDictionary : Dictionary<string, SlotInfo>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SlotInfoDictionary"/> class.
        /// </summary>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
        public SlotInfoDictionary(bool caseSensitive)
            : base(32, caseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase) { }
    }

}
