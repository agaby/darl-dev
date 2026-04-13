/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ModuleExport.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************

namespace DarlCompiler.Interpreter
{

    // Module export, container for public, exported symbols from module
    // Just a skeleton, to be completed
    /// Class ModuleExport.
    /// </summary>
    public class ModuleExport : IBindingSource
    {
        /// The module
        /// </summary>
        public ModuleInfo Module;
        /// Initializes a new instance of the <see cref="ModuleExport"/> class.
        /// </summary>
        /// <param name="module">The module.</param>
        public ModuleExport(ModuleInfo module)
        {
            Module = module;
        }

        /// Binds the specified request.
        /// </summary>
        /// <param name="request">The request.</param>
        /// <returns>Binding.</returns>
        public Binding Bind(BindingRequest request)
        {
            return null;
        }
    }



}
