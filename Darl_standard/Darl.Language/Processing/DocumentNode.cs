using DarlCompiler.Ast;
using DarlCompiler.Interpreter;
using DarlCompiler.Parsing;
using Datl.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    public class DocumentNode : BinaryDarlNode
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
            thread.CurrentNode = this;  //standard prologue
            DarlResult res = (DarlResult) await Left.Evaluate(thread);
            var pars = await Right.Evaluate(thread) as Dictionary<string,string>;
            string s = String.Empty;
            if (res.dataType == DarlResult.DataType.textual || res.IsUnknown())
            {
                var doc = res.stringConstant;
                var t = new TextProcess();
                s = t.Parse(doc, pars) as string;              
            }
            else
            {
                throw new ScriptException($"{Left.Term.Name} is not a textual input");
            }
            thread.CurrentNode = Parent;
            return new DarlResult("", s, DarlResult.DataType.textual);
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
                return ", ";
            }
        }

        public override string postamble
        {
            get
            {
                return " )";
            }
        }

        public class DocState
        {
            public string name { get; set; }
            public bool active { get; set; }
        }

        public override void WalkSaliences(double saliency, MapRootNode root, string currentRuleSet, string currentOutput)
        {
            //base.WalkSaliences(saliency, root, currentRuleSet, currentOutput);
        }
    }
}
