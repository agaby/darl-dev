using Darl.Common;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase.Meta;
using DarlCommon;
using DarlCompiler.Parsing;
using DarlLanguage.Processing;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Darl.Thinkbase
{
    /// <summary>
    /// State of a graph based conversation using a bot
    /// </summary>
    public class GraphHandler : IGraphHandler
    {

        public static readonly string questionIdentifier = "__question";

        public static string defaultSignum { get; } = "default:";

        public static string responseSignum { get; } = "response";

        public static string annotationSignum { get; } = "annotation";

        private readonly IGraphProcessing _graph;
        private readonly ILogger<GraphHandler> _logger;
        private readonly IMetaStructureHandler _metaHandler;
        public IDarlMetaRunTime _runtime;

        public GraphHandler(IGraphProcessing graph, ILogger<GraphHandler> logger, IMetaStructureHandler metaHandler, IDarlMetaRunTime runtime)
        {
            _graph = graph;
            _logger = logger;
            _metaHandler = metaHandler;
            _runtime = runtime;
        }

        /// <summary>
        /// Seek pass, interrupted by data requests when unknown values are encountered.
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="graphName"></param>
        /// <param name="subjectId"></param>
        /// <param name="targetId"></param>
        /// <param name="paths"></param>
        /// <param name="completionLineage"></param>
        /// <param name="values"></param>
        /// <param name="pending"></param>
        /// <returns></returns>
        public async Task<(List<InteractTestResponse>, DarlVar?)> GraphPass(string userId, string graphName, string subjectId, string targetId, List<string> paths, string completionLineage, List<DarlVar> values, DarlVar? pending, GraphProcess graphProcess)
        {
            //validate incoming values
            string validationResponse;
            if (!Validate(pending, values, out validationResponse)) //out of range value
            {
                _logger.LogInformation($"Validation error = {validationResponse}, KGName= {graphName}, userId = {userId}");
                return (new List<InteractTestResponse> { new InteractTestResponse { darl = "", response = new DarlVar { name = "response", dataType = DarlVar.DataType.textual, Value = validationResponse }, matches = new List<MatchedElement>() } }, pending);
            }
            var responses = new List<InteractTestResponse>();
            var model = await _graph.GetModel(userId, graphName);
            var ks = await GetKnowledgeState(userId, subjectId, graphName);
            if (!(pending is null))
            {
                var currentObj = model.vertices[pending.name];
                _logger.LogInformation($"Evaluating response = {currentObj.externalId ?? currentObj.name}, KGName= {graphName}, userId = {userId}");
                var vals = await EvaluateUIRule(model, currentObj, pending, responses, ks, values, paths, true);
                if (vals.Item1)
                {
                    return (responses, vals.Item2);
                }
            }
            //Use inference to update state based on new information
            var target = await _graph.GetGraphObjectById(model.modelName, targetId);
            List<GraphAbstraction>? res = null;
            switch (graphProcess)
            {
                case GraphProcess.seek:
                    {
                        var dependencies = GetExecutionOrder(model, target, paths);
                        if (target != null)
                            dependencies.Insert(0, new KeyValuePair<GraphAbstraction, int>(target, 1));
                        await UpdateNodeStates(ks, model, dependencies, values, completionLineage);
                        //find next element to present or terminate
                        res = FindNext(model, dependencies, ks, target, paths, completionLineage);
                    }
                    break;
                case GraphProcess.discover:
                    {
                        //do a breadth-first search out from this node, stopping whenever a ruleset is encountered dependent on an unknown data item
                        var visited = new List<GraphAbstraction>();
                        res = DiscoveryProcess(model, target, paths, visited);
                    }
                    break;
            }
            if (res != null && res.Count > 0)
            {
                values.Clear();
                var vals = await EvaluateUIRule(model, res[0], pending, responses, ks, values, paths);
                pending = vals.Item2;
            }
            else
            {
                _logger.LogInformation($"Completed seek  to {targetId}, KGName= {graphName}, userId = {userId}");
                responses.Add(new InteractTestResponse { response = new DarlVar { dataType = DarlVar.DataType.complete, Value = "This process is complete.", name = "response" } });
                await EvaluateUIRule(model, target, pending, responses, ks, values, paths);
                pending = null;
            }
            await _graph.SaveKSChanges(userId, subjectId, ks);
            return (responses, pending);
        }

        private List<GraphAbstraction>? DiscoveryProcess(IGraphModel model, GraphObject target, List<string> paths, List<GraphAbstraction> visited)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Use the internal recognition tree to recognise the text - if graph processing is required 
        /// set completionLineage, target and paths 
        /// </summary>
        /// <param name="conversationData"></param>
        /// <returns></returns>
        public async Task<List<InteractTestResponse>> InterpretText(string userId, string graphName, string subjectId, DarlVar conversationData)
        {
            var tokens = LineageLibrary.SimpleTokenizer(conversationData.Value);
            DarlVar response = new DarlVar { dataType = DarlVar.DataType.textual, Value = "No response generated...", unknown = true };
            DarlVar link = new DarlVar { dataType = DarlVar.DataType.link, unknown = true, Value = "" };
            DarlVar callResponse = new DarlVar { dataType = DarlVar.DataType.ruleset, unknown = true, Value = "" };
            DarlVar graphResponse = new DarlVar { dataType = DarlVar.DataType.seek, unknown = true, Value = "" };

            var outList = new List<InteractTestResponse>();
            List<MatchedElement> list = await _graph.Match($"{userId}_{graphName}", subjectId, tokens);
            while (list.Count > 0 && (response.unknown || response.weight < 1.0))
            {
                var lastMatch = list.Last();
                if (lastMatch == null) //no response
                    break;
                var last = ((MatchedGraphAttribute)lastMatch).terminus;
                var values = lastMatch.values;
                try
                {
                    var model = await _graph.GetModel(userId, graphName);
                    if (last.type == GraphAttribute.DataType.markdown) //just return the text
                    {
                        response = new DarlVar { dataType = DarlVar.DataType.textual, Value = last.value, name = "response" };
                    }
                    else
                    {
                        var tree = _runtime.CreateTree(last.value, null, model);
                        var vals = Meta.DarlVarExtensions.Convert(values);
                        await _runtime.Evaluate(tree, vals, null);
                        values = Meta.DarlVarExtensions.Convert(vals);
                        foreach (var r in vals)
                        {
                            if (r.name == nameof(response))
                            {
                                response = Meta.DarlVarExtensions.Convert(r);
                                if (list.Last().path.Contains(defaultSignum))
                                {
                                    response.approximate = true; //signals a default:
                                }
                            }
                            else if (r.name == nameof(link) && link.unknown)
                            {
                                link = Meta.DarlVarExtensions.Convert(r);
                                link.dataType = DarlVar.DataType.link;
                                if (list.Last().path.Contains(defaultSignum))
                                {
                                    link.approximate = true; //signals a default:
                                }
                            }
                            else if (r.name.EndsWith("Call.") && callResponse.unknown) //first wins
                            {
                                callResponse = Meta.DarlVarExtensions.Convert(r);
                                callResponse.dataType = DarlVar.DataType.ruleset;
                                callResponse.name = "Call";
                                if (list.Last().path.Contains(defaultSignum))
                                {
                                    callResponse.approximate = true; //signals a default:
                                }
                            }
                            else if (r.dataType == Meta.DarlResult.DataType.seek)
                            {
                                graphResponse = Meta.DarlVarExtensions.Convert(r);
                                graphResponse.dataType = DarlVar.DataType.seek;
                                graphResponse.name = "seek";
                                if (list.Last().path.Contains(defaultSignum))
                                {
                                    graphResponse.approximate = true; //signals a default:
                                }
                            }
                            else if (r.dataType == Meta.DarlResult.DataType.discover)
                            {
                                graphResponse = Meta.DarlVarExtensions.Convert(r);
                                graphResponse.dataType = DarlVar.DataType.discover;
                                graphResponse.name = "discover";
                                if (list.Last().path.Contains(defaultSignum))
                                {
                                    graphResponse.approximate = true; //signals a default:
                                }
                            }
                            else if (r.name == "terminate")
                            {
                                response = new DarlVar { dataType = DarlVar.DataType.complete, name = r.name };
                            }
                        }
                    }
                }
                catch //probably missing IO or access denied
                {
                    continue;
                }
                response.weight = Math.Min(response.weight, lastMatch.confidence); //pass through the match confidence
                if (response.unknown && !string.IsNullOrEmpty(response.Value))
                    response.unknown = false;
                outList.Add(new InteractTestResponse { darl = last.value, response = response, matches = list });
                if (!link.unknown)
                    outList.Add(new InteractTestResponse { darl = last.value, response = link, matches = list });
                if (!callResponse.unknown)
                    outList.Add(new InteractTestResponse { darl = last.value, response = callResponse, matches = list });
                if (!graphResponse.unknown)
                    outList.Add(new InteractTestResponse { darl = last.value, response = graphResponse, matches = list });
                list.RemoveAt(list.Count - 1); //remove the last match if no valid response
            }
            return outList;
        }

        /// <summary>
        /// Single step discovery operator
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="knowledgeGraphName"></param>
        /// <param name="subjectId">KS start name or externalId</param>
        /// <param name="lineages">permitted paths</param>
        /// <param name="log">Verbose description of path taken and why</param>
        /// <param name="currentTime">The time to use for the analysis if temporal.</param>
        /// <returns>A list of possible accessible states in depth first search order including intermediate states.</returns>
        /// <exception cref="MetaRuleException"></exception>
        public async Task<KnowledgeState> Discover(string userId, string knowledgeGraphName, string subjectId, List<string> lineages, StringBuilder log, DarlTime? currentTime)
        {
            var model = await _graph.GetModel(userId, knowledgeGraphName);
            if (model == null)
            {
                throw new MetaRuleException($"{knowledgeGraphName} doesn't exist in your account");
            }
            KnowledgeState ks = await _graph.GetKnowledgeState(userId, subjectId, knowledgeGraphName);
            if (ks == null)
            {
                throw new MetaRuleException($"{subjectId} subjectId doesn't exist.");
            }
            //used to record progress
            log.AppendLine($"Starting discovery from object {subjectId}, {DateTime.UtcNow}.");
            var kr = new KnowledgeRecord { subjectId = ks.subjectId, userId = ks.userId, created = ks.created, knowledgeGraphName = ks.knowledgeGraphName, processId = ks.processId, data = ks.data };
            var res = new KnowledgeState { userId = userId, knowledgeGraphName = knowledgeGraphName};
            await RecursiveDiscovery(model, kr, res, subjectId, 1.0, lineages, log, currentTime);
            log.AppendLine($"Completed discovery from object {subjectId}, {DateTime.UtcNow}.");
            return res;
        }
        /*
                /// <summary>
                /// Discovery pass interrupted by data requests when unknown values are encountered
                /// </summary>
                /// <param name="userId"></param>
                /// <param name="graphName"></param>
                /// <param name="subjectId"></param>
                /// <param name="targetId"></param>
                /// <param name="paths"></param>
                /// <param name="completionLineage"></param>
                /// <param name="values"></param>
                /// <param name="pending"></param>
                /// <returns></returns>
                public Task<(List<InteractTestResponse>, DarlVar)> DiscoverPass(string userId, string graphName, string subjectId, string targetId, List<string> paths, string completionLineage, List<DarlVar> values, DarlVar? pending)
                {
                    throw new NotImplementedException();
                }*/

        /*        public async Task<List<InteractTestResponse>> DiscoverForBot(string userId, string KnowledgeGraphName, string subjectId, List<string> lineages, string conversationID)
                {
                    var resultKS = new KnowledgeStateInput { knowledgeGraphName = KnowledgeGraphName, subjectId = conversationID };
                    var list = new List<InteractTestResponse>();
                    var path = new List<string>();
                    var ksList = await Discover(userId, KnowledgeGraphName, subjectId, lineages);
                    bool inTerminators = false; //ksList is in depth-first search order. After a block of leaf nodes we need to pop the last path node. 
                    foreach (var ks in ksList)
                    {
                        foreach (var i in ks.data.Keys)
                        {
                            if (ks.ContainsAttribute(i, _metaHandler.CommonLineages["complete"]))
                            {//it's a discovered end point
                                inTerminators = true;
                                var route = new List<string>();
                                route.AddRange(path);
                                route.Add(ks.subjectId);
                                var itr = new InteractTestResponse { darl = "", matches = new List<MatchedElement>(), reference = ks.subjectId, activeNodes = route };
                                var sb = new StringBuilder();
                                double affinity = 0.0;
                                foreach (var att in ks.data[i])
                                {
                                    if (att.lineage != _metaHandler.CommonLineages["complete"])
                                    {
                                        sb.AppendLine($"{att.name}: {att.value}");
                                    }
                                    else
                                    {
                                        affinity = att.confidence;
                                    }
                                }
                                itr.response = new DarlVar { dataType = DarlVar.DataType.textual, name = "response", Value = sb.ToString(), weight = affinity };
                                list.Add(itr);
                                resultKS.data.Add(new StringListGraphAttributeInputPair { name = ks.subjectId, value = ks.ConvertInputList(i) });
                            }
                            else
                            {
                                if (inTerminators)
                                {
                                    if (path.Any())
                                        path.Remove(path.Last());
                                }
                                inTerminators = false;
                                path.Add(ks.subjectId);
                            }
                        }
                    }
                    await _graph.CreateKnowledgeState(userId, resultKS);
                    return list.OrderByDescending(a => a.response.weight).ToList();
                }
        */
        #region private

        private async Task<KnowledgeState> GetKnowledgeState(string userId, string subjectId, string graphName)
        {
            var ks = await _graph.GetKnowledgeState(userId, subjectId, graphName);
            if (ks == null)
                ks = new KnowledgeState { userId = userId, subjectId = subjectId, knowledgeGraphName = graphName };
            return ks;
        }

        private bool Validate(DarlVar? pending, List<DarlVar> values, out string validationResponse)
        {
            validationResponse = "";
            if (!(pending is null))
            {
                var question = values.Where(a => a.name == questionIdentifier).FirstOrDefault();
                if (pending != null)
                {
                    if (question != null)
                    {
                        if (pending.sequence != null)
                            question.name = pending.sequence[0][0];
                        else question.name = pending.name;
                        question.dataType = pending.dataType;
                        question.unknown = false;
                        question.weight = 1.0;
                    }
                }
                if (!(question is null))
                {
                    switch (pending.dataType)
                    {
                        case DarlVar.DataType.categorical:
                            string q = question.Value.Trim().ToLower();
                            foreach (var c in pending.categories.Keys)
                            {
                                if (c.ToLower() == q)
                                {
                                    //categories can be transcribed - revert to value used in ruleset.
                                    //question.Value = LookupCategory(pending.name, question.Value, rf, "");
                                    return true;
                                }
                            }
                            validationResponse = "You must give one of the choices";
                            values.Remove(question);
                            return false;
                        case DarlVar.DataType.numeric:
                            double dval;
                            if (double.TryParse(question.Value, out dval))
                            {
                                if (pending.values != null && pending.values.Count >= 2)
                                {
                                    if (dval >= pending.values.First() && dval <= pending.values.Last())
                                        return true;
                                    validationResponse = $"The value must be between {pending.values.First()} and {pending.values.Last()} inclusive";
                                    values.Remove(question);
                                    return false;
                                }
                                else
                                {
                                    return true;
                                }
                            }
                            validationResponse = "You must give a number";
                            values.Remove(question);
                            return false;
                        case DarlVar.DataType.textual:
                            if (pending.categories != null) //regex present
                            {
                                var regex = new Regex(pending.categories.Keys.First());
                                if (!regex.Match(question.Value.Trim()).Success)
                                {
                                    //some sources decorate emails etc with html. in this case search through attributes for a valid section
                                    var source = question.Value.Trim();
                                    int index = 0;
                                    while (index < source.Length - 1 && (index = source.IndexOf('"', index)) != -1)//search attributes
                                    {
                                        int endIndex = source.IndexOf('"', index + 1);
                                        if (endIndex != -1)
                                        {
                                            var poss = source.Substring(index + 1, endIndex - (index + 1));
                                            if (regex.Match(poss.Trim()).Success)
                                            {
                                                question.Value = poss.Trim();
                                                return true;
                                            }
                                            index = endIndex + 1;
                                        }
                                    }
                                    index = 0;
                                    while (index < source.Length - 1 && (index = source.IndexOf('>', index)) != -1)//search text elements
                                    {
                                        int endIndex = source.IndexOf('<', index + 1);
                                        if (endIndex != -1)
                                        {
                                            var poss = source.Substring(index + 1, endIndex - (index + 1));
                                            if (regex.Match(poss.Trim()).Success)
                                            {
                                                question.Value = poss.Trim();
                                                return true;
                                            }
                                            index = endIndex + 1;
                                        }
                                    }
                                    validationResponse = "The response is not in the right format";
                                    values.Remove(question);
                                    return false;
                                }

                            }
                            if (pending.values != null && pending.values.Count > 0) //first value is max length expressed as double
                            {
                                if (question.Value.Length > (int)pending.values[0])
                                {
                                    validationResponse = $"Text is longer than {(int)pending.values[0]} characters, the maximum allowed";
                                    values.Remove(question);
                                    return false;
                                }
                            }
                            break;
                    }
                }
            }
            return true;
        }


        private void GenerateQuestionMessage(Dictionary<string, double> c, List<InteractTestResponse> responses, ParseTree tree, ref DarlVar pending, GraphObject res, string code, List<Meta.DarlResult> list)
        {
            //add the annotation by reading the annotation result
            var annot = list.Where(a => a.name == annotationSignum).FirstOrDefault();
            var next = (from entry in c orderby entry.Value descending select entry.Key).FirstOrDefault(); //find most salient
            var text = string.Empty;
            if (annot is null || annot.IsUnknown())
            {
                text = next; //use the variable name as a question
            }
            else
            {
                text = annot.Value.ToString(); //text provided
            }
            //now choose how to present this based on the type of the data-item;
            //look up the input using introspection into the tree.
            var nextRes = tree.GetInputs().Where(a => a.name == next).FirstOrDefault();
            if (!(nextRes is null))
            {
                switch (nextRes.iType)
                {
                    case Meta.InputDefinitionNode.InputTypes.categorical_input:
                        {
                            var cats = new Dictionary<string, double>();
                            foreach (var i in nextRes.categories) cats.Add(i, 1.0);
                            pending = new DarlVar { dataType = DarlVar.DataType.categorical, categories = cats, name = res.id, sequence = new List<List<string>> { new List<string> { nextRes.name } } };
                            responses.Add(new InteractTestResponse { response = new DarlVar { dataType = DarlVar.DataType.categorical, categories = cats, name = questionIdentifier, Value = text }, reference = res.externalId, darl = code, activeNodes = new List<string> { res.id } });
                        }
                        break;
                    case Meta.InputDefinitionNode.InputTypes.numeric_input:
                        {
                            var range = tree.GetInputRange(next);
                            var drange = new List<double> { double.NegativeInfinity, double.PositiveInfinity };
                            if (range.values.Count() >= 2)
                            {
                                drange[0] = (double)range.values[0];
                                drange[1] = (double)range.values.Last();
                            }
                            pending = new DarlVar { dataType = DarlVar.DataType.numeric, name = res.id, values = drange, sequence = new List<List<string>> { new List<string> { nextRes.name } } };
                            responses.Add(new InteractTestResponse { response = new DarlVar { dataType = DarlVar.DataType.numeric, name = questionIdentifier, Value = text, values = drange }, reference = res.externalId, darl = code, activeNodes = new List<string> { res.id } });
                        }
                        break;
                    default:
                        {
                            pending = new DarlVar { dataType = DarlVar.DataType.textual, name = res.id, sequence = new List<List<string>> { new List<string> { nextRes.name } } };
                            responses.Add(new InteractTestResponse { response = new DarlVar { dataType = DarlVar.DataType.textual, name = questionIdentifier, Value = text }, reference = res.externalId, darl = code, activeNodes = new List<string> { res.id } });
                        }
                        break;
                }
            }
        }


        private void GenerateInfoMessage(List<InteractTestResponse> responses, List<Meta.DarlResult> list, string code, GraphObject res)
        {
            var annot = list.Where(a => a.name == annotationSignum).FirstOrDefault();
            var text = "Thanks for that information.";
#pragma warning disable CS8604 // Possible null reference argument.
            if (annot.Exists() && !annot.IsUnknown()) //!= is overridden for DarlResult
#pragma warning restore CS8604 // Possible null reference argument.
                text = annot.Value.ToString();
            responses.Add(new InteractTestResponse { response = new DarlVar { dataType = DarlVar.DataType.textual, name = questionIdentifier, Value = text ?? String.Empty }, reference = res.externalId, darl = code, activeNodes = new List<string> { res.id ?? String.Empty } });
        }

        private async Task<(bool, DarlVar?)> EvaluateUIRule(IGraphModel model, GraphAbstraction? res, DarlVar? pending, List<InteractTestResponse> responses, KnowledgeState ks, List<DarlVar> values, List<string> lineages, bool data = false)
        {
            if (res != null)
            {
                GraphObject? o = null;
                if (res is KnowledgeRecord)
                {
                    o = ((KnowledgeRecord)res).DeReference(model, lineages).Item1;
                }
                else
                {
                    o = res as GraphObject;
                }
                //find display rule
                var code = _graph.FindDisplayAttribute(model, o.id);
                if (string.IsNullOrEmpty(code))
                {
                    if (data)//pending != null, no code and first pass = codeless
                    {
                        _graph.HandleCodelessValue(model, o, pending, values, ks);
                        return (false, pending);
                    }
                    else
                    {
                        if (_graph.FindMetaDisplayStructure(model, o, ref pending, responses))
                            return (true, pending);
                        throw new MetaRuleException($"There is no display rule defined for GraphObject {res.ToString()}");
                    }
                }
                //evaluate it
                var tree = _runtime.CreateTree(code, o, model); //findControlAttribute checks for syntax errors.
                var list = Meta.DarlVarExtensions.Convert(values);
                await _runtime.Evaluate(tree, list, ks);
                //calculate saliences if any outstanding turn into a question
                var c = _runtime.CalculateSaliences(list, tree);
                if (c.Any())
                {
                    GenerateQuestionMessage(c, responses, tree, ref pending, o, code, list);
                    return (true, pending);
                }
                if (!data)
                    GenerateInfoMessage(responses, list, code, o);
            }
            return (false, pending);
        }

        /// <summary>
        /// Hook for expansion to common-sense based evaluation
        /// </summary>
        /// <remarks>Only called if no other source of rules exists</remarks>
        /// <param name="model">The model</param>
        /// <param name="res">The current object</param>
        /// <param name="pending">The data item to be filled in</param>
        /// <param name="responses">The  response to send to the user</param>
        /// <returns></returns>
        private bool FindMetaStructure(IGraphModel model, GraphObject res, DarlVar pending, List<InteractTestResponse> responses)
        {
            //This will start hard wired for a couple of scenarios

            throw new NotImplementedException();
        }

        private async Task UpdateNodeStates(KnowledgeState ks, IGraphModel model, List<KeyValuePair<GraphAbstraction, int>> dependencies, List<DarlVar> values, string completionLineage)
        {
            if (dependencies == null)
                return;
            for (int n = dependencies.Count - 1; n >= 0; n--) //evaluate dependencies in reverse order
            {
                var o = dependencies[n];
                var key = ((GraphObject)o.Key);
                if (values.Any(a => a.name == key.id || a.name == key.externalId))
                {
                    var val = values.First(a => a.name == key.id || a.name == key.externalId);
                    var att = new GraphAttribute { id = Guid.NewGuid().ToString(), lineage = completionLineage, name = "completed", type = GraphAttribute.DataType.categorical, value = "true", confidence = val.weight };
                    ks.AddAttribute(key.id, att);
                }
                else if (ks.ContainsAttribute(key.id, completionLineage))
                {
                    continue;
                }
                else if (key.ContainsAttribute(completionLineage)) //preset node - dump any attributes into the KS
                {
                    foreach (var att in key.properties)
                    {
                        ks.AddAttribute(key.id, att);
                    }
                }
                else if (key.Out.Any())//leaf nodes can't have inferences
                {
                    if (key.properties != null) //look locally
                    {
                        var completed = key.properties.Where(a => a.lineage.StartsWith(completionLineage)).FirstOrDefault();
                        if (completed != null)//assume state is either unknown or completed
                        {
                            if (completed.type != GraphAttribute.DataType.ruleset)
                                continue;
                        }
                    }
                    //if we get here the required target is not complete - get the rules.
                    var ruleSource = model.FindControlAttribute(key.id, completionLineage);
                    if (string.IsNullOrEmpty(ruleSource))
                    {
                        _graph.HandleCodelessCompletion(model, key, ks);
                        continue;
                    }
                    _logger.LogInformation($"evaluating completion rule on object: {key.externalId ?? key.name}, code: {ruleSource}");
                    var tree = _runtime.CreateTree(ruleSource, key, model);
                    if (tree.HasErrors())
                    {
                        _logger.LogInformation($"Errors in completion rule on object: {key.externalId ?? key.name}, code: {ruleSource}");
                        continue;
                    }
                    var list = new List<Thinkbase.Meta.DarlResult>();
                    await _runtime.Evaluate(tree, list, ks);
                    _logger.LogInformation($"Completion rule results: {string.Join("\n", list)}");
                }
            }
        }

        private List<GraphAbstraction> FindNext(IGraphModel model, List<KeyValuePair<GraphAbstraction, int>> ordered, KnowledgeState ks, GraphAbstraction? node, List<string> paths, string completedLineage)
        {
            var list = new List<GraphAbstraction>();
            if (ordered == null || ks == null || node == null || paths == null || string.IsNullOrEmpty(completedLineage))
            {
                _logger.LogError("Bad parameters to FindNext");
                return list;
            }
            //if target is completed even though other dependencies remain return an empty list
            if (ks.ContainsAttribute(node.Id(model), completedLineage))
                return list;
            //first build dependency list of nodes reachable from the start node
            var saliences = new List<SalienceRecord>();
            //in descending order calculate salience
            //            saliences.Add(node, 1.0);
            foreach (var o in ordered)
            {
                double salience = 0.0;
                foreach (var c in ((GraphObject)o.Key).In)
                {
                    if (c.lineage != null && paths.Contains(c.lineage))
                    {
                        var parentNode = model.vertices[c.startId];
                        salience += saliences.Any(a => a.gobj == parentNode) ? saliences.First(a => a.gobj == parentNode).salience : 1.0;
                    }
                }
                saliences.Add(new SalienceRecord { gobj = o.Key as GraphObject, salience = salience });
            }
            saliences.Sort();
            var currentSalience = 0.0;
            foreach (var o in saliences)
            {
                var obj = o.gobj;
                currentSalience = Math.Max(currentSalience, o.salience);
                if (currentSalience != o.salience && list.Count > 0)
                    break;
                if (obj.Out(model).Count > 0) // not leaf
                    continue;
                if (ks.ContainsAttribute(obj.Id(model), completedLineage) || obj.ContainsAttribute(completedLineage))
                    continue;
                list.Add(obj);
            }
            //list is leaf nodes with highest salience
            return list;
        }

        /// <summary>
        /// Gets a list of nodes that are connected to the target where the most remote have an execution order of 1
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="node">The target node</param>
        /// <param name="paths">connection lineages to search over</param>
        /// <returns>The list</returns>
        private List<KeyValuePair<GraphAbstraction, int>> GetExecutionOrder(IGraphModel model, GraphAbstraction node, List<string> paths)
        {
            var dependencies = new List<Dependency>();
            if (node != null)
            {
                foreach (var c in node.Out(model))
                {
                    if (paths.Contains(c.lineage))
                    {
                        var childNode = model.vertices[c.endId];
                        AddDependency(model, dependencies, node, childNode, c.lineage, paths);
                    }
                }
            }
            //Now establish sequence
            int currentSequence = 1;
            var sequences = new Dictionary<GraphAbstraction, int>();
            bool complete = false;
            while (!complete)
            {
                var deletions = new List<Dependency>();
                foreach (var dep in dependencies)
                {
                    if (!sequences.ContainsKey(dep.dependent))
                        sequences.Add(dep.dependent, currentSequence);
                    else
                        sequences[dep.dependent] = currentSequence;
                    //if dependent does not match any parents
                    //remove that link
                    bool match = false;
                    foreach (Dependency otherDep in dependencies)
                    {
                        if (otherDep.parent == dep.dependent)
                        {
                            match = true;
                            break;
                        }
                    }
                    if (!match)
                        deletions.Add(dep);
                }
                if (deletions.Count == 0 && dependencies.Count > 0)
                {
                    throw new MetaRuleException("Loop found in nodes");
                }
                foreach (Dependency del in deletions)
                {
                    dependencies.Remove(del);
                }
                currentSequence++;
                complete = dependencies.Count == 0;
            }
            //sort dependency list
            return sequences.OrderByDescending(a => a.Value).ToList();
        }



        /// <summary>
        /// Recursive Search over KnowledgeRecords 
        /// </summary>
        /// <param name="model">The model</param>
        /// <param name="ks">The current search point</param>
        /// <param name="res">Where we've been</param>
        /// <param name="startSubjectId">The start subject id</param>
        /// <param name="weight">the minimum weight of the path taken.</param>
        /// <param name="lineages">Lineages to limit search to</param>
        /// <param name="log">Textual log of discovery process</param>
        /// <param name="currentTime">The time of the analysis</param>
        /// <returns></returns>
        private async Task RecursiveDiscovery(IGraphModel model, KnowledgeRecord ks, KnowledgeState state, string startSubjectId, double weight, List<string> lineages, StringBuilder log, DarlTime? currentTime)
        {
            //Assume that ks holds the start node
            var refs = ks.DeReference(model, lineages);
            var currentNode = refs.Item1;
            var connections = refs.Item2;

            if (currentNode == null) // not using KS data
            {
                currentNode = model.vertices.Values.Where(a => a.externalId == startSubjectId).FirstOrDefault();
                if (currentNode == null)
                    throw new MetaRuleException($"No node found in {ks.subjectId}");
                log.AppendLine($"Processing Node: {currentNode.externalId}, { DateTime.UtcNow}.");
                state.data.Add(currentNode.id ?? String.Empty, new List<GraphAttribute> { new GraphAttribute { name = "completed", type = GraphAttribute.DataType.categorical, value = "true", lineage = _metaHandler.CommonLineages["complete"] } });
                await RecursiveDiscovery(model, state, currentNode, weight, lineages, log, currentTime);
                return;
            }
            if (ks.subjectId != startSubjectId)//don't halt on the first
            {
                if (!await CheckCodeCompletion(model, state, currentNode, log, currentTime))
                    return;
            }

            //Search using inferred links in this KS and non-inferred in the model.

            log.AppendLine($"Processing KnowledgeState: {ks.subjectId}, { DateTime.UtcNow}.");
            //first pass collect required subject Ids and weights
            var ids = new List<(string, double)>();
            foreach (var s in connections.Where(a => a.inferred == true))
            {
                //to speed this up, batch the GetKnowledgeState calls so that all linked KSs are fetched in one go. 
                foreach (var a in ks.data[s.id ?? string.Empty])
                {
                    if (a.type == GraphAttribute.DataType.connection)
                    {
                        var subjectId = a.value;
                        if (!state.ContainsRecord(subjectId))//avoid loops
                        {
                            ids.Add((subjectId, s.weight));
                        }
                    }
                }
            }
            var distinctIds = ids.Select(a => a.Item1).Distinct().ToList();
            var statesToVisit = await _graph.GetSetofConnectedObjects(ks.userId, distinctIds, ks.knowledgeGraphName);
            if (statesToVisit != null)
            {
                var index = 0;
                foreach (var newKs in statesToVisit)
                {
                    if (Coexists(newKs, ks, model, currentTime))
                    {

                        if (newKs is KnowledgeRecord)
                        {
                            state.data.Add(ks.subjectId, new List<GraphAttribute> { new GraphAttribute { name = "completed", type = GraphAttribute.DataType.categorical, value = "true", lineage = _metaHandler.CommonLineages["complete"] } });
                            await RecursiveDiscovery(model, (KnowledgeRecord)newKs, state, startSubjectId, Math.Min(weight, ids[index].Item2), lineages, log, currentTime);
                        }
                        else if (newKs is GraphObject)
                        {
                            var go = (GraphObject)newKs;
                            state.data.Add(go.id ?? String.Empty, new List<GraphAttribute> { new GraphAttribute { name = "completed", type = GraphAttribute.DataType.categorical, value = "true", lineage = _metaHandler.CommonLineages["complete"] } });
                            await RecursiveDiscovery(model, state, go, Math.Min(weight, ids[index].Item2), lineages, log, currentTime);
                        }
                        index++;
                    }
                }
            }
            else
            {
                _logger.LogWarning($"Broken link: userId {ks.userId}, graphName {ks.knowledgeGraphName}, subjectId {ks.subjectId}");
            }
            //handle real top level connections.
            foreach (var s in connections.Where(a => a.inferred == false))
            {
                var endObject = model.vertices[s.endId];
                if (!CheckForLoop(state,endObject) && Coexists(endObject, ks, model, currentTime))//avoid loops
                {
                    state.data.Add(endObject.id ?? String.Empty, new List<GraphAttribute> { new GraphAttribute { name = "completed", type = GraphAttribute.DataType.categorical, value = "true", lineage = _metaHandler.CommonLineages["complete"] } });
                    await RecursiveDiscovery(model, state, endObject, Math.Min(weight, s.weight), lineages, log, currentTime);
                }
            }


        }

        private async Task<bool> CheckCodeCompletion(IGraphModel model, KnowledgeState ks, GraphObject currentNode, StringBuilder log, DarlTime? currentTime)
        {
            var code = _graph.FindCompleteAttribute(model, currentNode.id ?? String.Empty);
            if (!string.IsNullOrEmpty(code)) //no code implies discovery can continue.
            {
                //evaluate it
                var tree = _runtime.CreateTree(code, currentNode, model); //findControlAttribute checks for syntax errors.
                var list = new List<Meta.DarlResult>();
                await _runtime.Evaluate(tree, list, ks); //add current time for eval
                //return false if further movement not possible.
                var complete = list.FirstOrDefault(a => a.name == "completed");
                if (!complete.Exists() || (complete.Value as string) == "false" || complete.GetWeight() < 0.1)
                {
                    log.AppendLine($"Node not completed. code: {code}, { DateTime.UtcNow}.");
                    return false; //rules prevent further search
                }
            }
            return true;
        }

        private bool CheckForLoop(KnowledgeState state, GraphObject next)
        {
            if(!string.IsNullOrEmpty(next.id))
            {
                return state.ContainsRecord(next.id);
            }
            return true;
        }

        /// <summary>
        /// Recursive search over GraphObjects
        /// </summary>
        /// <param name="model"></param>
        /// <param name="ks"></param>
        /// <param name="res"></param>
        /// <param name="currentNode"></param>
        /// <param name="weight"></param>
        /// <param name="lineages"></param>
        /// <param name="log"></param>
        /// <param name="currentTime"></param>
        /// <returns></returns>
        private async Task RecursiveDiscovery(IGraphModel model, KnowledgeState state, GraphObject currentNode, double weight, List<string> lineages, StringBuilder log, DarlTime? currentTime)
        {

            if (!await CheckCodeCompletion(model, state, currentNode, log, currentTime))
                return;
            log.AppendLine($"Processing GraphObject: {currentNode.externalId}, { DateTime.UtcNow}.");
            foreach (var l in currentNode.Out)
            {
                var endObject = model.vertices[l.endId];
                if (!CheckForLoop(state, endObject) && Coexists(endObject, currentNode, model, currentTime))//avoid loops and objects that don't coexist
                {
                    state.data.Add(endObject.id ?? String.Empty, new List<GraphAttribute> { new GraphAttribute { name = "completed", type = GraphAttribute.DataType.categorical, value = "true", lineage = _metaHandler.CommonLineages["complete"] } });
                    await RecursiveDiscovery(model, state, endObject, Math.Min(weight, l.weight), lineages, log, currentTime);
                }
            }
        }


        private bool Coexists(GraphAbstraction endObject, GraphAbstraction currentNode, IGraphModel model, DarlTime? currentTime)
        {
            return currentNode.Coexists(endObject, model, currentTime) > 0.0;
        }





        /// <summary>
        /// Recursively searches a network for dependencies
        /// </summary>
        /// <param name="model"></param>
        /// <param name="dependencies"></param>
        /// <param name="currentParent"></param>
        /// <param name="currentNode"></param>
        /// <param name="linkLineage"></param>
        /// <param name="paths"></param>
        private void AddDependency(IGraphModel model, List<Dependency> dependencies, GraphAbstraction currentParent, GraphObject currentNode, string linkLineage, List<string> paths)
        {
            dependencies.Add(new Dependency { dependencyLineage = linkLineage, dependent = currentNode, parent = currentParent });
            foreach (var c in currentNode.Out)
            {
                if (paths.Contains(c.lineage))
                {
                    var childNode = model.vertices[c.endId];
                    AddDependency(model, dependencies, currentNode, childNode, c.lineage, paths);
                }
            }
        }



        #endregion
    }
    internal class Dependency
    {
        public GraphAbstraction parent { get; set; }
        public GraphAbstraction dependent { get; set; }

        public string dependencyLineage { get; set; }
    }
}
