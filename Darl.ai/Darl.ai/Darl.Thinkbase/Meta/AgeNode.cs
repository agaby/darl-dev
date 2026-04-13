/// <summary>
/// AgeNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Interpreter;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    /// <summary>
    /// returns the age of the temporal input relative to the current time.
    /// requires one parameter, a variable or function  returning a temporal value.
    /// </summary>
    public class AgeNode : UnaryDarlMetaNode
    {

        protected override async Task<object> DoEvaluate(ScriptThread thread)
        {
            Prologue(thread);
            DarlResult? res = Argument != null ? (DarlResult)await Argument.Evaluate(thread) : null;
            var grammar = thread.Runtime.Language.Grammar as DarlMetaGrammar;
            if (res != null && res.Exists())
            {
                var nowNode = new NowNode();
                var now = await nowNode.Evaluate(thread) as DarlResult;
                thread.CurrentNode = Parent;
                var res2 =  DarlResult.Age(res, now);
                Epilogue(thread,res2);
                return res2;
            }
            var res3 = new DarlResult(-1.0, true);
            Epilogue(thread,res3);
            return res3;
        }

        public override string preamble
        {
            get
            {
                return "age( ";
            }
        }

        public override string postamble
        {
            get
            {
                return ")";
            }
        }


    }
}
