/// <summary>
/// SingleNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Linq;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    //returns just the first matching attribute found
    public class SingleNode : AttributesNode
    {
        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            Prologue(thread);
            var res = (await base.DoEvaluate(thread) as DarlResult)!;
            if (res.IsUnknown())
            {
                Epilogue(thread, res);
                return res;
            }
            if (!res.values.Any())
            {
                var res2 =  new DarlResult(0.0, true);
                Epilogue(thread, res2);
                return res2;
            }
            var res3 = (res.values.First() as DarlResult)!;
            Epilogue(thread, res3);
            return res3;
        }

        public override string preamble
        {
            get
            {
                return "single(";
            }
        }
    }
}
