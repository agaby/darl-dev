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
// <copyright file="IBindingSource.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Collections.Generic;

namespace DarlCompiler.Interpreter
{

    /// <summary>
    /// Interface IBindingSource
    /// </summary>
    public interface IBindingSource
    {
        /// <summary>
        /// Binds the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Binding.</returns>
        Binding Bind(BindingRequest request);
    }

    /// <summary>
    /// Class BindingSourceList.
    /// </summary>
    public class BindingSourceList : List<IBindingSource>
    {
    }

    /// <summary>
    /// Class BindingSourceTable.
    /// </summary>
    [Serializable]
    public class BindingSourceTable : Dictionary<string, IBindingSource>, IBindingSource
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BindingSourceTable"/> class.
        /// </summary>
        /// <param name="caseSensitive">if set to <c>true</c> [case sensitive].</param>
        public BindingSourceTable(bool caseSensitive)
            : base(caseSensitive ? StringComparer.InvariantCulture : StringComparer.InvariantCultureIgnoreCase)
        {
        }
        //IBindingSource Members
        /// <summary>
        /// Binds the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Binding.</returns>
        public Binding Bind(BindingRequest request)
        {
            IBindingSource target;
            if (TryGetValue(request.Symbol, out target))
                return target.Bind(request);
            return null;
        }
    }

    // This class will be used to define extensions for BindingSourceTable
    /// <summary>
    /// Class BindingSourceTableExtensions.
    /// </summary>
    public static partial class BindingSourceTableExtensions
    {
    }

}
