/// <summary>
/// </summary>

﻿using DarlCompiler.Interpreter;
using System;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    public class MinTimeNode : DarlNode
    {
        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            return Task.FromResult<object>(new DarlResult("", DateTime.MinValue, DarlResult.DataType.temporal));
        }
    }
}
