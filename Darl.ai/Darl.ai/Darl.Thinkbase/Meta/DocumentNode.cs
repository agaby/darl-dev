/// <summary>
/// DocumentNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Interpreter;
using Datl.Language;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class DocumentNode : BinaryDarlMetaNode
    {
        public static string insertEscapeString { get; set; } = "%%";
        /// <summary>
        /// Does the evaluation.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>
        /// The result of the evaluation
        /// </returns>
        protected override async Task<object> DoEvaluate(ScriptThread thread)
        {
            Prologue(thread);
            DarlResult res = (DarlResult)await Left.Evaluate(thread);
            var pars = await Right.Evaluate(thread) as Dictionary<string, string>;
            string s = String.Empty;
            if (res.dataType == DarlResult.DataType.textual || res.IsUnknown())
            {
                var doc = res.stringConstant;
                var t = new TextProcess();
                s = (t.Parse(doc!, pars!) as string)!;
                s = s.Trim();
            }
            else
            {
                throw new ScriptException($"{Left.Term.Name} is not a textual input");
            }
            var res2 = new DarlResult("", s, DarlResult.DataType.textual);
            Epilogue(thread, res2);
            return res2;
        }

        public override string preamble
        {
            get
            {
                return "document( ";
            }
        }

        /// <summary>
        /// Gets the midamble.
        /// </summary>
        /// <value>
        /// The midamble, used to reconstruct the source code.
        /// </value>
        public override string midamble
        {
            get
            {
                return ", {";
            }
        }

        public override string postamble
        {
            get
            {
                return " })";
            }
        }

        public class DocState
        {
            public string name { get; set; }
            public bool active { get; set; }
        }
    }
}