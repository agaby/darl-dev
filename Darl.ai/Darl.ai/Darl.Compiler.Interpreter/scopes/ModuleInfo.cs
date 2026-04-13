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
// <copyright file="ModuleInfo.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;

namespace DarlCompiler.Interpreter
{

    /// <summary>
    /// Class ModuleInfoList.
    /// </summary>
    public class ModuleInfoList : List<ModuleInfo> { }

    /// <summary>
    /// Class ModuleInfo.
    /// </summary>
    public class ModuleInfo
    {
        /// <summary>
        /// The name
        /// </summary>
        public readonly string Name;
        /// <summary>
        /// The file name
        /// </summary>
        public readonly string FileName;
        /// <summary>
        /// The scope information
        /// </summary>
        public readonly ScopeInfo ScopeInfo; //scope for module variables
        /// <summary>
        /// The imports
        /// </summary>
        public readonly BindingSourceList Imports = new BindingSourceList();

        /// <summary>
        /// Initializes a new instance of the <see cref="ModuleInfo"/> class.
        /// </summary>
        /// <param name="name">The name.</param>
        /// <param name="fileName">Name of the file.</param>
        /// <param name="scopeInfo">The scope information.</param>
        public ModuleInfo(string name, string fileName, ScopeInfo scopeInfo)
        {
            Name = name;
            FileName = fileName;
            ScopeInfo = scopeInfo;
        }

        //Used for imported modules
        /// <summary>
        /// Binds to export.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Binding.</returns>
        public Binding BindToExport(BindingRequest request)
        {
            return null;
        }

    }
}
