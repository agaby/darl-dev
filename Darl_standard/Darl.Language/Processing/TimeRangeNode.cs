using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using DarlCompiler.Interpreter;

namespace DarlLanguage.Processing
{
    public class TimeRangeNode : MultipleDarlNode
    {
        protected override async Task<object> DoEvaluate(ScriptThread thread)
        {
            if (arguments[0] == null)
            {
                return new DarlResult(0.0f, true);
            }
            DarlResult res2 = new DarlResult("", DateTime.Now, DarlResult.DataType.temporal);
            res2.values.Clear();
            foreach (DarlNode child in arguments)
            {
                if (child != null)
                {
                    DarlResult res1 = (DarlResult)await child.Evaluate(thread);
                    if (res1.IsUnknown())//if any children unknown, whole thing unknown.
                        return new DarlResult(0.0f, true);
                    if (res1.dataType != DarlResult.DataType.temporal)
                        throw new RuleException($"TimeRange: {child.GetName()} not a temporal type.");
                    //if the constituents of a fuzzytuple are themselves fuzzy we're into
                    //type 2 fuzzy sets, but since we don't support those yet we'll summarize them
                    //with CofG
                    res2.values.Add(res1.CofG());
                }
                else
                    break; // quit on first null
            }
            res2.values.Sort();
            res2.Normalise(false);
            return res2;
        }


        /// <summary>
        /// Gets the preamble.
        /// </summary>
        /// <value>
        /// The preamble, used to reconstruct the source code.
        /// </value>
        public override string preamble
        {
            get
            {
                return "timerange( ";
            }
        }
        /// <summary>
        /// Gets the midamble.
        /// </summary>
        /// <value>
        /// The midamble, used to reconstruct the source code.
        /// </value>
        public override string midamble
        {
            get
            {
                return ", ";
            }
        }

        /// <summary>
        /// Gets the postamble.
        /// </summary>
        /// <value>
        /// The postamble, used to reconstruct the source code.
        /// </value>
        public override string postamble
        {
            get
            {
                return ") ";
            }
        }
    }
}
