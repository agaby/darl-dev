using Darl.Common;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase.Meta;
using DarlCommon;
using DarlCompiler.Parsing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using static Darl.Lineage.Bot.IBotProcessing;
using static Darl.Thinkbase.IGraphHandler;

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



        private static int minimumData = 5;

        private readonly ConcurrentDictionary<string, DarlMetaRunTime> runtimes = new ConcurrentDictionary<string, DarlMetaRunTime>();
        private readonly ConcurrentDictionary<string, DateTime> runtimeLifetimes = new ConcurrentDictionary<string, DateTime>();


        private readonly IGraphProcessing _graph;
        private readonly ILogger<GraphHandler> _logger;
        private readonly IMetaStructureHandler _metaHandler;
        private readonly IConfiguration _config;

        public GraphHandler(IConfiguration config, IGraphProcessing graph, ILogger<GraphHandler> logger, IMetaStructureHandler metaHandler)
        {
            _graph = graph;
            _logger = logger;
            _metaHandler = metaHandler;
            _config = config;
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
        public async Task<(List<InteractTestResponse>, DarlVar?)> GraphPass(KnowledgeState ks, IGraphModel model, string subjectId, string targetId, List<string> paths, string completionLineage, List<DarlVar> values, DarlVar? pending, GraphProcess graphProcess)
        {
            var runtime = GetRuntime(subjectId);
            //validate incoming values
            string validationResponse;
            if (!Validate(pending, values, out validationResponse)) //out of range value
            {
                _logger.LogInformation($"Validation error = {validationResponse}, KGName= {model.modelName}");
                return (new List<InteractTestResponse> { new InteractTestResponse { darl = "", response = new DarlVar { name = "response", dataType = DarlVar.DataType.textual, Value = validationResponse }, matches = new List<MatchedElement>() } }, pending);
            }
            var responses = new List<InteractTestResponse>();
            if (!(pending is null))
            {
                var currentObj = model.vertices[pending.name];
                _logger.LogInformation($"Evaluating response = {currentObj.externalId ?? currentObj.name}, KGName= {model.modelName}");
                var vals = await EvaluateUIRule(runtime, model, currentObj, pending, responses, ks, values, paths, true);
                if (vals.Item1)
                {
                    return (responses, vals.Item2);
                }
            }
            //Use inference to update state based on new information
            var target = model.vertices.ContainsKey(targetId) ? model.vertices[targetId] : model.vertices.Values.Where(a => a.externalId == targetId).FirstOrDefault();
            List<GraphAbstraction>? res = null;
            switch (graphProcess)
            {
                case GraphProcess.seek:
                    {
                        _logger.LogInformation($"Seeking target {target}");
                        var dependencies = GetExecutionOrder(model, target, paths);
                        if (target != null)
                            dependencies.Insert(0, new KeyValuePair<GraphAbstraction, int>(target, 1));
                        else
                        {
                            _logger.LogError($"Target not found in Bot GraphPass for Seek; id: {targetId}, graphName: {model.modelName}.");
                        }
                        _logger.LogInformation($"{dependencies.Count} dependencies found.");
                        await UpdateNodeStates(runtime, ks, model, dependencies, values, completionLineage);
                        //find next element to present or terminate
                        res = FindNext(model, dependencies, ks, target, paths, completionLineage);
                    }
                    break;
                case GraphProcess.discover:
                    {
                        _logger.LogInformation($"Discovering target {target}");
                        //do a breadth-first search out from this node, stopping whenever a ruleset is encountered dependent on an unknown data item
                        var visited = new List<GraphAbstraction>();
                        res = DiscoveryProcess(model, target, paths, visited);
                    }
                    break;
            }
            if (res != null && res.Count > 0)
            {
                values.Clear();
                var vals = await EvaluateUIRule(runtime, model, res.First(), pending, responses, ks, values, paths);
                pending = vals.Item2;
                if (!responses.Any())
                {
                    _logger.LogError($"No responses generated for {res.First().Name(model)}.");
                }
                else
                {
                    var r = responses.Last();
                    var text = r.response.Value;
                    var darl = r.darl;
                    var refer = r.reference;
                    _logger.LogInformation($"Returned text: {text}, darl source: {darl}, reference = {refer}");
                }
                if (pending != null)
                    _logger.LogInformation($"{pending.name} selected as next question node. datatype: {pending.dataType}, weight: {pending.weight}, unknown: {pending.unknown} for model.modelName: {model.modelName}");
                else
                    _logger.LogInformation("Next question node is null.");
            }
            else
            {
                _logger.LogInformation($"Completed seek  to {targetId}, KGName= {model.modelName}");
                responses.Clear();
                await EvaluateUIRule(runtime, model, target, pending, responses, ks, values, paths);
                if (!responses.Any())
                    responses.Add(new InteractTestResponse { response = new DarlVar { dataType = DarlVar.DataType.complete, Value = "This process is complete.", name = "response" } });
                pending = null;
            }
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
        public async Task<List<InteractTestResponse>> InterpretText(IGraphModel model, string subjectId, DarlVar conversationData)
        {
            var tokens = LineageLibrary.SimpleTokenizer(conversationData.Value);
            var response = new DarlVar { dataType = DarlVar.DataType.textual, Value = "No response generated...", unknown = true };
            var link = new DarlVar { dataType = DarlVar.DataType.link, unknown = true, Value = "" };
            var callResponse = new DarlVar { dataType = DarlVar.DataType.ruleset, unknown = true, Value = "" };
            var graphResponse = new DarlVar { dataType = DarlVar.DataType.seek, unknown = true, Value = "" };
            var outList = new List<InteractTestResponse>();
            List<MatchedElement> list = await _graph.Match(model!, subjectId, tokens);
            while (list.Any() && (response.unknown || response.weight < 1.0))
            {
                var lastMatch = list.Last();
                if (lastMatch == null) //no response
                    break;
                var last = ((MatchedGraphAttribute)lastMatch).terminus;
                var values = lastMatch.values;
                try
                {
                    if (last.type == GraphAttribute.DataType.markdown) //just return the text
                    {
                        response = new DarlVar { dataType = DarlVar.DataType.textual, Value = last.value, name = "response" };
                    }
                    else
                    {
                        if (values != null)
                            _logger.LogInformation($"Processing Match: {values.Count} incoming values: {string.Join(',', values)}");
                        else
                            _logger.LogInformation($"Processing Match");
                        var _runtime = new DarlMetaRunTime(_config, _metaHandler);
                        var tree = _runtime.CreateTree(last.value, null, model);
                        var vals = Meta.DarlVarExtensions.Convert(values);
                        await _runtime.Evaluate(tree, vals, null);
                        values = Meta.DarlVarExtensions.Convert(vals);
                        _logger.LogInformation($"Processing Match: {values.Count} outgoing values: {string.Join(',', values)}");
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
                catch (Exception ex) //probably missing IO or access denied
                {
                    _logger.LogError(ex, $"GraphHandler InterpretText failure. model: {model.modelName}, code: {last.value}");
                    if (list.Count > 1)
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
        public async Task<KnowledgeState> Discover(string userId, string knowledgeGraphName, string subjectId, List<string> lineages, StringBuilder log, FuzzyTime? currentTime)
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
            log.AppendLine($"Starting discovery from object {subjectId}, Evaluation Time: {currentTime ?? new FuzzyTime(DarlTime.UtcNow)} {DateTime.UtcNow}.");
            var kr = new KnowledgeRecord { subjectId = ks.subjectId, userId = ks.userId, created = ks.created, knowledgeGraphName = ks.knowledgeGraphName, processId = ks.processId, data = ks.data };
            var res = new KnowledgeState { userId = userId, knowledgeGraphName = knowledgeGraphName, subjectId = Guid.NewGuid().ToString(), created = DateTime.UtcNow };
            await RecursiveDiscovery(model, kr, res, subjectId, 1.0, lineages, log, currentTime, 0);
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



        public async Task<KnowledgeState> Seek(KnowledgeState ks, string? targetId, List<string> paths, string completionLineage)
        {
            var runtime = new DarlMetaRunTime(_config, _metaHandler);
            var values = new List<DarlVar>();
            var model = await _graph.GetModel(ks.userId, ks.knowledgeGraphName);
            var target = await _graph.GetGraphObjectById(model.modelName, targetId ?? model.defaultTarget);
            var dependencies = GetExecutionOrder(model, target, paths);
            if (target != null)
                dependencies.Insert(0, new KeyValuePair<GraphAbstraction, int>(target, 1));
            await UpdateNodeStates(runtime, ks, model, dependencies, values, completionLineage);
            return ks;
        }

        public async Task<Meta.DarlMineReport> Learn(string userId, string graphName, string target, LearningForm form, string targetLineage, string valueLineage, int percentTrain = 100, SetChoices sets = SetChoices.three)
        {
            var runtime = new DarlMetaRunTime(_config, _metaHandler);
            if (form == LearningForm.association || form == LearningForm.unsupervised)
                throw new NotImplementedException();
            //get all the KnowledgeStates for this ruleset
            var model = await _graph.GetModel(userId, graphName);
            if (model == null)
                throw new MetaRuleException($"{graphName} doesn't exist in your account");
            var obj = await _graph.GetGraphObjectById($"{userId}_{graphName}", target);
            if (obj == null)
            {
                obj = await _graph.GetGraphObjectByExternalId($"{userId}_{graphName}", target);
            }
            if (obj == null)
            {
                throw new MetaRuleException($"{target} doesn't exist in {graphName}");
            }
            if (target != obj.id)
                target = obj.id ?? "";
            var data = await _graph.GetKnowledgeStatesByType(userId, target, graphName);
            return await SupervisedCore(data, model, target, targetLineage, valueLineage, percentTrain, sets, runtime);
        }


        public async Task<DarlMineReport> Build(string userId, string name, string data, string patternPath, List<DataMap> rawDataMaps, LoadType ltype = LoadType.xml, LearningForm form = LearningForm.supervised)
        {
            var runtime = new DarlMetaRunTime(_config, _metaHandler);
            var model = await _graph.GetModel(userId, name);
            if (model == null) //create one
            {
                await _graph.CreateNewGraph(userId, name);
                model = await _graph.GetModel(userId, name);
            }
            else
            {
                model.Clear();
                model.AddDefaultContent();
            }
            var dataMaps = FixDataMapLineages(rawDataMaps);
            var target = dataMaps.FirstOrDefault(a => a.target);
            var compositeName = userId + "_" + name;
            DarlMineReport bestReport = new DarlMineReport();
            if (target != null && form == LearningForm.supervised) //supervised
            {
                var TargetObj = await CreateNode(target, compositeName);
                //add value, text and completed
                var targetText = $"The value of {TargetObj.externalId} is inferred to be %%{TargetObj.externalId}%%.";
                TargetObj.properties = new List<GraphAttribute>();
                TargetObj.properties.Add(new GraphAttribute { name = "value", lineage = _metaHandler.CommonLineages["answer"], type = target.dataType, confidence = 1.0, id = Guid.NewGuid().ToString() });
                TargetObj.properties.Add(new GraphAttribute { name = "text", lineage = _metaHandler.CommonLineages["text"], type = GraphAttribute.DataType.textual, confidence = 1.0, id = Guid.NewGuid().ToString(), value = targetText });
                var ruleAtt = new GraphAttribute
                {
                    name = "display",
                    lineage = _metaHandler.CommonLineages["display"],
                    type = GraphAttribute.DataType.ruleset,
                    confidence = 1.0,
                    id = Guid.NewGuid().ToString()
                };
                TargetObj.properties.Add(ruleAtt);
                foreach (var map in dataMaps.Where(a => !a.target))//add nodes
                {
                    var sourceObject = await CreateNode(map, compositeName);
                    //add value and text
                    sourceObject.properties = new List<GraphAttribute>();
                    sourceObject.properties.Add(new GraphAttribute { name = "value", lineage = _metaHandler.CommonLineages["answer"], type = map.dataType, confidence = 1.0, id = Guid.NewGuid().ToString() });
                    sourceObject.properties.Add(new GraphAttribute { name = "text", lineage = _metaHandler.CommonLineages["text"], type = GraphAttribute.DataType.textual, confidence = 1.0, id = Guid.NewGuid().ToString(), value = $"What is the value for {sourceObject.externalId}?" });
                    var link = new GraphConnectionInput { startId = TargetObj.id, endId = sourceObject.id, lineage = _metaHandler.CommonLineages["necessitate"], name = "depends", weight = 1.0 };
                    await _graph.CreateGraphConnection(compositeName, link, OntologyAction.build);
                }
                //now create the initial completion code for the target object.
                var source = _metaHandler.GetBuildInitialRuleSet(model, TargetObj.id, target.objId);
                ruleAtt.value = source;
                var kstates = _graph.LoadData(userId, name, model, data, patternPath, dataMaps, ltype);
                //train the rules for each set choice
                var sets = new List<SetChoices> { SetChoices.three, SetChoices.five, SetChoices.seven, SetChoices.nine };
                var scores = new List<DarlMineReport>();
                foreach (var s in sets)
                {
                    scores.Add(await SupervisedCore(kstates, model, TargetObj.id, _metaHandler.CommonLineages["display"], _metaHandler.CommonLineages["answer"], 90, s, runtime));
                }
                //choose the best set choice and copy the created rules to the completion attribute
                switch (target.dataType)
                {
                    case GraphAttribute.DataType.numeric:
                        //look for lowest ave in/outsample score
                        bestReport = scores.Aggregate((a1, a2) => (a1.trainPerformance + a1.testPerformance) >= (a2.trainPerformance + a2.testPerformance) ? a2 : a1);

                        break;
                    case GraphAttribute.DataType.categorical:
                        //look for highest ave in/outsample score
                        bestReport = scores.Aggregate((a1, a2) => (a1.trainPerformance + a1.testPerformance) >= (a2.trainPerformance + a2.testPerformance) ? a1 : a2);
                        break;
                }
                //remove connections that have little effect on the target
                //move fuzzy sets and categories out to the source nodes
                UpdateCategoriesAndSets(runtime, bestReport.code, model, TargetObj);
                //copy ruleset to the target node.
                ruleAtt.value = bestReport.code;
                //create the conversation nodes.
                CreateRecognitionObjects(TargetObj, model);
                //save the ruleset
                await _graph.Store(compositeName);
            }
            else if (form == LearningForm.unsupervised) //
            {

            }
            return bestReport;

        }


        #region private

        private DarlMetaRunTime GetRuntime(string subjectId)
        {
            //first clearout old runtimes
            foreach (var item in runtimeLifetimes.Keys)
            {
                if (runtimeLifetimes[item] < DateTime.UtcNow - new TimeSpan(0, 30, 0))
                {
                    runtimeLifetimes.TryRemove(item, out DateTime t);
                    runtimes.TryRemove(item, out DarlMetaRunTime c);
                }
            }
            if (runtimes.TryGetValue(subjectId, out DarlMetaRunTime runtime))
                return runtime;
            runtime = new DarlMetaRunTime(_config, _metaHandler);
            runtimeLifetimes.TryAdd(subjectId, DateTime.UtcNow);
            runtimes.TryAdd(subjectId, runtime);
            return runtime;
        }

        private void FindSetBoundaries(int desiredSets, IODefinitionNode inp, List<DarlResult> values)
        {
            List<int> keylist = new List<int>(); //this will hold the sorted indexes of the data values
            bool output = inp is OutputDefinitionNode;
            for (int n = 0; n < values.Count; n++)
                keylist.Add(n);
            keylist.Sort((a, b) =>
            {
                if (values[a].Value == null || double.IsNaN((double)values[a].Value))
                    return 1;
                if (values[b].Value == null || double.IsNaN((double)values[b].Value))
                    return -1;
                return values[a].CompareTo(values[b]);
            }); //sort the indexes, not the values

            int nValues = values.Count;
            if (values.Count > 0 && (values[keylist[0]] == null || double.IsNaN((double)values[keylist[0]].Value)))
                return; //only null data in this input/output
            // Some of these values may be nulls. The sort algorithm stuffs these at the top end with the value NaN
            // decrement nValues until the top value is not NaN.
            while (double.IsNaN((double)values[keylist[nValues - 1]].Value))
            {
                nValues--;
            }
            //check for too few data values
            if (nValues < desiredSets * 2 + 1)
                return; // can't create sets - test for this - shouldn't cause problems.

            //so that we don't need to repeatedly look up sets during learning,
            //the learning source will be appended with a composite int value.
            //The result after dividing by 1000 is the positive slope side of the set that contains the value
            //and the remainder is the degree of truth * 1000.
            //So 3567 is the positive slope of set three value 0.567. This means that set 2 also fires on
            //the negative slope 0.433.
            //this is chosen to be easy to reconstruct in IODefinitionNode.CalculateSetMembership
            List<int> ranks = new List<int>(keylist);
            for (int n = 0; n < values.Count; n++)
            {
                ranks[keylist[n]] = n;
            }

            DarlResult res;
            switch (desiredSets)
            {
                case 3:
                    res = new DarlResult(output ? 2 * (double)values[keylist[0]].Value - (double)values[keylist[(nValues) / 2]].Value : double.NegativeInfinity, (double)values[keylist[0]].Value, (double)values[keylist[nValues / 2]].Value)
                    {
                        leftUnbounded = true,
                        identifier = "small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[nValues - 1]].Value)
                    {
                        identifier = "medium"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], output ? 2 * (double)res.values[2] - (double)res.values[1] : double.PositiveInfinity)
                    {
                        rightUnbounded = true,
                        identifier = "large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    break;
                case 5:
                    res = new DarlResult(output ? 2 * (double)values[keylist[0]].Value - (double)values[keylist[(nValues) / 4]].Value : double.NegativeInfinity, (double)values[keylist[0]].Value, (double)values[keylist[nValues / 4]].Value)
                    {
                        leftUnbounded = true,
                        identifier = "very_small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[nValues / 2]].Value)
                    {
                        identifier = "small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[3 * nValues / 4]].Value)
                    {
                        identifier = "medium"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[nValues - 1]].Value)
                    {
                        identifier = "large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], output ? (2 * (double)res.values[2]) - (double)res.values[1] : double.PositiveInfinity)
                    {
                        rightUnbounded = true,
                        identifier = "very_large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    break;
                case 7:
                    res = new DarlResult(output ? 2 * (double)values[keylist[0]].Value - (double)values[keylist[(nValues) / 6]].Value : double.NegativeInfinity, (double)values[keylist[0]].Value, (double)values[keylist[nValues / 6]].Value)
                    {
                        leftUnbounded = true,
                        identifier = "very_small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[nValues / 3]].Value)
                    {
                        identifier = "small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[nValues / 2]].Value)
                    {
                        identifier = "quite_small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[2 * nValues / 3]].Value)
                    {
                        identifier = "medium"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[(5 * nValues) / 6]].Value)
                    {
                        identifier = "quite_large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[nValues - 1]].Value)
                    {
                        identifier = "large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], output ? 2 * (double)res.values[2] - (double)res.values[1] : double.PositiveInfinity)
                    {
                        rightUnbounded = true,
                        identifier = "very_large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    break;
                case 9:
                    res = new DarlResult(output ? (2 * (double)values[keylist[0]].Value) - (double)values[keylist[(nValues) / 8]].Value : double.NegativeInfinity, (double)values[keylist[0]].Value, (double)values[keylist[(nValues) / 8]].Value)
                    {
                        leftUnbounded = true,
                        identifier = "extremely_small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[nValues / 4]].Value)
                    {
                        identifier = "very_small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[(3 * nValues) / 8]].Value)
                    {
                        identifier = "small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[nValues / 2]].Value)
                    {
                        identifier = "quite_small"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[5 * nValues / 8]].Value)
                    {
                        identifier = "medium"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[3 * nValues / 4]].Value)
                    {
                        identifier = "quite_large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[7 * nValues / 8]].Value)
                    {
                        identifier = "large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], (double)values[keylist[nValues - 1]].Value)
                    {
                        identifier = "very_large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    res = new DarlResult((double)res.values[1], (double)res.values[2], output ? 2 * (double)res.values[2] - (double)res.values[1] : double.PositiveInfinity)
                    {
                        rightUnbounded = true,
                        identifier = "extremely_large"
                    };
                    inp.sets.Add(res.identifier, res);
                    inp.categories.Add(res.identifier);
                    break;
                default:
                    throw new MetaRuleException(string.Format("illegal set choice: {0}", desiredSets));
            }

            for (int n = 0; n < values.Count; n++)
            {
                if (ranks[n] < nValues)
                {
                    int rampNumber = ranks[n] * (desiredSets - 1) / nValues;
                    var result = inp.sets[inp.categories[rampNumber]];
                    int truth = 0;
                    if ((double)result.values[2] != (double)result.values[1])
                        truth = Math.Min(999, Convert.ToInt32(((double)values[n].Value - (double)result.values[1]) * 1000 / ((double)result.values[2] - (double)result.values[1])));
                    inp.learningSource.Add(rampNumber * 1000 + truth);
                }
                else
                    inp.learningSource.Add(-1);
            }
        }


        private List<DataMap> FixDataMapLineages(List<DataMap> dataMaps)
        {
            var list = new List<DataMap>();
            var names = new HashSet<string>();
            foreach (var k in dataMaps)
            {
                var newMap = new DataMap { dataType = k.dataType, target = k.target, attLineage = k.attLineage, objectLineage = k.objectLineage, objectSubLineage = k.objectSubLineage, objId = k.objId, relPath = k.relPath };
                if (names.Contains(k.objId))
                {
                    throw new MetaRuleException($"Duplicate objId {k.objId}");
                }
                names.Add(k.objId);
                if (!string.IsNullOrEmpty(k.attLineage) && !_metaHandler.IsValidLineage(k.attLineage))
                {
                    if (_metaHandler.CommonLineages.TryGetValue(k.attLineage, out var lineage))
                    {
                        newMap.attLineage = lineage;
                    }
                    else
                    {
                        throw new MetaRuleException($"Attribute lineage {k.attLineage} not found in the model under object {k.objId}.");
                    }
                }
                if (!string.IsNullOrEmpty(k.objectLineage) && !_metaHandler.IsValidLineage(k.objectLineage))
                {
                    if (_metaHandler.CommonLineages.TryGetValue(k.objectLineage, out var lineage))
                    {
                        newMap.objectLineage = lineage;
                    }
                    else
                    {
                        throw new MetaRuleException($"Attribute lineage {k.objectLineage} not found in the model under object {k.objId}.");
                    }
                }
                if (!string.IsNullOrEmpty(k.objectSubLineage) && !_metaHandler.IsValidLineage(k.objectSubLineage))
                {
                    if (_metaHandler.CommonLineages.TryGetValue(k.objectSubLineage, out var lineage))
                    {
                        newMap.objectSubLineage = lineage;
                    }
                    else
                    {
                        throw new MetaRuleException($"Attribute lineage {k.objectSubLineage} not found in the model under object {k.objId}.");
                    }
                }
                list.Add(newMap);
            }
            return list;
        }

        private void UpdateCategoriesAndSets(IDarlMetaRunTime runtime, string code, IGraphModel model, GraphObject target)
        {
            var tree = runtime.CreateTree(code, target, model);
            tree.GetInputs().ForEach(x =>
            {
                var matchingNode = model.vertices.Values.FirstOrDefault(a => a.externalId == x.name);
                if (matchingNode != null)
                {
                    var catNode = matchingNode.properties != null ? matchingNode.properties.FirstOrDefault(b => b.lineage == _metaHandler.CommonLineages["answer"]) : null;
                    if (catNode != null)
                    {
                        switch (catNode.type)
                        {
                            case GraphAttribute.DataType.categorical:
                                {
                                    catNode.properties = new List<GraphAttribute>();
                                    foreach (var cat in x.categories)
                                    {
                                        catNode.properties.Add(new GraphAttribute { value = cat, type = GraphAttribute.DataType.textual, name = "category", lineage = _metaHandler.CommonLineages["category"] });
                                    }
                                }
                                break;
                        }
                    }
                }
            });
        }


        private async Task<DarlMineReport> SupervisedCore(List<KnowledgeState> data, IGraphModel model, string target, string targetLineage, string valueLineage, int percentTrain, SetChoices sets, DarlMetaRunTime runtime)
        {
            if (data.Count < minimumData)
                throw new MetaRuleException($"Only {data.Count} training values found. Cannot continue.");
            var ps = await PrepareDataForLearning(data, model, target, targetLineage, valueLineage, percentTrain, (int)sets, runtime);
            var rep = runtime.MineSupervised(ps);
            rep.sets = sets;
            return rep;
        }

        private async Task<GraphObject> CreateNode(DataMap map, string compositeName)
        {
            var TargetObj = new GraphObjectInput { name = map.objId, lineage = map.objectLineage, subLineage = map.objectSubLineage, externalId = map.objId };
            TargetObj.properties.Add(new GraphAttributeInput { name = "value", lineage = _metaHandler.CommonLineages["answer"], type = map.dataType });
            TargetObj.properties.Add(new GraphAttributeInput { name = "text", lineage = _metaHandler.CommonLineages["text"], type = GraphAttribute.DataType.textual, value = $"What is the value of {map.objId}?" });
            return await _graph.CreateGraphObject(compositeName, TargetObj, OntologyAction.build);
        }

        private void CreateRecognitionObjects(GraphObject? target, IGraphModel model)
        {
            if (target == null || string.IsNullOrEmpty(target.lineage))
                return;
            var code = $"output textual response;\n if anything then response will be \"Please answer the following:\";\n output network completed \"{target.externalId}\" complete;\n if anything then completed will be seek(necessitate); ";
            if (target.lineage.Contains('+'))
            {
                var split = model.SplitCompositeLineage(target.lineage);
                var typeWordMain = _metaHandler.GetTypeWord(split.Item1);
                var typeWordSub = _metaHandler.GetTypeWord(split.Item2);
                var obj1 = new GraphObject { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, lineage = split.Item1, name = typeWordMain };
                model.recognitionVertices.Add(obj1.id, obj1);
                var obj2 = new GraphObject { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, lineage = split.Item2, name = typeWordSub };
                model.recognitionVertices.Add(obj2.id, obj2);
                var obj3 = new GraphObject { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, lineage = split.Item2, name = typeWordSub };
                model.recognitionVertices.Add(obj3.id, obj3);
                var obj4 = new GraphObject { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, lineage = split.Item1, name = typeWordMain };
                model.recognitionVertices.Add(obj4.id, obj4);
                var termObj = new GraphObject { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, lineage = "terminus:", name = "#", properties = new List<GraphAttribute> { new GraphAttribute { id = Guid.NewGuid().ToString(), type = GraphAttribute.DataType.ruleset, value = code, lineage = "adjective:8953" } } };
                model.recognitionVertices.Add(termObj.id, termObj);
                var conn = new GraphConnection { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, endId = obj1.id, startId = model.recognitionRoots["default:"].id ?? "", weight = 1.0 };
                model.recognitionVertices[conn.startId].Out.Add(conn);
                model.recognitionVertices[conn.endId].In.Add(conn);
                model.recognitionEdges.Add(conn.id, conn);
                conn = new GraphConnection { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, endId = obj3.id, startId = model.recognitionRoots["default:"].id ?? "", weight = 1.0 };
                model.recognitionVertices[conn.startId].Out.Add(conn);
                model.recognitionVertices[conn.endId].In.Add(conn);
                model.recognitionEdges.Add(conn.id, conn);
                conn = new GraphConnection { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, endId = obj2.id, startId = obj1.id ?? "", weight = 1.0 };
                model.recognitionVertices[conn.startId].Out.Add(conn);
                model.recognitionVertices[conn.endId].In.Add(conn);
                model.recognitionEdges.Add(conn.id, conn);
                conn = new GraphConnection { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, endId = obj4.id, startId = obj3.id ?? "", weight = 1.0 };
                model.recognitionVertices[conn.startId].Out.Add(conn);
                model.recognitionVertices[conn.endId].In.Add(conn);
                model.recognitionEdges.Add(conn.id, conn);
                conn = new GraphConnection { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, endId = termObj.id, startId = obj2.id ?? "", weight = 1.0 };
                model.recognitionVertices[conn.startId].Out.Add(conn);
                model.recognitionVertices[conn.endId].In.Add(conn);
                model.recognitionEdges.Add(conn.id, conn);
                conn = new GraphConnection { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, endId = termObj.id, startId = obj4.id ?? "", weight = 1.0 };
                model.recognitionVertices[conn.startId].Out.Add(conn);
                model.recognitionVertices[conn.endId].In.Add(conn);
                model.recognitionEdges.Add(conn.id, conn);
                model.defaultTarget = target.id;
                model.initialText = $"What is the {typeWordMain} {typeWordSub}?";
            }
            else
            {
                var typeWord = _metaHandler.GetTypeWord(target.lineage);
                var obj = new GraphObject { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, lineage = target.lineage, name = typeWord, properties = new List<GraphAttribute> { new GraphAttribute { id = Guid.NewGuid().ToString(), type = GraphAttribute.DataType.ruleset, value = code, lineage = "adjective:8953" } } };
                model.recognitionVertices.Add(obj.id, obj);
                var conn = new GraphConnection { id = Guid.NewGuid().ToString(), _virtual = true, inferred = false, endId = obj.id, startId = model.recognitionRoots["default:"].id ?? "", weight = 1.0 };
                model.recognitionVertices[conn.startId].Out.Add(conn);
                model.recognitionVertices[conn.endId].In.Add(conn);
                model.recognitionEdges.Add(conn.id, conn);
                model.defaultTarget = target.id;
                model.initialText = $"What is the {typeWord}?";
            }
        }

        private async Task<KnowledgeState> GetKnowledgeState(string userId, string subjectId, string graphName)
        {
            var ks = await _graph.GetKnowledgeState(userId, subjectId, graphName);
            if (ks == null)
            {
                ks = await _graph.CreateKnowledgeState(userId, new KnowledgeStateInput { knowledgeGraphName = graphName, data = new List<StringListGraphAttributeInputPair>(), subjectId = subjectId });
            }
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


        private void GenerateQuestionMessage(List<SalienceRecord> c, List<InteractTestResponse> responses, ParseTree tree, ref DarlVar pending, GraphObject res, (string?,string?) code, List<Meta.DarlResult> list, DarlMetaActivity? dma)
        {
            //add the annotation by reading the annotation result
            var annot = list.Where(a => a.name == annotationSignum).FirstOrDefault();
            var next = (from entry in c orderby entry.salience descending select ((GraphObject)entry.gobj).externalId).FirstOrDefault(); //find most salient
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
            if (nextRes == null)
                nextRes = tree.GetInputs().Where(a => a.name == "response").FirstOrDefault();
            if (!(nextRes is null))
            {
                var activeNodes = new List<string> { res.id! };
                if (code.Item2 != res.id)
                    activeNodes.Add(code.Item2!);
                switch (nextRes.iType)
                {
                    case Meta.InputDefinitionNode.InputTypes.categorical_input:
                        {
                            var cats = new Dictionary<string, double>();
                            foreach (var i in nextRes.categories) cats.Add(i, 1.0);
                            pending = new DarlVar { dataType = DarlVar.DataType.categorical, categories = cats, name = res.id, sequence = new List<List<string>> { new List<string> { nextRes.name } } };
                            responses.Add(new InteractTestResponse { response = new DarlVar { dataType = DarlVar.DataType.categorical, categories = cats, name = questionIdentifier, Value = text }, reference = res.externalId, darl = code.Item1!, activeNodes = activeNodes, codeActivity = dma });
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
                            responses.Add(new InteractTestResponse { response = new DarlVar { dataType = DarlVar.DataType.numeric, name = questionIdentifier, Value = text, values = drange }, reference = res.externalId, darl = code.Item1!, activeNodes = activeNodes, codeActivity = dma });
                        }
                        break;
                    default:
                        {
                            pending = new DarlVar { dataType = DarlVar.DataType.textual, name = res.id, sequence = new List<List<string>> { new List<string> { nextRes.name } } };
                            responses.Add(new InteractTestResponse { response = new DarlVar { dataType = DarlVar.DataType.textual, name = questionIdentifier, Value = text }, reference = res.externalId, darl = code.Item1!, activeNodes = activeNodes, codeActivity = dma });
                        }
                        break;
                }
            }
        }


        private void GenerateInfoMessage(List<InteractTestResponse> responses, List<Meta.DarlResult> list, (string?, string?) code, GraphObject res)
        {
            var annot = list.Where(a => a.name == annotationSignum).FirstOrDefault();
            var text = "Thanks for that information.";
#pragma warning disable CS8604 // Possible null reference argument.
            if (annot.Exists() && !annot.IsUnknown()) //!= is overridden for DarlResult
#pragma warning restore CS8604 // Possible null reference argument.
                text = annot.Value.ToString();
            var activeNodes = new List<string> { res.id! };
            if (code.Item2 != res.id)
                activeNodes.Add(code.Item2!);
            responses.Add(new InteractTestResponse { response = new DarlVar { dataType = DarlVar.DataType.textual, name = questionIdentifier, Value = text ?? String.Empty }, reference = res.externalId, darl = code.Item1!, activeNodes = activeNodes });
        }

        private async Task<(bool, DarlVar?)> EvaluateUIRule(IDarlMetaRunTime runtime, IGraphModel model, GraphAbstraction? res, DarlVar? pending, List<InteractTestResponse> responses, KnowledgeState ks, List<DarlVar> values, List<string> lineages, bool data = false)
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
                var code = _graph.FindControlAttribute(model, o!.id!);
                if (string.IsNullOrEmpty(code.Item1))
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
                var tree = runtime.CreateTree(code.Item1, o, model); //findControlAttribute checks for syntax errors.
                var list = Meta.DarlVarExtensions.Convert(values);
                var dma = await runtime.Evaluate(tree, list, ks);
                //calculate saliences if any outstanding turn into a question
                var saliences = new HashSet<SalienceRecord>();
                var c = runtime.CalculateKGSaliences(saliences, ks, tree);
                if (c.Any())
                {
                    GenerateQuestionMessage(c.ToList(), responses, tree, ref pending, o, code, list, dma);
                    if(responses.Any())
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

        private async Task UpdateNodeStates(IDarlMetaRunTime runtime, KnowledgeState ks, IGraphModel model, List<KeyValuePair<GraphAbstraction, int>> dependencies, List<DarlVar> values, string completionLineage)
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
                    var ruleSource = model.FindControlAttribute(key.id);
                    if (string.IsNullOrEmpty(ruleSource.Item1))
                    {
                        _graph.HandleCodelessCompletion(model, key, ks);
                        continue;
                    }
                    _logger.LogInformation($"Evaluating completion rule on object: {key.externalId ?? key.name}");
                    var tree = runtime.CreateTree(ruleSource.Item1, key, model);
                    if (tree.HasErrors())
                    {
                        _logger.LogInformation($"Errors in completion rule on object: {key.externalId ?? key.name}, code: {ruleSource}");
                        continue;
                    }
                    var list = new List<Thinkbase.Meta.DarlResult>();
                    await runtime.Evaluate(tree, list, ks);
                    _logger.LogInformation($"Completion rule results: {string.Join("; ", list)}");
                }
            }
        }

        private List<GraphAbstraction> FindNext(IGraphModel model, List<KeyValuePair<GraphAbstraction, int>> ordered, KnowledgeState ks, GraphAbstraction? node, List<string> paths, string completedLineage, bool randomiseSaliences = false)
        {
            var list = new List<GraphAbstraction>();
            var rand = new Random();
            if (ordered == null || ks == null || node == null || paths == null || string.IsNullOrEmpty(completedLineage))
            {
                _logger.LogError($"Bad parameters to FindNext: ordered: {ordered is null}, ks: {ks is null}, node: {node is null}, paths: {paths is null}, completedLineage: {string.IsNullOrEmpty(completedLineage)}");
                return list;
            }
            //if target is completed even though other dependencies remain return an empty list
            if (ks.ContainsAttribute(node.Id(model), completedLineage))
                return list;
            //first build dependency list of nodes reachable from the target node
            var saliences = new HashSet<SalienceRecord>();
            //in descending order calculate salience
            //            saliences.Add(node, 1.0);
            var _runtime = new DarlMetaRunTime(_config, _metaHandler);
            foreach (var o in ordered)
            {
                var go = o.Key as GraphObject;
                var code = _graph.FindControlAttribute(model, go?.id ?? "");
                if (!string.IsNullOrEmpty(code.Item1))
                {
                    var tree = _runtime.CreateTree(code.Item1, go, model);
                    _runtime.CalculateKGSaliences(saliences, ks, tree);
                    // add randomise saliences
                }
            }
            var salienceList = saliences.Distinct().OrderByDescending(a => a.salience).ToList();
            var currentSalience = 0.0;
            foreach (var o in salienceList)
            {
                var obj = o.gobj;
                currentSalience = Math.Max(currentSalience, o.salience);
                if (currentSalience != o.salience && list.Count > 0)
                    break;
                if (obj.Out(model).Count > 0 && !IsCodeLess(obj, model))// not leaf
                    continue;
                if (ks.ContainsAttribute(obj.Id(model), completedLineage) || obj.ContainsAttribute(completedLineage))
                    continue;
                list.Add(obj);
            }
            //list is leaf nodes with highest salience
            return list;
        }

        private bool IsCodeLess(GraphAbstraction obj, IGraphModel model)
        {
            //An early implementation allows a leaf node to have subnodes representing categories or sets. 
            //This enables alternate text for categories
            //Support this by recognizing such nodes.
            foreach (var c in obj.Out(model))
            {
                var src = model.vertices[c.endId];
                if (src.lineage != _metaHandler.CommonLineages["category"])
                    return false;
                if (src.Out.Count > 0)
                    return false;
                if (src.properties != null)
                {
                    foreach (var a in src.properties)
                    {
                        if (a.type == GraphAttribute.DataType.ruleset)
                            return false;
                    }
                }
            }
            return true;
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
        private async Task RecursiveDiscovery(IGraphModel model, KnowledgeRecord ks, KnowledgeState state, string startSubjectId, double weight, List<string> lineages, StringBuilder log, FuzzyTime? currentTime, int depth)
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
                log.AppendLine($"{DepthIndicator(depth)}Processing Node: {currentNode.externalId}, {DateTime.UtcNow}.");
                state.data.Add(currentNode.id ?? String.Empty, new List<GraphAttribute> { new GraphAttribute { name = "completed", type = GraphAttribute.DataType.categorical, value = "true", lineage = _metaHandler.CommonLineages["complete"] } });
                await RecursiveDiscovery(model, state, currentNode, weight, lineages, log, currentTime, depth + 1);
                return;
            }
            if (!await CheckCodeCompletion(model, state, currentNode, log, currentTime, depth))
                return;
            if (ks.subjectId == startSubjectId) //copy attributes from ks into state
            {
                if (!state.data.ContainsKey(currentNode.id ?? string.Empty))
                {
                    state.data.Add(currentNode.id ?? string.Empty, new List<GraphAttribute>());
                }
                state.data[currentNode.id ?? string.Empty].AddRange(ks.data[currentNode.id ?? string.Empty]);
            }

            //Search using inferred links in this KS and non-inferred in the model.

            log.AppendLine($"{DepthIndicator(depth)}Processing KnowledgeState: {ks.subjectId}, {DateTime.UtcNow}.");
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
                            await RecursiveDiscovery(model, (KnowledgeRecord)newKs, state, startSubjectId, Math.Min(weight, ids[index].Item2), lineages, log, currentTime, depth + 1);
                        }
                        else if (newKs is GraphObject)
                        {
                            var go = (GraphObject)newKs;
                            state.data.Add(go.id ?? String.Empty, new List<GraphAttribute> { new GraphAttribute { name = "completed", type = GraphAttribute.DataType.categorical, value = "true", lineage = _metaHandler.CommonLineages["complete"] } });
                            await RecursiveDiscovery(model, state, go, Math.Min(weight, ids[index].Item2), lineages, log, currentTime, depth + 1);
                        }
                        index++;
                    }
                }
            }
            else
            {
                _logger.LogInformation($"Broken link: userId {ks.userId}, graphName {ks.knowledgeGraphName}, subjectId {ks.subjectId}");
            }
            //handle real top level connections.
            foreach (var s in connections.Where(a => a.inferred == false))
            {
                var endObject = model.vertices[s.endId];
                if (!CheckForLoop(state, endObject) && Coexists(endObject, ks, model, currentTime))//avoid loops
                {
                    state.data.Add(endObject.id ?? String.Empty, new List<GraphAttribute> { new GraphAttribute { name = "completed", type = GraphAttribute.DataType.categorical, value = "true", lineage = _metaHandler.CommonLineages["complete"] } });
                    await RecursiveDiscovery(model, state, endObject, Math.Min(weight, s.weight), lineages, log, currentTime, depth + 1);
                }
            }


        }

        private async Task<bool> CheckCodeCompletion(IGraphModel model, KnowledgeState ks, GraphObject currentNode, StringBuilder log, FuzzyTime? currentTime, int depth)
        {
            var code = _graph.FindControlAttribute(model, currentNode.id ?? String.Empty);
            if (!string.IsNullOrEmpty(code.Item1)) //no code implies discovery can continue.
            {
                try
                {
                    //evaluate it
                    var _runtime = new DarlMetaRunTime(_config, _metaHandler);
                    var tree = _runtime.CreateTree(code.Item1, currentNode, model); //findControlAttribute checks for syntax errors.
                    var list = new List<Meta.DarlResult>();
                    await _runtime.Evaluate(tree, list, ks, currentTime); //add current time for eval
                                                                          //return false if further movement not possible.
                    var complete = list.FirstOrDefault(a => a.name == "completed");
                    if (!complete.Exists() || (complete.Value as string) == "false" || complete.GetWeight() < 0.1)
                    {
                        log.AppendLine($"{DepthIndicator(depth)}Node not completed. {currentNode.externalId} {DateTime.UtcNow}.");
                        return false; //rules prevent further search
                    }
                }
                catch (Exception ex)
                {
                    log.AppendLine($"Error in evaluating code: \n{code}\nmessage: \n{ex.Message}\n {DateTime.UtcNow}.");
                    return false;
                }
            }
            return true;
        }

        private bool CheckForLoop(KnowledgeState state, GraphObject next)
        {
            if (!string.IsNullOrEmpty(next.id))
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
        private async Task RecursiveDiscovery(IGraphModel model, KnowledgeState state, GraphObject currentNode, double weight, List<string> lineages, StringBuilder log, FuzzyTime? currentTime, int depth)
        {

            if (!await CheckCodeCompletion(model, state, currentNode, log, currentTime, depth))
                return;
            log.AppendLine($"{DepthIndicator(depth)}Processing GraphObject: {currentNode.externalId}, {DateTime.UtcNow}.");
            foreach (var l in currentNode.Out)
            {
                var endObject = model.vertices[l.endId];
                if (!CheckForLoop(state, endObject) && Coexists(endObject, currentNode, model, currentTime))//avoid loops and objects that don't coexist
                {
                    state.data.Add(endObject.id ?? String.Empty, new List<GraphAttribute> { new GraphAttribute { name = "completed", type = GraphAttribute.DataType.categorical, value = "true", lineage = _metaHandler.CommonLineages["complete"] } });
                    await RecursiveDiscovery(model, state, endObject, Math.Min(weight, l.weight), lineages, log, currentTime, depth + 1);
                }
            }
        }


        private bool Coexists(GraphAbstraction endObject, GraphAbstraction currentNode, IGraphModel model, FuzzyTime? currentTime)
        {
            return currentNode.Coexists(endObject, model, currentTime) > 0.0;
        }

        private string DepthIndicator(int depth)
        {
            var di = string.Empty;
            for (int n = 0; n < depth; n++)
            {
                di += "- ";
            }
            return di;
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



        private async Task<PreparedLearningSet> PrepareDataForLearning(List<KnowledgeState> data, IGraphModel model, string target, string targetLineage, string valueLineage, int percentTrain, int sets, IDarlMetaRunTime runtime)
        {
            //find the target in the KG.
            GraphObject? targetObj = null;
            if (model.vertices.ContainsKey(target))
            {
                targetObj = model.vertices[target];
            }
            else
            {
                targetObj = model.vertices.Values.FirstOrDefault(a => a.externalId == target);
            }
            if (targetObj == null)
            {
                throw new MetaRuleException($"Target {target} not present in model");
            }
            if (!targetObj.ContainsAttribute(targetLineage, null))
            {
                throw new MetaRuleException($"Target Attribute {targetLineage} not present in model");
            }
            if (!targetObj.ContainsAttribute(valueLineage, null))
            {
                throw new MetaRuleException($"Value Attribute {valueLineage} not present in model");
            }
            var paths = model.GetLineages(GraphElementType.connection).Select(a => a.lineage).ToList(); //choose all paths
            //find dependencies of the target.
            var dependencies = GetExecutionOrder(model, targetObj, paths);
            if (!dependencies.Any())
                throw new MetaRuleException($"No source data nodes/attributes have been found.");
            //Get the ruleset to process
            var att = targetObj.GetAttribute(targetLineage);
            if (att == null)
                throw new MetaRuleException($"Target attribute {targetLineage} not found");
            if (att.type != GraphAttribute.DataType.ruleset)
            {
                throw new MetaRuleException($"Target attribute {targetLineage} is not a ruleset.");
            }
            var source = att.value;
            ParseTree? tree = null;
            try
            {
                tree = runtime.CreateTree(source, targetObj, model);
            }
            catch (Exception ex)
            {
                throw new MetaRuleException($"Errors in the target rule source. ", ex);
            }
            if (tree == null)
            {
                throw new MetaRuleException($"Empty rule source");
            }

            var ps = new PreparedLearningSet
            {
                sets = sets,
                data = new Dictionary<string, List<DarlResult>>(),
                rroot = tree.Root.AstNode as MetaRootNode ?? new MetaRootNode(),
                patternCount = data.Count,
                ruleset = source,
                targetNode = targetObj,
                model = model,
                knowledgeStates = data,
                targetNodeId = target,
                targetLineage = targetLineage,
                valueLineage = valueLineage,
                percentTrain = percentTrain
            };

            ps.ruleSetContents = ps.rroot.Span;

            //Now perform data analysis
            //use the Evaluate function to reference all the data items.
            OutputDefinitionNode? output = null;
            OutputDefinitionNode? control = null;
            foreach (var ks in data)
            {
                var values = new List<DarlResult>();
                try
                {
                    await runtime.Evaluate(tree, values, ks);
                }
                catch (Exception ex)
                {

                }
                foreach (var v in values)
                {
                    if (!ps.data.ContainsKey(v.name))
                        ps.data.Add(v.name, new List<DarlResult> { v });
                    else
                        ps.data[v.name].Add(v);
                }
                //lineages are only valid after evaluation
                if (output == null)
                    output = ps.rroot.outputs.Select(a => a.Value as Meta.OutputDefinitionNode).Where(b => b != null && b.lineage == valueLineage).FirstOrDefault();
                if (control == null)
                    control = ps.rroot.outputs.Select(a => a.Value as Meta.OutputDefinitionNode).Where(b => b != null && b.lineage == targetLineage).FirstOrDefault();
                if (output != null)
                {
                    if (!ps.data.ContainsKey(output.name))
                    {
                        ps.data.Add(output.name, new List<DarlResult>());
                    }
                    ps.data[output.name].Add(ConvertResult(ks.GetAttribute(target, valueLineage)));
                }
            }
            //output lineages not set 'til evaluation

            ps.outp = output;
            //now process data

            ps.inps = ps.rroot.outputs.Values.Where(i => i != output && i != control).Select(a => a as OutputAsInputDefinitionNode ?? new OutputAsInputDefinitionNode()).ToList<IODefinitionNode>();
            ps.inps.AddRange(ps.rroot.inputs.Values.ToList<IODefinitionNode>());

            var ioDefs = new List<IODefinitionNode>();
            ioDefs.AddRange(ps.rroot.outputs.Values.Where(i => i != control).ToList());
            ioDefs.AddRange(ps.rroot.inputs.Values);

            foreach (var o in ioDefs)
            {
                if (ps.data.ContainsKey(o.name))
                {
                    var values = ps.data[o.name];
                    switch (values[0].dataType)
                    {
                        case DarlResult.DataType.numeric:
                            FindSetBoundaries(sets, o, values);
                            break;
                        case DarlResult.DataType.categorical:
                            HandleCategories(o, values);
                            break;
                    }
                }
            }

            //create an ind set for training and testing
            //Create a random selection of indices to be used when data is loaded
            if (percentTrain < 100)
            {
                Random rand = new Random();
                for (int n = 0; n < ps.patternCount; n++)
                {
                    if (rand.Next(100) < percentTrain)
                    {
                        ps.inSamplePatterns.Add(n);
                    }
                    else
                    {
                        ps.outSamplePatterns.Add(n);
                    }
                }
            }
            else
            {
                for (int n = 0; n < ps.patternCount; n++)
                {
                    ps.inSamplePatterns.Add(n);
                }
            }
            return ps;
        }

        private void HandleCategories(IODefinitionNode o, List<DarlResult> values)
        {
            o.categories.Clear();
            o.catsAsIdentifiers.Clear();
            foreach (DarlResult result in values)
            {
                var val = (result.Value as string);
                if (val == null)
                {
                    o.learningSource.Add(-1);
                    continue;
                }
                if (!o.categories.Contains(val))
                {
                    o.categories.Add(val);//collect all categories
                    o.catsAsIdentifiers.Add(val, false);
                }
                o.learningSource.Add(o.categories.IndexOf(val));
            }
        }


        /// <summary>
        /// Convert a GraphAttribute to a DarlResult
        /// </summary>
        /// <param name="graphAttribute"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        private DarlResult ConvertResult(GraphAttribute? graphAttribute)
        {
            if (graphAttribute == null)
                return new DarlResult();
            switch (graphAttribute.type)
            {
                case GraphAttribute.DataType.numeric:
                    {
                        var res = new DarlResult(DarlResult.DataType.numeric, graphAttribute.confidence);
                        try
                        {
                            res.Value = Convert.ToDouble(graphAttribute.value);
                            res.values.Add(res.Value);
                            res.Normalise(false);
                        }
                        catch
                        {
                            res = new DarlResult(0.0, true);
                        }
                        return res;
                    }
                case GraphAttribute.DataType.categorical:
                    {
                        var res = new DarlResult(DarlResult.DataType.categorical, graphAttribute.confidence);
                        res.Value = graphAttribute.value;
                        if (graphAttribute.properties != null)
                        {
                            foreach (var property in graphAttribute.properties)
                            {
                                res.categories.Add(property.value, 1.0);
                            }
                        }
                        return res;
                    }
                default:
                    return new DarlResult();
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
