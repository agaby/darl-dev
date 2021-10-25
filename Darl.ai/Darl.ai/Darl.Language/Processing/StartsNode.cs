using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DarlCompiler.Interpreter;

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