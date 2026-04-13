/// <summary>
/// </summary>

﻿using DarlCompiler.Interpreter;
using System;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class MaxTimeNode : DarlMetaNode
    {
        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            Prologue(thread);
            var res = new DarlResult("", DateTime.MaxValue, DarlResult.DataType.temporal);
            Epilogue(thread, res);
            return Task.FromResult<object>(res);
        }

        public override string preamble
        {
            get
            {
                return "maxtime";
            }
        }
     }
}