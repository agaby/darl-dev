/// </summary>

﻿using DarlCompiler.Interpreter;
using System;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    public class MaxTimeNode : DarlNode
    {
        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            return Task.FromResult<object>(new DarlResult("", DateTime.MaxValue, DarlResult.DataType.temporal));
        }
    }
}
