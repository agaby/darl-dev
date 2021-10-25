// ***********************************************************************
// Assembly         : DarlCompiler.Interpreter
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-25-2015
// ***********************************************************************
// <copyright file="CommandLine.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System;
using System.Threading;
using DarlCompiler.Parsing;
using System.Threading.Tasks;
using Darl_standard;

namespace DarlCompiler.Interpreter
{

    //An abstraction of a Console. 
    /// <summary>
    /// Interface IConsoleAdaptor
    /// </summary>
    public interface IConsoleAdaptor
    {
        /// <summary>
        /// Gets or sets a value indicating whether this <see cref="IConsoleAdaptor"/> is canceled.
        /// </summary>
        /// <value><c>true</c> if canceled; otherwise, <c>false</c>.</value>
        bool Canceled { get; set; }
        /// <summary>
        /// Writes the specified text.
        /// </summary>
        /// <param name="text">The text.</param>
        void Write(string text);
        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="text">The text.</param>
        void WriteLine(string text);
        /// <summary>
        /// Sets the text style.
        /// </summary>
        /// <param name="style">The style.</param>
        void SetTextStyle(ConsoleTextStyle style);
        /// <summary>
        /// Reads this instance.
        /// </summary>
        /// <returns>System.Int32.</returns>
        int Read(); //reads a key
        /// <summary>
        /// Reads the line.
        /// </summary>
        /// <returns>System.String.</returns>
        string ReadLine(); //reads a line; returns null if Ctrl-C is pressed
        /// <summary>
        /// Sets the title.
        /// </summary>
        /// <param name="title">The title.</param>
        void SetTitle(string title);
    }

    //WARNING: Ctrl-C for aborting running script does NOT work when you run console app from Visual Studio 2010. 
    // Run executable directly from bin folder. 
    /// <summary>
    /// Class CommandLine.
    /// </summary>
    public class CommandLine
    {
        #region Fields and properties
        /// <summary>
        /// The runtime
        /// </summary>
        public readonly LanguageRuntime Runtime;
        /// <summary>
        /// The _console
        /// </summary>
        public readonly IConsoleAdaptor _console;
        //Initialized from grammar
        /// <summary>
        /// The title
        /// </summary>
        public string Title;
        /// <summary>
        /// The greeting
        /// </summary>
        public string Greeting;
        /// <summary>
        /// The prompt
        /// </summary>
        public string Prompt; //default prompt
        /// <summary>
        /// The prompt more input
        /// </summary>
        public string PromptMoreInput; //prompt to show when more input is expected

        /// <summary>
        /// The application
        /// </summary>
        public readonly ScriptApp App;
        /// <summary>
        /// The _worker thread
        /// </summary>
        Thread _workerThread;
        /// <summary>
        /// Gets a value indicating whether this instance is evaluating.
        /// </summary>
        /// <value><c>true</c> if this instance is evaluating; otherwise, <c>false</c>.</value>
        public bool IsEvaluating { get; private set; }

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="CommandLine"/> class.
        /// </summary>
        /// <param name="runtime">The runtime.</param>
        /// <param name="console">The console.</param>
        public CommandLine(LanguageRuntime runtime, IConsoleAdaptor console = null)
        {
            Runtime = runtime;
            _console = console ?? new ConsoleAdapter();
            var grammar = runtime.Language.Grammar;
            Title = grammar.ConsoleTitle;
            Greeting = grammar.ConsoleGreeting;
            Prompt = grammar.ConsolePrompt;
            PromptMoreInput = grammar.ConsolePromptMoreInput;
            App = new ScriptApp(Runtime);
            App.ParserMode = ParseMode.CommandLine;
            // App.PrintParseErrors = false;
            App.RethrowExceptions = false;

        }

        /// <summary>
        /// Runs this instance.
        /// </summary>
        public void Run()
        {
            try
            {
                RunImpl();
            }
            catch (Exception ex)
            {
                _console.SetTextStyle(ConsoleTextStyle.Error);
                _console.WriteLine(Resources.ErrConsoleFatalError);
                _console.WriteLine(ex.ToString());
                _console.SetTextStyle(ConsoleTextStyle.Normal);
                _console.WriteLine(Resources.MsgPressAnyKeyToExit);
                _console.Read();
            }
        }


 
        /// <summary>
        /// Waits for script complete.
        /// </summary>
        private void WaitForScriptComplete()
        {
            _console.Canceled = false;
            while (true)
            {
                Thread.Sleep(50);
                if (!IsEvaluating) return;
                if (_console.Canceled)
                {
                    _console.Canceled = false;
                    if (Confirm(Resources.MsgAbortScriptYN))
                        WorkerThreadAbort();
                }//if Canceled
            }
        }

        /// <summary>
        /// Evaluates the specified script.
        /// </summary>
        /// <param name="script">The script.</param>
        private async Task Evaluate(string script)
        {
            try
            {
                IsEvaluating = true;
                await App.Evaluate(script);
            }
            finally
            {
                IsEvaluating = false;
            }
        }

        /// <summary>
        /// Workers the thread start.
        /// </summary>
        /// <param name="data">The data.</param>
        private async Task WorkerThreadStart(object data)
        {
            try
            {
                var script = data as string;
                await App.Evaluate(script);
            }
            finally
            {
                IsEvaluating = false;
            }
        }
        /// <summary>
        /// Workers the thread abort.
        /// </summary>
        private void WorkerThreadAbort()
        {
            try
            {
                _workerThread.Abort();
                _workerThread.Join(50);
            }
            finally
            {
                IsEvaluating = false;
            }
        }

        /// <summary>
        /// Confirms the specified message.
        /// </summary>
        /// <param name="message">The message.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        private bool Confirm(string message)
        {
            _console.WriteLine(string.Empty);
            _console.Write(message);
            var input = _console.ReadLine();
            return Resources.ConsoleYesChars.Contains(input);
        }

        /// <summary>
        /// Reports the exception.
        /// </summary>
        private void ReportException()
        {
            _console.SetTextStyle(ConsoleTextStyle.Error);
            var ex = App.LastException;
            var scriptEx = ex as ScriptException;
            if (scriptEx != null)
                _console.WriteLine(scriptEx.Message + " " + Resources.LabelLocation + " " + scriptEx.Location.ToUiString());
            else
            {
                if (App.Status == AppStatus.Crash)
                    _console.WriteLine(ex.ToString());   //Unexpected interpreter crash:  the full stack when debugging your language  
                else
                    _console.WriteLine(ex.Message);

            }
            //
        }

    }
}
