using DarlCompiler.Interpreter;
using System;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class RandomTextNode : MultipleDarlMetaNode
    {
        protected override async Task<object> DoEvaluate(ScriptThread thread)
        {
            Prologue(thread);
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
                DarlResult res = (DarlResult)await arguments[choice].Evaluate(thread);
                if (res.dataType == DarlResult.DataType.textual)
                {
                    var res2 = new DarlResult("", res.stringConstant, DarlResult.DataType.textual);
                    Epilogue(thread, res2);
                    return res2;
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