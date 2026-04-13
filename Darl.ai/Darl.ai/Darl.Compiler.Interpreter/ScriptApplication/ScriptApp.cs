/// <summary>
/// ScriptApp.cs - Core module for the Darl.dev project.
/// </summary>

﻿// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="ScriptApp.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using Darl.Thinkbase.Meta;
using DarlCompiler.Interpreter.Ast;
using DarlCompiler.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace DarlCompiler.Interpreter
{

    /// <summary>
    /// Enum AppStatus
    /// </summary>
    public enum AppStatus
    {
        /// <summary>
        /// The ready
        /// </summary>
        Ready,
        /// <summary>
        /// The evaluating
        /// </summary>
        Evaluating,
        /// <summary>
        /// The waiting more input
        /// </summary>
        WaitingMoreInput, //command line only
        /// <summary>
        /// The syntax error
        /// </summary>
        SyntaxError,
        /// <summary>
        /// The runtime error
        /// </summary>
        RuntimeError,
        /// <summary>
        /// The crash
        /// </summary>
        Crash, //interpreter crash
        /// <summary>
        /// The aborted
        /// </summary>
        Aborted
    }

    /// <summary>
    /// Represents a running instance of a script application.
    /// </summary>
    public sealed class ScriptApp
    {
        /// <summary>
        /// The language
        /// </summary>
        public readonly LanguageData Language;
        /// <summary>
        /// The runtime
        /// </summary>
        public readonly LanguageRuntime Runtime;
        /// <summary>
        /// Gets the parser.
        /// </summary>
        /// <value>The parser.</value>
        public Parser Parser { get; private set; }

        /// <summary>
        /// The data map
        /// </summary>
        public AppDataMap DataMap;

        /// <summary>
        /// The static scopes
        /// </summary>
        public Scope[] StaticScopes;
        /// <summary>
        /// The main scope
        /// </summary>
        public Scope MainScope;
        /// <summary>
        /// Gets the globals.
        /// </summary>
        /// <value>The globals.</value>
        public IDictionary<string, object> Globals { get; private set; }
        /// <summary>
        /// The imported assemblies
        /// </summary>
        private readonly IList<Assembly> ImportedAssemblies = new List<Assembly>();

        /// <summary>
        /// The output buffer
        /// </summary>
        public StringBuilder OutputBuffer = new StringBuilder();
        /// <summary>
        /// The _lock object
        /// </summary>
        private readonly object _lockObject = new object();

        // Current mode/status variables
        /// <summary>
        /// The status
        /// </summary>
        public AppStatus Status;
        /// <summary>
        /// The evaluation time
        /// </summary>
        public long EvaluationTime;
        /// <summary>
        /// The last exception
        /// </summary>
        public Exception LastException;
        /// <summary>
        /// The rethrow exceptions
        /// </summary>
        public bool RethrowExceptions = true;

        /// <summary>
        /// Gets the last script.
        /// </summary>
        /// <value>The last script.</value>
        public ParseTree LastScript { get; private set; } //the root node of the last executed script


        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptApp"/> class.
        /// </summary>
        /// <param name="language">The language.</param>
        public ScriptApp(LanguageData language)
        {
            Language = language;
            var grammar = language.Grammar as InterpretedLanguageGrammar;
            Runtime = grammar.CreateRuntime(language);
            DataMap = new AppDataMap(Language.Grammar.CaseSensitive);
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptApp"/> class.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        public ScriptApp(LanguageRuntime runtime)
        {
            Runtime = runtime;
            Language = Runtime.Language;
            DataMap = new AppDataMap(Language.Grammar.CaseSensitive);
            Init();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ScriptApp"/> class.
        /// </summary>
        /// <param name="dataMap">The data map.</param>
        public ScriptApp(AppDataMap dataMap)
        {
            DataMap = dataMap;
            Init();
        }

        /// <summary>
        /// Initializes this instance.
        /// </summary>
        [SecuritySafeCritical]
        private void Init()
        {
            Parser = new Parser(Language);
            //Create static scopes
            MainScope = new Scope(DataMap.MainModule.ScopeInfo, null, null, null);
            StaticScopes = new Scope[DataMap.StaticScopeInfos.Count];
            StaticScopes[0] = MainScope;
            Globals = MainScope.AsDictionary();
        }

        #endregion

        /// <summary>
        /// Gets the parser messages.
        /// </summary>
        /// <returns>LogMessageList.</returns>
        public LogMessageList GetParserMessages()
        {
            return Parser.Context.CurrentParseTree.ParserMessages;
        }
        // Utilities
        /// <summary>
        /// Gets the import assemblies.
        /// </summary>
        /// <returns>IEnumerable&lt;Assembly&gt;.</returns>
        public IEnumerable<Assembly> GetImportAssemblies()
        {
            //simple default case - return all assemblies loaded in domain
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        /// <summary>
        /// Gets or sets the parser mode.
        /// </summary>
        /// <value>The parser mode.</value>
        public ParseMode ParserMode
        {
            get { return Parser.Context.Mode; }
            set { Parser.Context.Mode = value; }
        }

        #region Evaluation

        // Darl interpreter requires that once a script is executed in a ScriptApp, it is bound to AppDataMap object, 
        // and all later script executions should be performed only in the context of the same app (or at least by an App with the same DataMap).
        // The reason is because the first execution sets up a data-binding fields, like slots, scopes, etc, which are bound to ScopeInfo objects, 
        // which in turn is part of DataMap.
        /// <summary>
        /// Evaluates the specified parsed script.
        /// </summary>
        /// <param name="parsedScript">The parsed script.</param>
        /// <returns>System.Object.</returns>
        public async Task<DarlMetaActivity?> Evaluate(ParseTree parsedScript)
        {
            Util.Check(parsedScript.Root.AstNode != null, "Root AST node is null, cannot evaluate script. Create AST tree first.");
            var root = parsedScript.Root.AstNode as AstNode;
            Util.Check(root != null,
              "Root AST node {0} is not a subclass of DarlCompiler.Interpreter.AstNode. ScriptApp cannot evaluate this script.", root.GetType());
            Util.Check(root.Parent == null || root.Parent == DataMap.ProgramRoot,
              "Cannot evaluate parsed script. It had been already evaluated in a different application.");
            LastScript = parsedScript;
            return await EvaluateParsedScript();
        }


        //Actual implementation
        /// <summary>
        /// Evaluates the parsed script.
        /// </summary>
        /// <returns>System.Object.</returns>
        private async Task<DarlMetaActivity?> EvaluateParsedScript()
        {
            LastScript.Tag = DataMap;
            var root = LastScript.Root.AstNode as AstNode;
            root.DependentScopeInfo = MainScope.Info;

            Status = AppStatus.Evaluating;
            ScriptThread thread = null;
            try
            {
                thread = new ScriptThread(this);
                await root.Evaluate(thread);
                Status = AppStatus.Ready;
                var dma = new DarlMetaActivity();
                while(thread.ExecutionStack.Any())
                {
                    var top = thread.ExecutionStack.Pop();
                    dma.activeNodes.Add(new DarlMetaActiveNode { name = top.Item1.GetType().ToString(), weight = top.Item2, location = top.Item1.Span });
                }
                return dma;
            }
            catch (ScriptException se)
            {
                Status = AppStatus.RuntimeError;
                se.Location = thread!.CurrentNode.Location;
                se.ScriptStackTrace = thread.GetStackTrace();
                LastException = se;
                if (RethrowExceptions)
                    throw;
                return null;
            }
            catch (Exception ex)
            {
                Status = AppStatus.RuntimeError;
                var se = new ScriptException(ex.Message, ex, thread.CurrentNode.Location, thread.GetStackTrace());
                LastException = se;
                if (RethrowExceptions)
                    throw se;
                return null;

            }//catch

        }
        #endregion


        #region Output writing
        #region ConsoleWrite event
        /// <summary>
        /// Occurs when [console write].
        /// </summary>
        public event EventHandler<ConsoleWriteEventArgs> ConsoleWrite;
        /// <summary>
        /// Called when [console write].
        /// </summary>
        /// <param name="text">The text.</param>
        private void OnConsoleWrite(string text)
        {
            if (ConsoleWrite != null)
            {
                ConsoleWriteEventArgs args = new ConsoleWriteEventArgs(text);
                ConsoleWrite(this, args);
            }
        }
        #endregion



        /// <summary>
        /// Writes the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        public void Write(string text)
        {
            lock (_lockObject)
            {
                OnConsoleWrite(text);
                OutputBuffer.Append(text);
            }
        }
        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="text">The text.</param>
        public void WriteLine(string text)
        {
            lock (_lockObject)
            {
                OnConsoleWrite(text + Environment.NewLine);
                OutputBuffer.AppendLine(text);
            }
        }

        /// <summary>
        /// Clears the output buffer.
        /// </summary>
        public void ClearOutputBuffer()
        {
            lock (_lockObject)
            {
                OutputBuffer.Clear();
            }
        }

        /// <summary>
        /// Gets the output.
        /// </summary>
        /// <returns>System.String.</returns>
        public string GetOutput()
        {
            lock (_lockObject)
            {
                return OutputBuffer.ToString();
            }
        }
        #endregion



    }
}
