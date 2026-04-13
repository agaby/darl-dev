/// <summary>
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ScopeInfo.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Interpreter.Ast;
using System;
using System.Collections.Generic;

/// <summary>
/// The Interpreter namespace.
/// </summary>
namespace DarlCompiler.Interpreter
{

    /// <summary>
    /// Class ScopeInfoList.
    /// </summary>
    public class ScopeInfoList : List<ScopeInfo> { }

    /// <summary>
    /// Describes all variables (locals and parameters) defined in a scope of a function or module.
    /// </summary>
    /// <remarks>ScopeInfo is metadata, it does not contain variable values. The Scope object (described by ScopeInfo) is a container for values.</remarks>
    // Note that all access to SlotTable is done through "lock" operator, so it's thread safe
    public class ScopeInfo
    {
        /// <summary>
        /// The values count
        /// </summary>
        public int ValuesCount, ParametersCount;
        /// <summary>
        /// The owner node
        /// </summary>
        public AstNode OwnerNode; //might be null
        // Static/singleton scopes only; for ex,  modules are singletons. Index in App.StaticScopes array  
        /// <summary>
        /// The static index
        /// </summary>
        public int StaticIndex = -1;
        /// <summary>
        /// The level
        /// </summary>
        public int Level;
        /// <summary>
        /// As string
        /// </summary>
        public readonly string AsString;
        /// <summary>
        /// The scope instance
        /// </summary>
        public Scope ScopeInstance; //Experiment: reusable scope instance; see ScriptThread.cs class

        /// <summary>
        /// The _slots
        /// </summary>
        private readonly SlotInfoDictionary _slots;
        /// <summary>
        /// The lock object
        /// </summary>
        protected internal object LockObject = new object();

        /// <summary>
        /// Initializes a new instance of the <see cref="ScopeInfo"/> class.
        /// </summary>
        /// <param name="ownerNode">The owner node.</param>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
        /// <exception cref="System.Exception">ScopeInfo owner node may not be null.</exception>
        public ScopeInfo(AstNode ownerNode, bool caseSensitive)
        {
            if (ownerNode == null)
                throw new Exception("ScopeInfo owner node may not be null.");
            OwnerNode = ownerNode;
            _slots = new SlotInfoDictionary(caseSensitive);
            Level = Parent == null ? 0 : Parent.Level + 1;
            var sLevel = "level=" + Level;
            AsString = OwnerNode == null ? sLevel : OwnerNode.AsString + ", " + sLevel;

        }

        //Lexical parent
        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <value>The parent.</value>
        public ScopeInfo Parent
        {
            get
            {
                if (_parent == null)
                    _parent = GetParent();
                return _parent;
            }
            /// <summary>
            /// The _parent
            /// </summary>
        }
        ScopeInfo _parent;

        /// <summary>
        /// Gets the parent.
        /// </summary>
        /// <returns>ScopeInfo.</returns>
        public ScopeInfo GetParent()
        {
            if (OwnerNode == null) return null;
            var currentParent = OwnerNode.Parent;
            while (currentParent != null)
            {
                var result = currentParent.DependentScopeInfo;
                if (result != null) return result;
                currentParent = currentParent.Parent;
            }
            return null; //should never happen
        }

        #region Slot operations
        /// <summary>
        /// Adds the slot.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="type">The type.</param>
        /// <returns>SlotInfo.</returns>
        public SlotInfo AddSlot(string name, SlotType type)
        {
            lock (LockObject)
            {
                var index = type == SlotType.Value ? ValuesCount++ : ParametersCount++;
                var slot = new SlotInfo(this, type, name, index);
                _slots.Add(name, slot);
                return slot;
            }
        }

        //Returns null if slot not found.
        /// <summary>
        /// Gets the slot.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <returns>SlotInfo.</returns>
        public SlotInfo GetSlot(string name)
        {
            lock (LockObject)
            {
                SlotInfo slot;
                _slots.TryGetValue(name, out slot);
                return slot;
            }
        }

        /// <summary>
        /// Gets the slots.
        /// </summary>
        /// <returns>IList&lt;SlotInfo&gt;.</returns>
        public IList<SlotInfo> GetSlots()
        {
            lock (LockObject)
            {
                return new List<SlotInfo>(_slots.Values);
            }
        }

        /// <summary>
        /// Gets the names.
        /// </summary>
        /// <returns>IList&lt;System.String&gt;.</returns>
        public IList<string> GetNames()
        {
            lock (LockObject)
            {
                return new List<string>(_slots.Keys);
            }
        }

        /// <summary>
        /// Gets the slot count.
        /// </summary>
        /// <returns>System.Int32.</returns>
        public int GetSlotCount()
        {
            lock (LockObject)
            {
                return _slots.Count;
            }
        }
        #endregion

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return AsString;
        }
    }
}
