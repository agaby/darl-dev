/// <summary>
/// StartsNode.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCompiler.Interpreter;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    public class StartsNode : BinaryDarlNode
    {
        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            return Task.FromResult<object>(base.DoEvaluate(thread));
        }

        public override string midamble => "starts ";
    }
}