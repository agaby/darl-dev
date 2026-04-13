/// </summary>

﻿// ***********************************************************************
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

    /// Enum SlotType
    /// </summary>
    public enum SlotType
    {
        /// The value
        /// </summary>
        Value,     //local or property value
        /// The parameter
        /// </summary>
        Parameter, //function parameter
        /// The function
        /// </summary>
        Function,
        /// The closure
        /// </summary>
        Closure,
    }

    /// Describes a variable.
    /// </summary>
    public class SlotInfo
    {
        /// The scope information
        /// </summary>
        public readonly ScopeInfo ScopeInfo;
        /// The type
        /// </summary>
        public readonly SlotType Type;
        /// The name
        /// </summary>
        public readonly string Name;
        /// The index
        /// </summary>
        public readonly int Index;
        /// The is public
        /// </summary>
        public bool IsPublic = true; //for module-level slots, indicator that symbol is "exported" and visible by code that imports the module
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

    /// Class SlotInfoDictionary.
    /// </summary>
    [Serializable]
    public class SlotInfoDictionary : Dictionary<string, SlotInfo>
    {
        /// Initializes a new instance of the <see cref="SlotInfoDictionary"/> class.
        /// </summary>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
        public SlotInfoDictionary(bool caseSensitive)
            : base(32, caseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase) { }
    }

}
