using Darl.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    /// <summary>
    /// Extracts a time value representing the existence of the referenced node or attribute.
    /// </summary>
    public  class ExistenceNode : UnaryDarlMetaNode
    {

        /// <summary>
        /// Does the evaluation.
        /// </summary>
        /// <param name="thread">The thread.</param>
        /// <returns>
        /// The result of the evaluation
        /// </returns>
        protected override async Task<object> DoEvaluate(DarlCompiler.Interpreter.ScriptThread thread)
        {
            thread.CurrentNode = this;  //standard prologue
            DarlResult res = Argument != null ? (DarlResult)await Argument.Evaluate(thread) : null;
            var grammar = thread.Runtime.Language.Grammar as DarlMetaGrammar;
            if (grammar.currentNode == null)
            {
                thread.CurrentNode = Parent;
                return new DarlResult(0, true);
            }
            if (res is null) //operate on object existence
            {
                if (grammar.currentNode.existence == null || !grammar.currentNode.existence.Any())
                    return new DarlResult(0, true);
                //return the truth of the statement: now and the objects existence overlap in time.
                thread.CurrentNode = Parent;
                return ConvertTime(grammar.currentNode.existence); //convert to DarlResult
            }
            else //existence of an attribute or external node
            {
                if (grammar.currentNode.properties == null)
                    return new DarlResult(0, true);
                var att = grammar.currentModel.FindAttributeExistence(grammar.currentNode.id, res.Value.ToString(), grammar.state);
                if (att == null)
                {
                    if (grammar.currentModel.vertices.ContainsKey(res.Value.ToString()))
                    {
                        return ConvertTime(grammar.currentModel.vertices[res.Value.ToString()].existence);
                    }
                    else
                    {
                        var nodebyExtId = grammar.currentModel.vertices.Values.FirstOrDefault(a => a.externalId == res.Value.ToString());
                        if (nodebyExtId != null)
                        {
                            if(nodebyExtId.existence != null)
                                return ConvertTime(nodebyExtId.existence);
                            if(grammar.state.ContainsRecord(nodebyExtId.id ?? ""))
                            {
                                var exAtt = grammar.state.GetAttribute(nodebyExtId.id ?? "", "noun:01,5,03,3,018"); //life in common lineages replace.
                                if(exAtt != null && exAtt.existence != null)
                                {
                                    return ConvertTime(exAtt.existence);
                                }

                            }
                        }
                    }
                    thread.CurrentNode = Parent;
                    return new DarlResult(0, true);
                }
                thread.CurrentNode = Parent;
                return att;
            }
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
                return "existence( ";
            }
        }
        public override string postamble
        {
            get
            {
                return ")";
            }
        }

        private DarlResult ConvertTime(List<DarlTime> darlTimes)
        {
            var duration = new DarlResult("existence", DarlResult.DataType.temporal,1.0);
            foreach(var i in darlTimes)
            {
                duration.values.Add((i ?? DarlTime.MinValue).raw);
            }            
            duration.Normalise(false);
            return duration;
        }
    }
}
