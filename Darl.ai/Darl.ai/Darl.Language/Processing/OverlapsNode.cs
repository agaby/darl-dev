/// </summary>

﻿using DarlCompiler.Interpreter;
using System.Threading.Tasks;

namespace DarlLanguage.Processing
{
    public class OverlapsNode : BinaryDarlNode
    {
        protected override Task<object> DoEvaluate(ScriptThread thread)
        {
            return base.DoEvaluate(thread);
        }

        public override string midamble => "overlaps";
    }
}
