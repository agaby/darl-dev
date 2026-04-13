// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="AppDataMap.cs" company="Dr Andy's IP LLC">
//     Copyright   2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using DarlCompiler.Interpreter.Ast;

namespace DarlCompiler.Interpreter
{

    /// Represents a set of all of static scopes/modules in the application.
    /// </summary>
    public class AppDataMap
    {
        /// The program root
        /// </summary>
        public AstNode ProgramRoot; //artificial root associated with MainModule
        /// The static scope infos
        /// </summary>
        public ScopeInfoList StaticScopeInfos = new ScopeInfoList();
        /// The modules
        /// </summary>
        public ModuleInfoList Modules = new ModuleInfoList();
        /// The main module
        /// </summary>
        public ModuleInfo MainModule;
        /// The language case sensitive
        /// </summary>
        public readonly bool LanguageCaseSensitive;

        /// Initializes a new instance of the <see cref="AppDataMap"/> class.
        /// </summary>
        /// <param name="languageCaseSensitive">if set to <c>true</c> [language case sensitive].</param>
        /// <param name="programRoot">The program root.</param>
        public AppDataMap(bool languageCaseSensitive, AstNode? programRoot = null)
        {
            LanguageCaseSensitive = languageCaseSensitive;
            ProgramRoot = programRoot ?? new AstNode();
            var mainScopeInfo = new ScopeInfo(ProgramRoot, LanguageCaseSensitive);
            StaticScopeInfos.Add(mainScopeInfo);
            mainScopeInfo.StaticIndex = 0;
            MainModule = new ModuleInfo("main", "main", mainScopeInfo);
            Modules.Add(MainModule);
        }

        /// Gets the module.
        /// </summary>
        /// <param name="moduleNode">The module node.</param>
        /// <returns>ModuleInfo.</returns>
        public ModuleInfo GetModule(AstNode moduleNode)
        {
            foreach (var m in Modules)
                if (m.ScopeInfo == moduleNode.DependentScopeInfo)
                    return m;
            return null;
        }


    }

}
