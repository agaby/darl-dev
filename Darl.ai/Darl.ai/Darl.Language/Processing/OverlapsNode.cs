using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DarlCompiler.Interpreter;

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
