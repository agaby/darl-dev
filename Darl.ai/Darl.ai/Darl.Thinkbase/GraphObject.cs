/// <summary>
/// GraphObject.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Lineage;
using DarlCommon;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Darl.Thinkbase
{
    [ProtoContract(AsReferenceDefault = true)]
    public class GraphObject : GraphElement
    {
        public static LineageComparer comp { get; set; } = new LineageComparer(); //single comparer

        public static string defaultLabel = "default:";

        public static double fuzzySimilarityThreshold = 0.7;

        public static string recognizedLineage = "adjective:8953";

        public static string terminatingLabel = "terminus:";


        [ProtoMember(1)]
        public string externalId { get; set; }//optional
        [ProtoMember(2)]
        public List<GraphConnection> Out { get; set; } = new List<GraphConnection>();
        [ProtoMember(3)]
        public List<GraphConnection> In { get; set; } = new List<GraphConnection>();

        internal void Match(IGraphModel model, List<string> tokens, List<DarlVar> values, List<MatchedGraphAttribute> matches, List<DefaultMatchGraphAttribute> defaultMatches, string path, int depth = 0, bool fuzzy = false, double confidence = 1.0)
        {
            comp.lineageMatch = true;
            var children = new SortedList<string, GraphObject>(comp);
            foreach (var c in Out)
            {
                if (model.recognitionVertices.ContainsKey(c.endId))
                {
                    var child = model.recognitionVertices[c.endId];
                    children.Add(child.lineage, child);
                }
            }
            if (properties != null)
            {
                if (properties.Any(a => a.lineage == recognizedLineage))
                {
                    matches.Add(new MatchedGraphAttribute { terminus = properties.First(a => a.lineage == recognizedLineage), path = path, values = values, depth = depth, confidence = confidence });
                }
                else if (properties.Any(a => a.type == GraphAttribute.DataType.markdown))
                {
                    matches.Add(new MatchedGraphAttribute { terminus = properties.First(a => a.type == GraphAttribute.DataType.markdown), path = path, values = values, depth = depth, confidence = confidence });
                }
            }
            if (children.ContainsKey(terminatingLabel))
            {
                var localValues = new List<DarlVar>(values);
                var newpath = IncrementPath(path, children[terminatingLabel].lineage);
                children[terminatingLabel].Match(model, tokens, localValues, matches, defaultMatches, newpath, depth + 1, fuzzy, confidence);
            }

            while (depth < tokens.Count)
            {
                int nextDepth = depth;
                List<LineageRecord> cs = null;
                if (children.ContainsKey(defaultLabel))//pick up any default rules, overwriting those further up the tree.
                {
                    if (children[defaultLabel].properties != null)
                    {
                        if (children[defaultLabel].properties.Any(a => a.lineage == recognizedLineage))
                        {
                            defaultMatches.Add(new DefaultMatchGraphAttribute { Att = children[defaultLabel].properties.First(a => a.lineage == recognizedLineage), Depth = depth, path = IncrementPath(path, defaultLabel) });
                        }
                        else if (children[defaultLabel].properties.Any(a => a.type == GraphAttribute.DataType.markdown))
                        {
                            defaultMatches.Add(new DefaultMatchGraphAttribute { Att = children[defaultLabel].properties.First(a => a.type == GraphAttribute.DataType.markdown), Depth = depth, path = IncrementPath(path, defaultLabel) });
                        }
                    }
                }
                var tok = tokens[nextDepth];
                if (children.ContainsKey(tok)) //handle literal matches
                {
                    var localValues = new List<DarlVar>(values);
                    var newpath = IncrementPath(path, children[tok].lineage);
                    children[tok].Match(model, tokens, localValues, matches, defaultMatches, newpath, nextDepth + 1, fuzzy, confidence);
                }
                else //handle lineage matches and values - note, literal matches win
                {
                    if (fuzzy)
                    {
                        foreach (var k in children.Keys)
                        {
                            var v = children[k];
                            if (v.IsLiteral() || v.IsComposite())
                            {
                                var sim = LineageLibrary.Similarity(tok, k);
                                if (sim >= fuzzySimilarityThreshold && sim < 1.0) //not a perfect match but better than the threshold
                                {
                                    var newpath = IncrementPath(path, v.lineage);
                                    v.Match(model, tokens, new List<DarlVar>(values), matches, defaultMatches, newpath, nextDepth + 1, fuzzy, Math.Min(confidence, sim));
                                }
                            }
                        }
                    }
                    int offset = nextDepth;
                    cs = LineageLibrary.WordRecognizer(tokens, ref offset);
                    foreach (var v in children.Values.Where(a => a.IsValue())) //handle values
                    {
                        int relativeDepth = nextDepth;
                        var newpath = IncrementPath(path, v.lineage);
                        for (int n = nextDepth; n < tokens.Count; n++)
                        {//treat value: as defining any length of text that parses

                            var vf = LineageLibrary.HandleValues(v.lineage, tokens, ref relativeDepth, cs, v.lineage, Math.Min(n - relativeDepth + 1, tokens.Count));
                            if (!vf.unknown)
                            {
                                var localValues = new List<DarlVar>(values);
                                localValues.Add(vf);
                                v.Match(model, tokens, localValues, matches, defaultMatches, newpath, n + 1, fuzzy, confidence); //was relativeDepth
                            }
                        }
                    }
                    foreach (var w in cs)
                    {
                        string lineage = w.lineage;
                        if (children.ContainsKey(lineage)) //handle lineage matches
                        {
                            var localValues = new List<DarlVar>(values);
                            var newpath = IncrementPath(path, children[lineage].lineage); //now contains element matched to.
                            children[lineage].Match(model, tokens, localValues, matches, defaultMatches, newpath, nextDepth + 1, fuzzy, confidence); //Other above calls have nextdepth + 1 
                        }
                    }
                }
                depth++;
            }

        }



        /// <summary>
        /// Find the recognition tree child nodes from any given path
        /// </summary>
        /// <param name="model">The graph model</param>
        /// <param name="tokens">the path elements</param>
        /// <param name="depth">the current location</param>
        /// <remarks>Used for editing</remarks>
        /// <returns>the children</returns>
        public List<GraphObject> Navigate(IGraphModel model, List<string> tokens, int depth = 0)
        {
            comp.lineageMatch = false;
            var children = new SortedList<string, GraphObject>(comp);
            foreach (var c in Out)
            {
                var child = model.recognitionVertices[c.endId];
                children.Add(child.lineage, child);
            }
            if (depth >= tokens.Count)
            {
                return children.Values.ToList();
            }
            var tok = tokens[depth];
            if (children.ContainsKey(tok)) //handle literal matches
            {
                return children[tok].Navigate(model, tokens, depth + 1);
            }
            return new List<GraphObject>();
        }

        /// <summary>
        /// Find the recognition tree graphObject at the end of a path
        /// </summary>
        /// <param name="model">The graph model</param>
        /// <param name="tokens">the path elements</param>
        /// <param name="depth">the current location</param>
        /// <returns>the object</returns>
        public GraphObject Find(IGraphModel model, List<string> tokens, int depth = 0)
        {
            comp.lineageMatch = false;
            var children = new SortedList<string, GraphObject>(comp);
            foreach (var c in Out)
            {
                var child = model.recognitionVertices[c.endId];
                children.Add(child.lineage, child);
            }
            var tok = tokens[depth];
            if (children.ContainsKey(tok)) //handle literal matches
            {
                if (depth + 1 == tokens.Count)
                {
                    return children[tok];
                }
                else
                {
                    return children[tok].Find(model, tokens, depth + 1);
                }
            }
            else
            {
                return null;
            }
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.AppendLine($"id = {id},");
            sb.AppendLine($"externalId = {externalId},");
            sb.AppendLine($"lineage = {lineage},");
            sb.AppendLine($"in count = {In.Count},");
            sb.AppendLine($"out count = {Out.Count},");
            sb.AppendLine("Attributes:");
            if (properties != null)
            {
                foreach (var a in properties)
                {
                    sb.Append(a.ToString());
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Does this object contain an attribute with the given lineage and type?
        /// </summary>
        /// <param name="Lineage">The attribute lineage to look for.</param>
        /// <param name="type">The type to look for. If null any type will do.</param>
        /// <returns></returns>
        public override bool ContainsAttribute(string Lineage, GraphAttribute.DataType? type = GraphAttribute.DataType.ruleset)
        {
            if (properties != null)
                if (type != null)
                    return properties.Any(a => a.lineage == Lineage && a.confidence > 0.0 && a.type != GraphAttribute.DataType.ruleset);
                else
                    return properties.Any(a => a.lineage == Lineage && a.confidence > 0.0);
            return false;
        }

        public string? GetAttributeValue(string lineage)
        {
            if (properties != null)
            {
                var att = properties.FirstOrDefault(a => a.lineage == lineage && a.confidence > 0.0);
                return att != null ? att.value : null;
            }
            return null;
        }

        public GraphAttribute? GetAttribute(string lineage)
        {
            if (properties != null)
            {
                return properties.FirstOrDefault(a => a.lineage == lineage && a.confidence > 0.0);
            }
            return null;
        }

        private string IncrementPath(string path, string postfix)
        {
            return string.IsNullOrEmpty(path) ? postfix : path + "/" + postfix;
        }

        private bool IsLiteral()
        {
            return !(lineage ?? String.Empty).Contains(':');
        }

        private bool IsComposite()
        {
            return (lineage ?? String.Empty).Contains('+');
        }

        private bool IsValue()
        {
            return (lineage ?? String.Empty).Contains("value:");
        }

        public override (GraphObject?, List<GraphConnection>) DeReference(IGraphModel model, List<string>? lineages)
        {
            return (this, Out);
        }


    }
}
