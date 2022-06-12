using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase.Meta;
using DarlCommon;
using DarlLanguage.Processing;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Darl.Thinkbase
{

    /// <summary>
    /// Supply default behavior for KGs in the absence of rulesets
    /// </summary>
    public class MetaStructureHandler : IMetaStructureHandler
    {

        public static readonly string questionIdentifier = "__question";


        private readonly List<LineageRecord> defaultNodeLineages = new List<LineageRecord>
        {
            LineageLibrary.lineages["noun:01,0,2,00,26,4,0"], //appraisal
            LineageLibrary.lineages["noun:00,2,00"], //person
            LineageLibrary.lineages["noun:01,0,0,15,07,02,02"], //category
            LineageLibrary.lineages["noun:01,5,04,3"], //number
            LineageLibrary.lineages["noun:01,1,16,07"], //age
            LineageLibrary.lineages["noun:01,1,16,21,08,2"], //gender
            LineageLibrary.lineages["noun:01,3,14,01,06"], //name
            LineageLibrary.lineages["noun:00,1,01,19,14,1"], //address
            LineageLibrary.lineages["noun:01,1,02"], //time
            LineageLibrary.lineages["noun:01,5,07,00,05,2"],//currency
            LineageLibrary.lineages["noun:01,5,03,3,018"]//life 
        };
        private readonly List<LineageRecord> defaultAttLineages = new List<LineageRecord>
        {
            LineageLibrary.lineages["noun:00,1,00,3,10,09,06"], //display
            LineageLibrary.lineages["adjective:5500"], //complete            
            LineageLibrary.lineages["noun:01,4,05,21,05"], //description
            LineageLibrary.lineages["noun:01,4,04,02,07,01"], //text
            LineageLibrary.lineages["noun:01,4,05,21,19"]//answer
        };
        private readonly List<LineageRecord> defaultConnLineages = new List<LineageRecord>
        {
            LineageLibrary.lineages["verb:019,031"],//consist
            LineageLibrary.lineages["verb:534"],//postdates
            LineageLibrary.lineages["verb:533"],//precedes
            LineageLibrary.lineages["verb:393"],//own
            LineageLibrary.lineages["verb:145"],//necessitate
            LineageLibrary.lineages["verb:021"]//have
        };

        public ConcurrentDictionary<string, string> CommonLineages { get; } = new ConcurrentDictionary<string, string>();

        /// <summary>
        /// preload DarlMetaRuntime with these lineages to include the root set.
        /// </summary>
        public Dictionary<string, LineageDefinitionNode> PreloadLineages { get; } = new Dictionary<string, LineageDefinitionNode>();

        public MetaStructureHandler()
        {
            //assumes typewords don't clash
            foreach (var d in defaultNodeLineages)
            {
                CommonLineages.TryAdd(d.typeWord, d.lineage);
            }
            foreach (var d in defaultAttLineages)
            {
                CommonLineages.TryAdd(d.typeWord, d.lineage);
            }
            foreach (var d in defaultConnLineages)
            {
                CommonLineages.TryAdd(d.typeWord, d.lineage);
            }
            foreach (var c in CommonLineages.Keys)
            {
                PreloadLineages.Add(c, new LineageDefinitionNode { name = c, Value = CommonLineages[c], typeword = c });
            }
        }


        public List<LineageRecord> DefaultNodeLineages { get => defaultNodeLineages; }
        public List<LineageRecord> DefaultAttLineages { get => defaultAttLineages; }
        public List<LineageRecord> DefaultConnLineages { get => defaultConnLineages; }

        public bool IsObjectLineage(string lin)
        {
            return lin.StartsWith("noun:") || lin.StartsWith("proper_noun:");
        }

        public bool IsConnectionLineage(string lin)
        {
            return lin.StartsWith("verb:");
        }

        public bool IsValidLineage(string lin)
        {
            return LineageLibrary.CheckLineage(lin);
        }

        public bool FindMetaDisplayStructure(IGraphModel model, GraphObject res, ref DarlVar pending, List<InteractTestResponse> responses)
        {
            //In the process of evaluation leaf nodes get called first.
            //If this is a leaf node of one of the possible types i.e. categorical, numeric, textual, etc.
            //look to the parent to find possible patterns.
            if (res.Out.Any() || !res.In.Any()) //not a leaf or a root
            { 
                var child = model.vertices[res.Out[0].endId];
                if(!string.IsNullOrEmpty(child.GetAttributeValue(CommonLineages["answer"])))
                {
                    res = child;
                }
                else
                {
                    return pending != null;
                }
            }
            //check if this is a constant node - i.e. one with a preset or remote value.
            if (!string.IsNullOrEmpty(res.GetAttributeValue(CommonLineages["answer"])))
            {
                //in this case the parent node is the true leaf node
                var parent = model.vertices[res.In[0].startId];
                var agg = AggregateChildren(parent, model, CommonLineages["consist"]);
                if (agg.Item2 == null)
                    return false;
                pending = agg.Item1;
                responses.Add(agg.Item2);
                return true;
            }
            //this is a simple leaf node. Ensure it contains text and answer attributes (answer must be empty)
            pending = new DarlVar();//will be the pending value, so needs info to tie up once response is obtained.
            pending.name = res.id;
            pending.sequence = new List<List<string>> { new List<string> { res.externalId } };
            var questionText = GetProperty(res, CommonLineages["text"]);
            if (string.IsNullOrEmpty(questionText))
            {
                questionText = $"_You have not set any question text. Add a text attribute to the {res.externalId} node._";
            }
            var answer = res.GetAttribute(CommonLineages["answer"]);
            if (answer == null)
            {
                questionText += $"\n\n_You have not set an answer attribute on the {res.externalId} node, so I can't infer the data type.\nPlease add one with an empty value_";
                answer = new GraphAttribute { confidence = 1.0, type = GraphAttribute.DataType.textual, value = null };
            }
            pending.dataType = answer.ConvertDataType();
            switch(pending.dataType)
            {
                case DarlVar.DataType.categorical:
                    if(answer.properties != null)
                    {
                        pending.categories = answer.properties.Where(a => a.name == "category").Select(a => a.value).ToDictionary(x => x, y => 1.0);
                    }
                    break;
            }
            responses.Add(new InteractTestResponse { response = new DarlVar { dataType = pending.dataType, categories = pending.categories, values = pending.values, name = questionIdentifier, Value = questionText }, reference = res.externalId, darl = "Inferred from structure", activeNodes = new List<string> { res.id } });
            return true;
        }


        
        public (DarlVar, InteractTestResponse) AggregateChildren(GraphObject go, IGraphModel model, string ConnectionLineage)
        {
            string aggregateLineage = "";
            var nodes = new List<string> { go.id };
            DarlVar pending = new DarlVar();//will be the pending value, so needs info to tie up once response is obtained.
            pending.name = go.id;
            pending.sequence = new List<List<string>> { new List<string> { go.externalId } };
            var questionText = GetProperty(go, CommonLineages["text"]);
            foreach (var o in go.Out.Where(a => a.lineage.StartsWith(ConnectionLineage)))
            {
                var child = model.vertices[o.endId];
                nodes.Add(child.id);
                if (string.IsNullOrEmpty(aggregateLineage))
                {
                    aggregateLineage = child.lineage;
                    if (aggregateLineage.StartsWith(CommonLineages["category"]))
                    {
                        pending.dataType = DarlVar.DataType.categorical;
                    }
                    else if (aggregateLineage.StartsWith(CommonLineages["number"]))
                    {
                        pending.dataType = DarlVar.DataType.numeric;
                    }
                    else if (aggregateLineage.StartsWith(CommonLineages["text"]))
                    {
                        pending.dataType = DarlVar.DataType.textual;
                    }
                    else if (aggregateLineage.StartsWith(CommonLineages["time"]))
                    {
                        pending.dataType = DarlVar.DataType.date;
                    }
                    else
                    {
                        return (new DarlVar { unknown = true, weight = 0.0 }, null);
                    }
                    AggregateValue(pending, child);
                }
                else
                {
                    if (aggregateLineage.StartsWith(child.lineage))
                    {
                        aggregateLineage = child.lineage;
                        AggregateValue(pending, child);
                    }
                    else if (child.lineage.StartsWith(aggregateLineage))
                    {
                        AggregateValue(pending, child);
                    }
                    else //children not homogeneous
                    {
                        return (new DarlVar { unknown = true, weight = 0.0 }, null);
                    }
                }
            }
            var resp = new InteractTestResponse { response = new DarlVar { dataType = pending.dataType, categories = pending.categories, values = pending.values, name = questionIdentifier, Value = questionText }, reference = go.externalId, darl = "Inferred from structure", activeNodes = nodes };
            return (pending, resp);
        }

        private void AggregateValue(DarlVar pending, GraphObject child)
        {
            switch (pending.dataType)
            {
                case DarlVar.DataType.categorical:
                    if (pending.categories == null)
                        pending.categories = new Dictionary<string, double>();
                    var p = GetProperty(child, CommonLineages["text"]);
                    if (p != null)
                    {
                        pending.categories.Add(p, 1.0);
                        var txt = GetProperty(child, CommonLineages["answer"]);
                        if (txt != null)
                        {
                            if (pending.sequence.Count == 1)
                            {
                                pending.sequence.Add(new List<string>());
                            }
                            pending.sequence[1].Add(p); //key
                            pending.sequence[1].Add(txt); //value
                        }
                    }
                    else
                    {
                        pending.categories.Add(child.name, 1.0);
                    }
                    break;
                case DarlVar.DataType.numeric:
                    if (pending.values == null)
                        pending.values = new List<double>();
                    pending.values.Sort();
                    var n = GetProperty(child, CommonLineages["number"]);
                    if (n != null)
                    {
                        pending.values.Add(double.Parse(n));
                        pending.values.Sort();
                    }
                    break;
                case DarlVar.DataType.date:
                    break;
                default:
                    var t = GetProperty(child, CommonLineages["text"]);
                    if (t != null)
                        pending.Value = t;
                    break;
            }
        }

        private string GetProperty(GraphObject child, string lineage)
        {
            if (child.properties == null)
                return null;
            var p = child.properties.Where(a => a.lineage.StartsWith(lineage)).FirstOrDefault();
            if (p == null)
                return null;
            return p.value;
        }

        /// <summary>
        /// Perform the same processing as a standardized ruleset
        /// </summary>
        /// <param name="model"></param>
        /// <param name="res"></param>
        /// <param name="pending"></param>
        /// <param name="values"></param>
        /// <param name="ks"></param>
        public void HandleCodelessValue(IGraphModel model, GraphObject res, DarlVar pending, List<DarlVar> values, KnowledgeState ks)
        {
            var response = values.Where(a => a.name == res.externalId).FirstOrDefault();
            if (response == null)
                throw new RuleException($"Codeless value {res.name} not found in returned values");
            switch (pending.dataType)
            {
                case DarlVar.DataType.categorical:
                    //check values is a valid category
                    if (!pending.categories.ContainsKey(response.Value))
                    {
                        throw new RuleException($"Category {response.Value} of {res.name} not a valid category");
                    }
                    //translate category values here.
                    var cat = response.Value;
                    if (pending.sequence.Count == 2) //there's a cat translation table in the sequence
                    {
                        for (int n = 0; n < pending.sequence[1].Count; n++)
                        {
                            if (pending.sequence[1][n] == cat)
                            {
                                cat = pending.sequence[1][n + 1];
                                break;
                            }
                            n++; //advance in pairs
                        }
                    }
                    //set answer att with value in ks
                    ks.AddAttribute(res.id, new GraphAttribute { id = Guid.NewGuid().ToString(), confidence = 1.0, lineage = CommonLineages["answer"], value = cat, name = "answer", type = GraphAttribute.DataType.categorical });
                    //mark children as completed
                    break;
                case DarlVar.DataType.numeric:
                    //check values is a valid number
                    //set answer att with value in ks
                    ks.AddAttribute(res.id, new GraphAttribute { id = Guid.NewGuid().ToString(), confidence = 1.0, lineage = CommonLineages["answer"], value = response.Value, name = "answer", type = GraphAttribute.DataType.numeric });
                    break;
            }
            foreach (var i in res.Out)
            {
                var o = model.vertices[i.endId];
                var att = new GraphAttribute { id = Guid.NewGuid().ToString(), lineage = CommonLineages["complete"], name = "completed", type = GraphAttribute.DataType.categorical, value = "true", confidence = 1.0 };
                ks.AddAttribute(o.id, att);
            }
        }

        /// <summary>
        /// When no completion rule is defined on a non-leaf node do this
        /// </summary>
        /// <param name="model"></param>
        /// <param name="res"></param>
        /// <param name="ks"></param>
        public void HandleCodelessCompletion(IGraphModel model, GraphObject res, KnowledgeState ks)
        {
            var completed = true;
            foreach (var i in res.Out)
            {
                var o = model.vertices[i.endId];
                if (!ks.ContainsAttribute(o.id, CommonLineages["complete"]))
                {
                    completed = false;
                    break;
                }
            }
            if (completed)
            {
                var att = new GraphAttribute { id = Guid.NewGuid().ToString(), lineage = CommonLineages["complete"], name = "completed", type = GraphAttribute.DataType.categorical, value = "true", confidence = 1.0 };
                ks.AddAttribute(res.id, att);
            }
        }

        public List<(string, string)> CreateCompletionRuleFirstPass(IGraphModel model, GraphObject res)
        {
            var set = new HashSet<(string, string)>();
            if (model.vertices.ContainsKey(res.id))
            {
                foreach (var c in res.Out)
                {
                    var child = model.vertices[c.endId];
                    set.Add((c.lineage, child.lineage));
                }
            }
            else if (model.virtualVertices.ContainsKey(res.lineage))
            {
                //Find all objects of this type 
                foreach (var o in model.vertices.Where(a => a.Value.lineage.StartsWith(res.lineage)))
                {
                    foreach (var c in o.Value.Out)
                    {
                        var child = model.vertices[c.endId];
                        set.Add((c.lineage, child.lineage));
                    }
                }
            }
            return set.ToList();
        }

        public string CreateCompletionRuleSecondPass(IGraphModel model, GraphObject res, List<(string, string, string)> paths, string op)
        {
            // paths are link lineage, object lineage, all or any
            // create ruleset 
            var constants = new Dictionary<string, string>();
            constants.Add(CommonLineages["complete"], "complete");
            var fragments = new List<string>();
            foreach (var c in paths)
            {
                if (!constants.ContainsKey(c.Item1))
                {
                    constants.Add(c.Item1, LineageLibrary.lineages[c.Item1].typeWord);
                }
                if (!constants.ContainsKey(c.Item2))
                {
                    constants.Add(c.Item2, LineageLibrary.lineages[c.Item2].typeWord);
                }
                fragments.Add($"{c.Item3}({constants[c.Item2]}, {constants[c.Item1]}, complete) ");
            }
            var sb = new StringBuilder();
            foreach (var con in constants)
            {
                if (!PreloadLineages.ContainsKey(con.Value))
                {
                    sb.AppendLine($"lineage {con.Value} \"{con.Key}\";");
                }
            }
            sb.AppendLine($"output categorical completed {{true,false}} complete;");
            sb.Append("if ");
            if (fragments.Any())
            {
                sb.Append(string.Join($" {op} ", fragments));
            }
            else
            {
                sb.Append("anything");
            }
            sb.AppendLine(" then completed will be true;");
            return sb.ToString();
        }

        public string GetBuildInitialRuleSet(IGraphModel model, string objectId, string target)
        {
            var outsb = new StringBuilder();
            var rulesb = new StringBuilder();
            if (model != null)
            {
                if (model.vertices.ContainsKey(objectId))
                {
                    outsb.AppendLine("output categorical completed {true, false} complete;");
                    outsb.AppendLine("output textual reportSource;");
                    outsb.AppendLine("output textual annotation;");   
                    var obj = model.vertices[objectId];
                    var targetAtt = obj.GetAttribute(CommonLineages["answer"]);
                    if (targetAtt != null)
                    {
                        outsb.AppendLine($"output {targetAtt.type} {target} answer;");
                        rulesb.AppendLine($"if {target} is present then completed will be true;");
                        rulesb.AppendLine("if anything then reportSource will be attribute(text);");
                        rulesb.AppendLine($"if {target} is present then annotation will be document(reportSource,{{{target}}});");
                    }
                    foreach (var c in obj.Out)
                    {
                        var source = model.vertices[c.endId];
                        var att = source.GetAttribute(CommonLineages["answer"]);
                        if (att != null)
                        {
                            var outname = BuildName(source);
                            if (outname != null)
                            {
                                outsb.AppendLine($"input {att.type} {outname} \"{outname}\" answer;");
                            }
                        }
                    }                    
                }
            }
            return outsb.ToString() + "\n\n" + rulesb.ToString();
        }

        public string GetSuggestedRuleSet(IGraphModel model, string objectId, string lineage)
        {
            if (model != null)
            {
                if (model.vertices.ContainsKey(objectId))
                {
                    var obj = model.vertices[objectId];
                    var outsb = new StringBuilder();
                    var rulesb = new StringBuilder();
                    foreach (var c in obj.Out)
                    {
                        var source = model.vertices[c.endId];
                        var att = source.GetAttribute(CommonLineages["answer"]);
                        if (att != null)
                        {
                            var outname = BuildName(source);
                            if (outname != null)
                            { 
                                outsb.AppendLine($"output {att.type} {outname};");
                                rulesb.AppendLine($"if anything then {outname} will be single({BuildTypeWordString(source, model)},{GetConnectionTypeWord(c)},answer);");
                            }
                        }
                    }
                    return outsb.ToString() + "\n\n" + rulesb.ToString();
                }
                else if (model.recognitionVertices.ContainsKey(objectId))
                {
                    var obj = model.recognitionVertices[objectId];
                    var sb = new StringBuilder();
                    var att = obj.GetAttribute(CommonLineages["text"]);
                    sb.AppendLine("output textual response;");
                    sb.AppendLine("output network completed \"<externalId of seek destination>\" complete;");
                    sb.AppendLine("");
                    if (att != null) //node text in attribute
                    {
                        sb.AppendLine("if anything then response will be attribute(text);");
                    }
                    else
                    {
                        sb.AppendLine("if anything then response will be \"Put your introductory text here.\";");
                    }
                    sb.AppendLine("if anything then completed will be seek(<connection lineage>);");
                    return sb.ToString();
                }
            }
            return string.Empty;

        }
        private string BuildName(GraphObject obj)
        {
            if (!string.IsNullOrEmpty(obj.name))
            {
                return obj.name.Trim().Replace(' ', '_');
            }
            else if (!string.IsNullOrEmpty(obj.externalId))
            {
                return obj.externalId.Trim().Replace(' ', '_');
            }
            return string.Empty;
        }

        private string BuildTypeWordString(GraphObject obj, IGraphModel model)
        {
            var lins = model.SplitCompositeLineage(obj.lineage);
            if (lins.Item2 == null)
                return GetTypeWord(lins.Item1);
            return $"{GetTypeWord(lins.Item1)}+{GetTypeWord(lins.Item2)}";
        }

        private string GetConnectionTypeWord(GraphConnection c)
        {
            return GetTypeWord(c.lineage);
        }

        public string GetTypeWord(string? lineage)
        {
            if(lineage != null && LineageLibrary.lineages.TryGetValue(lineage, out LineageRecord rec))
            {
                return rec.typeWord;
            }
            return "";
        }

    }
}
