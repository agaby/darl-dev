/// <summary>
/// </summary>

﻿using DarlCompiler.Interpreter;
using System;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class TimeRangeNode : MultipleDarlMetaNode
    {
        protected override async Task<object> DoEvaluate(ScriptThread thread)
        {
            Prologue(thread);
            if (arguments[0] == null)
            {
                var res =  new DarlResult(0.0f, true);
                Epilogue(thread,res);
            }
            DarlResult res2 = new DarlResult("", DateTime.Now, DarlResult.DataType.temporal);
            res2.values.Clear();
            foreach (DarlMetaNode child in arguments)
            {
                if (child != null)
                {
                    DarlResult res1 = (DarlResult)await child.Evaluate(thread);
                    if (res1.IsUnknown())//if any children unknown, whole thing unknown.
                    {
                        var res = new DarlResult(0.0f, true);
                        Epilogue(thread,res);
                        return res;
                    }
                    if (res1.dataType != DarlResult.DataType.temporal)
                        throw new MetaRuleException($"TimeRange: {child.GetName()} not a temporal type.");
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
            Epilogue(thread,res2);
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