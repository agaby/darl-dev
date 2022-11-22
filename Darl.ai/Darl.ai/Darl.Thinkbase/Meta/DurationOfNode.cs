using Darl.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.Thinkbase.Meta
{
    public class DurationOfNode : UnaryDarlMetaNode
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
                    return new DarlResult(0, true);
                //return the truth of the statement: now and the objects existence overlap in time.
                var res2 = CalculateDuration(grammar!.currentNode.existence);
                Epilogue(thread,res2);
                return res2;
            }
            else //existence of an attribute or external node
            {
                if (grammar!.currentNode!.properties == null)
                {
                    var res2 = new DarlResult(0, true);
                    Epilogue(thread, res2);
                    return res2;
                }
                var att = grammar.currentModel.FindAttributeExistence(grammar!.currentNode.id!, res.Value.ToString()!, grammar.state);
                if (att == null)
                {
                    if (grammar.currentModel.vertices.ContainsKey(res.Value.ToString()!))
                    {
                        var res4 =  CalculateDuration(grammar!.currentModel.vertices[res.Value.ToString()].existence);
                        Epilogue(thread, res4);
                        return res4;
                    }
                    else
                    {
                        var nodebyExtId = grammar.currentModel.vertices.Values.FirstOrDefault(a => a.externalId == res.Value.ToString());
                        if (nodebyExtId != null)
                        {
                            var res4 =  CalculateDuration(nodebyExtId.existence);
                            Epilogue(thread, res4);
                            return res4;
                        }
                    }
                    var res2 = new DarlResult(0, true);
                    Epilogue(thread, res2);
                    return res2;
                }
                thread.CurrentNode = Parent;
                var res3 = CalculateDuration(att);
                Epilogue(thread, res3);
                return res3;
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
                return "durationof( ";
            }
        }
        public override string postamble
        {
            get
            {
                return ")";
            }
        }

        /// <summary>
        /// Handles fuzzy dateTimes up to degree 4
        /// </summary>
        /// <param name="existence"></param>
        /// <returns></returns>
        private DarlResult CalculateDuration(List<DarlTime?> existence)
        {
            var duration = new DarlResult("duration", DarlResult.DataType.duration);
            switch (existence.Count)
            {
                default:
                case 0:
                case 1:
                    duration.values.Add(0);
                    break;
                case 2:
                    duration.values.Add((existence[1] ?? DarlTime.MinValue).raw - (existence[0] ?? DarlTime.MinValue).raw);
                    break;
                case 3:
                    duration.values.Add((existence[1] ?? DarlTime.MinValue).raw - (existence[0] ?? DarlTime.MinValue).raw);
                    duration.values.Add((existence[2] ?? DarlTime.MinValue).raw - (existence[0] ?? DarlTime.MinValue).raw);
                    break;
                case 4:
                    duration.values.Add((existence[2] ?? DarlTime.MinValue).raw - (existence[1] ?? DarlTime.MinValue).raw);
                    duration.values.Add((existence[3] ?? DarlTime.MinValue).raw - (existence[0] ?? DarlTime.MinValue).raw);
                    break;
            }
            duration.Normalise(false);
            return duration;

        }
    }
}
