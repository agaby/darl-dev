using Darl.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    /// <summary>
    /// Extracts a time value representing the existence of the referenced node or attribute.
    /// </summary>
    public class ExistenceNode : UnaryDarlMetaNode
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
            Prologue(thread);
            DarlResult? res = Argument != null ? (DarlResult)await Argument.Evaluate(thread) : null;
            var grammar = thread.Runtime.Language.Grammar as DarlMetaGrammar;
            if (grammar!.currentNode == null)
            {
                var res2 = new DarlResult(0, true);
                Epilogue(thread, res2);
                return res2;
            }
            if (res is null) //operate on object existence
            {
                if (grammar.currentNode.existence == null || !grammar.currentNode.existence.Any())
                {
                    var res3 = new DarlResult(0, true);
                    Epilogue(thread, res3);
                    return res3;
                }
                //return the truth of the statement: now and the objects existence overlap in time.
                var res2 = ConvertTime(grammar.currentNode.existence); //convert to DarlResult
                Epilogue(thread, res2);
                return res2;
            }
            else //existence of an attribute or external node
            {
                if (grammar.currentNode.properties == null)
                {
                    var res3 = new DarlResult(0, true);
                    Epilogue(thread, res3);
                    return res3;
                }
                var att = grammar.currentModel.FindAttributeExistence(grammar!.currentNode.id!, res.Value.ToString(), grammar.state);
                if (att == null)
                {
                    if (grammar.currentModel.vertices.ContainsKey(res.Value.ToString()!))
                    {
                        var res4 = ConvertTime(grammar.currentModel.vertices[res.Value.ToString()].existence);
                        Epilogue(thread, res4);
                        return res4;
                    }
                    else
                    {
                        var nodebyExtId = grammar.currentModel.vertices.Values.FirstOrDefault(a => a.externalId == res.Value.ToString());
                        if (nodebyExtId != null)
                        {
                            if (nodebyExtId.existence != null)
                            {
                                var res4 = ConvertTime(nodebyExtId.existence);
                                Epilogue(thread, res4);
                                return res4;
                            }
                            if (grammar.state.ContainsRecord(nodebyExtId.id ?? ""))
                            {
                                var exAtt = grammar.state.GetAttribute(nodebyExtId.id ?? "", "noun:01,5,03,3,018"); //life in common lineages replace.
                                if (exAtt != null && exAtt.existence != null)
                                {
                                    var res4 = ConvertTime(exAtt.existence);
                                    Epilogue(thread, res4);
                                    return res4;
                                }
                            }
                        }
                    }
                    var res2 = new DarlResult(0, true);
                    Epilogue(thread, res2);
                    return res2;
                }
                var res5 = ConvertTime(att);
                Epilogue(thread, res5);
                return res5;
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
            var duration = new DarlResult("existence", DarlResult.DataType.temporal, 1.0);
            foreach (var i in darlTimes)
            {
                duration.values.Add((i ?? DarlTime.MinValue).raw);
            }
            duration.Normalise(false);
            return duration;
        }
    }
}
