using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarlCompiler.Interpreter;
using DarlCompiler.Parsing;

namespace DarlLanguage.Processing
{
    public class RandomTextNode : MultipleDarlNode
    {


        protected override async Task<object> DoEvaluate(ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            if (arguments.Count == 0)
            {
                thread.CurrentNode = Parent;
                return new DarlResult("", "", DarlResult.DataType.textual);
            }
            else
            {
                var choice = 0;
                if (arguments.Count > 1)
                {
                    var r = new Random();
                    choice = r.Next(arguments.Count);
                }
                string text = string.Empty;
                DarlResult res = (DarlResult) await arguments[choice].Evaluate(thread);
                if (res.dataType == DarlResult.DataType.textual)
                {
                    thread.CurrentNode = Parent;
                    return new DarlResult("", res.stringConstant, DarlResult.DataType.textual);
                }
                else
                {
                    throw new ScriptException($"Non-textual parameter passed to randomtext in position {choice}. ");
                }
            }           
        }

        public override string preamble
        {
            get
            {
                return "randomtext( ";
            }
        }
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
                return ") ";
            }
        }
    }
}
