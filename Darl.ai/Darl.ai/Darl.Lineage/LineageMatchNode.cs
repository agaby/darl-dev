/// <summary>
/// </summary>

﻿using DarlCommon;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Darl.Lineage
{
    /// <summary>
    /// Alternative implementation that uses much less space.
    /// </summary>
    [ProtoContract()]
    public class LineageMatchNode
    {
        public static LineageComparer comp { get; set; } = new LineageComparer(); //single comparer

        public static double fuzzySimilarityThreshold = 0.7;

        [ProtoMember(1)]
        public LineageElement element { get; set; }

        [ProtoMember(2)]
        public SortedList<string, LineageMatchNode> children { get; set; } = new SortedList<string, LineageMatchNode>(comp);

        /// <summary>
        /// all the pieces of darl code and implications associated with the sequence
        /// </summary>
        [ProtoMember(3)]
        public LineageAnnotationNode annotation { get; set; }

        /// <summary>
        /// Build the execution tree from the edit tree.
        /// This has two jobs, to expand each child where more than one text or lineage is present, and to collect LineageAnnotationNodes
        /// The resulting graph is minimized for execution and is a DAG, but will in general not be a tree.
        /// </summary>
        /// <param name="executionRoot"></param>
        /// <param name="hs"></param>
        /// <returns>A new sub-graph set up for execution</returns>
        /// <remarks>As written will fail with an exception if | separated tags match existing tags in the same node of the tree. Need to handle this</remarks>
        public LineageMatchNode CreateExecutionGraph(HashSet<LineageAnnotationNode> hs)
        {
            var executionNode = new LineageMatchNode();
            executionNode.element = element;
            if (hs.Contains(annotation))
            {
                executionNode.annotation = hs.First(a => a == annotation);
            }
            else
            {
                executionNode.annotation = annotation;
                hs.Add(annotation);
            }
            foreach (var c in children.Keys)
            {
                var lmn = children[c];
                if (c.Contains('|'))
                {
                    //evaluate the sub-children once
                    var subChildren = new SortedList<string, LineageMatchNode>(comp);
                    foreach (var cc in lmn.children.Keys)
                    {
                        var slmn = lmn.children[cc];
                        subChildren.Add(cc, slmn.CreateExecutionGraph(hs));
                    }
                    foreach (var s in c.Split('|'))
                    {
                        if (!executionNode.children.ContainsKey(s.Trim())) //establishes priority of independent over composite
                            executionNode.children.Add(s.Trim(), new LineageMatchNode { element = CreateFromText(c), children = subChildren, annotation = lmn.annotation });//was createfrom text(s)
                        else
                        {
                            var node = executionNode.children[s.Trim()];
                            if (node.annotation == null && lmn.annotation != null) //preserve annotations if present in the duplicate
                                node.annotation = lmn.annotation;
                            /*                            foreach(var sck in subChildren.Keys)
                                                        {
                                                            if (!node.children.ContainsKey(sck))
                                                                node.children.Add(sck, subChildren[sck]);
                                                        }*/
                        }
                    }
                }
                else
                {
                    if (!executionNode.children.ContainsKey(c)) //establishes priority of independent over composite
                        executionNode.children.Add(c, lmn.CreateExecutionGraph(hs));
                    else
                    {
                        var node = executionNode.children[c];
                        if (node.annotation == null && lmn.annotation != null) //preserve annotations if present in the duplicate
                            node.annotation = lmn.annotation;
                    }
                }
            }
            return executionNode;
        }

        public static string defaultLabel = "default:";

        public LineageMatchNode()
        {

        }

        internal void Create(LineageTemplateSet lts)
        {
            foreach (var lt in lts.templates)
            {
                RecursivelyCreate(lt, lts.payload);
            }
        }

        internal List<LineageMatchNode> Navigate(List<string> tokens)
        {
            return RecursivelyNavigate(tokens);
        }

        internal List<LineageMatchNode> RecursivelyNavigate(List<string> tokens, int depth = 0)
        {
            if (depth >= tokens.Count)
            {
                return children.Values.ToList();
            }
            var tok = tokens[depth];
            if (children.ContainsKey(tok)) //handle literal matches
            {
                return children[tok].RecursivelyNavigate(tokens, depth + 1);
            }
            return new List<LineageMatchNode>();
        }

        internal void Rationalize(StringBuilder sb)
        {
            foreach (var elem in children.Values)
            {
                if (elem.element.lineage.Contains('|'))
                {
                    elem.element.type = LineageType.composite;
                }
            }
            if (children.ContainsKey("New node")) //clean up bug
            {
                var rep = children["New node"];
                children.Remove("New node");
                children.Add("new node", rep);
            }
            var lineages = children.Values.Where(a => a.element.type == LineageType.concept);
            var literals = children.Values.Where(a => a.element.type == LineageType.literal);
            var literalConceptDictionary = new Dictionary<string, List<LineageRecord>>();
            var merges = new List<MergeSet>();
            foreach (var lit in literals) //build this to avoid concept lookups in the inner loop
            {
                var offset = 0;
                literalConceptDictionary.Add(lit.element.lineage, LineageLibrary.WordRecognizer(new List<string> { lit.element.lineage }, ref offset, true));
            }
            foreach (var lin in lineages)
            {
                foreach (var lit in literals)
                {
                    foreach (var concept in literalConceptDictionary[lit.element.lineage])
                    {
                        if (concept.lineage.StartsWith(lin.element.lineage))
                        {
                            //merge
                            merges.Add(new MergeSet { major = lin, minor = lit });
                            break;
                        }
                    }
                }
            }
            foreach (var merge in merges)
            {
                Merge(merge);
                children.Remove(merge.minor.element.lineage);
            }
            foreach (var terminal in children.Values.Where(a => a.element.type == LineageType.Default || a.element.type == LineageType.value))
            {
                terminal.children.Clear();
            }
            foreach (var child in children.Values)
            {
                child.Rationalize(sb);
            }
        }

        private static void Merge(MergeSet merge)
        {
            foreach (var child in merge.minor.children.Keys)
            {
                if (merge.major.children.ContainsKey(child))
                {
                    Merge(new MergeSet { major = merge.major.children[child], minor = merge.minor.children[child] });
                }
                else
                {
                    merge.major.children.Add(child, merge.minor.children[child]);
                }
            }
            if (merge.major.annotation != null && merge.minor.annotation != null)
            {
                merge.major.annotation.darl.AddRange(merge.minor.annotation.darl);
                merge.major.annotation.implications.AddRange(merge.minor.annotation.implications);
            }
        }

        /// <summary>
        /// Find the sequence of nodes for a path
        /// </summary>
        /// <param name="tokens">elements of the path</param>
        /// <param name="list">the list</param>
        /// <param name="depth">current depth</param>
        internal void FindSequence(List<string> tokens, List<LineageMatchNode> list, int depth = 0)
        {
            var tok = tokens[depth];
            if (children.ContainsKey(tok)) //handle literal matches
            {
                list.Add(children[tok]);
                if (depth + 1 != tokens.Count)
                {
                    children[tok].Find(tokens, depth + 1);
                }
            }
        }

        internal void AddDescriptions()
        {
            if (element != null && string.IsNullOrEmpty(element.description))
            {

                switch (element.type)
                {
                    case LineageType.literal:
                        element.description = $"Literal: {element.lineage}";
                        break;
                    case LineageType.Default:
                        element.description = "Default response";
                        break;
                    case LineageType.value:
                        var text = LineageLibrary.lineages.ContainsKey(element.lineage) ? LineageLibrary.lineages[element.lineage].description : "unknown - check";
                        element.description = $"Value: {text}";
                        break;
                    case LineageType.concept:
                        var text2 = LineageLibrary.lineages.ContainsKey(element.lineage) ? LineageLibrary.lineages[element.lineage].description : "unknown - check";
                        element.description = $"Lineage: {text2}";
                        break;
                    case LineageType.composite:
                        {
                            var sb = new StringBuilder();
                            sb.Append("Composite: ");
                            foreach (var s in element.lineage.Split('|'))
                            {
                                var lin = element.lineage.Trim();
                                sb.Append(LineageLibrary.lineages.ContainsKey(lin) ? LineageLibrary.lineages[lin].description : $"Literal: {lin}");
                            }
                            element.description = sb.ToString();
                        }
                        break;
                }
            }
            foreach (var child in children.Values)
                child.AddDescriptions();
        }

        /// <summary>
        /// Add a new node to the tree recursively
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="newName"></param>
        /// <param name="depth"></param>
        /// <returns>The newly added node</returns>
        internal LineageMatchNode Add(List<string> tokens, string newName, int depth = 0)
        {
            if (depth == tokens.Count || (tokens.Count == 1 && depth == 0 && tokens[0] == "#"))//# indicates at root.
            {
                var nn = new LineageMatchNode() { element = CreateFromText(newName) };
                if (children.ContainsKey(newName)) //replace
                {
                    var old = children[newName];
                    LineageMatchNode.comp.multiMatch = false;
                    if (children[old.element.lineage].annotation != null)
                        nn.annotation = children[old.element.lineage].annotation;
                    children.Remove(old.element.lineage);
                    children.Add(newName, nn);
                }
                else
                {
                    children.Add(newName, nn);
                }
                return nn;
            }
            var tok = tokens[depth];
            if (children.ContainsKey(tok)) //handle literal matches
            {
                return children[tok].Add(tokens, newName, depth + 1);
            }
            return null;
        }

        private LineageElement CreateFromText(string text)
        {
            text = text.Trim().ToLower();
            if (text.Contains(":") && LineageLibrary.lineages.ContainsKey(text))
            {
                return LineageLibrary.lineages[text];
            }
            else if (text.Contains("|"))
            {
                return new LineageElement() { lineage = text, type = LineageType.composite };
            }
            return new LineageElement() { lineage = text, type = LineageType.literal };
        }

        /// <summary>
        /// copy copyroot and it's children onto the end of the tokens path.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="copyRoot"></param>
        internal void Paste(List<string> tokens, LineageMatchNode copyRoot, int depth = 0)
        {
            if (depth == tokens.Count)
            {
                children.Add(copyRoot.element.lineage, copyRoot);
                return;
            }
            var tok = tokens[depth];
            if (children.ContainsKey(tok)) //handle literal matches
            {
                children[tok].Paste(tokens, copyRoot, depth + 1);
            }
        }

        internal void Rename(List<string> tokens, string newName, int depth = 0)
        {
            var tok = tokens[depth];
            if (children.ContainsKey(tok)) //handle literal matches
            {
                if (depth + 1 == tokens.Count)
                {
                    var node = children[tok];
                    if (newName.Contains(":")) //handle symbols
                    {
                        if (newName.Contains("|")) //composite including symbol
                        {
                            foreach (var c in newName.Split('|'))
                            {
                                if (c.Contains(":"))
                                    HandleLineage(c, node);
                            }
                        }
                        else
                        {
                            HandleLineage(newName, node);
                        }
                    }
                    else
                    {
                        node.element.lineage = newName;
                        node.element.type = LineageType.literal;
                    }
                    children.Remove(tok);
                    children.Add(newName, node);
                }
                else
                {
                    children[tok].Rename(tokens, newName, depth + 1);
                }
            }
        }

        /// <summary>
        /// check validity of special symbols in a rename
        /// </summary>
        /// <param name="newName"></param>
        /// <param name="node"></param>
        private void HandleLineage(string newName, LineageMatchNode node)
        {
            string label;
            var parsedName = ParseValue(newName, out label);
            if (LineageLibrary.lineages.ContainsKey(parsedName))
            {
                node.element = LineageLibrary.lineages[parsedName];
                if (!string.IsNullOrEmpty(label))
                {
                    if (node.annotation == null)
                        node.annotation = new LineageAnnotationNode();
                    node.annotation.implications.Add(label);
                }
            }
            else
            {
                throw new Exception($"invalid symbol: {parsedName}");
            }
        }

        /// <summary>
        /// looks for specific label in value
        /// </summary>
        /// <param name="name"></param>
        /// <param name="label"></param>
        /// <returns></returns>
        private string ParseValue(string name, out string label)
        {
            if (name.Contains("("))
            {
                int labelStart = name.IndexOf('(');
                int labelEnd = name.IndexOf(')');
                label = name.Substring(labelStart + 1, labelEnd == -1 ? name.Length - labelStart - 1 : labelEnd - labelStart - 1);
                return name.Substring(0, labelStart);
            }
            label = "";
            return name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="stack"></param>
        /// <param name="results"></param>
        /// <param name="depth"></param>
        internal void FindSearchPaths(List<string> tokens, Stack<string> stack, List<SearchCandidate> results, int depth = 0)
        {
            StackToPath(stack, results, depth, tokens);
            if (depth >= tokens.Count)
            {
                return;
            }
            int nextDepth = depth;
            List<LineageRecord> cs = null;
            if (element != null && element.type == LineageType.value)
            {
                cs = LineageLibrary.WordRecognizer(tokens, ref nextDepth);
            }
            var tok = tokens[nextDepth];
            if (children.ContainsKey(tok)) //handle literal matches
            {
                stack.Push(children[tok].element.lineage);
                children[tok].FindSearchPaths(tokens, stack, results, nextDepth + 1);
                stack.Pop();
            }
            else //handle lineage matches and values  - note, literal matches win
            {
                if (cs == null)
                    cs = LineageLibrary.WordRecognizer(tokens, ref nextDepth);
                foreach (var w in cs)
                {
                    if (children.ContainsKey(w.lineage)) //handle lineage matches
                    {
                        stack.Push(children[w.lineage].element.lineage);
                        children[w.lineage].FindSearchPaths(tokens, stack, results, nextDepth);
                        stack.Pop();
                    }
                }
                //now look for value match
                foreach (var v in children.Values)
                {
                    if (v.element.type == LineageType.value)
                    {
                        var vf = LineageLibrary.HandleValues(v.element.lineage, tokens, ref nextDepth, cs);
                        if (!vf.unknown) //value found
                        {
                            stack.Push(v.element.lineage);
                            children[v.element.lineage].FindSearchPaths(tokens, stack, results, nextDepth);
                            stack.Pop();
                        }
                    }
                }
            }
        }

        private void StackToPath(Stack<string> stack, List<SearchCandidate> results, int depth, List<string> tokens)
        {
            int index = 0;
            String res = "";
            var st = stack.ToList();
            st.Reverse();
            foreach (var s in st)
            {
                if (index++ == 0)
                {
                    res = s;
                }
                else
                {
                    res += string.IsNullOrEmpty(res) ? s : "/" + s;
                }
            }
            var cand = new SearchCandidate { depth = depth, fullpath = res, path = res };
            for (int n = depth; n < tokens.Count; n++)
            {
                cand.fullpath += n == 0 ? "" : "/";
                if (int.TryParse(tokens[n], out int p))
                {
                    cand.fullpath += "value:number,integer";
                }
                else if (double.TryParse(tokens[n], out double q))
                {
                    cand.fullpath += "value:number,float";
                }
                else
                    cand.fullpath += tokens[n];
            }
            results.Add(cand);
        }

        internal void Delete(List<string> tokens, int depth = 0)
        {

            var tok = tokens[depth];
            if (children.ContainsKey(tok)) //handle literal matches
            {
                if (depth + 1 == tokens.Count)
                {
                    children.Remove(tok);
                }
                else
                {
                    children[tok].Delete(tokens, depth + 1);
                }
            }
        }

        internal LineageMatchNode Find(List<string> tokens, int depth = 0)
        {
            var tok = tokens[depth];
            if (children.ContainsKey(tok)) //handle literal matches
            {
                if (depth + 1 == tokens.Count)
                {
                    return children[tok];
                }
                else
                {
                    return children[tok].Find(tokens, depth + 1);
                }
            }
            else
            {
                return null;
            }

        }

        internal void Match(List<string> tokens, List<DarlVar> values, List<MatchedAnnotation> matches, List<DefaultAnnotation> defaultMatches, string path, int depth = 0, bool fuzzy = false, double confidence = 1.0)
        {
            if (annotation != null)
                matches.Add(new MatchedAnnotation { annotation = annotation, path = path, values = values, depth = depth, confidence = confidence });
            if (depth >= tokens.Count)
            {
                return;
            }
            int nextDepth = depth;
            List<LineageRecord> cs = null;
            if (children.ContainsKey(defaultLabel))//pick up any default rules, overwriting those further up the tree.
            {
                if (children[defaultLabel].annotation != null)
                {
                    defaultMatches.Add(new DefaultAnnotation { Node = children[defaultLabel].annotation, Depth = depth, path = IncrementPath(path, defaultLabel) });
                }
            }
            var tok = tokens[nextDepth];
            if (children.ContainsKey(tok)) //handle literal matches
            {
                var localValues = new List<DarlVar>(values);
                var newpath = IncrementPath(path, children[tok].element.lineage);
                children[tok].Match(tokens, localValues, matches, defaultMatches, newpath, nextDepth + 1, fuzzy, confidence);
            }
            else //handle lineage matches and values - note, literal matches win
            {
                if (fuzzy)
                {
                    foreach (var k in children.Keys)
                    {
                        var v = children[k];
                        if (v.element.type == LineageType.literal || v.element.type == LineageType.composite)
                        {
                            var sim = LineageLibrary.Similarity(tok, k);
                            if (sim >= fuzzySimilarityThreshold && sim < 1.0) //not a perfect match but better than the threshold
                            {
                                var newpath = IncrementPath(path, v.element.lineage);
                                v.Match(tokens, new List<DarlVar>(values), matches, defaultMatches, newpath, nextDepth + 1, fuzzy, Math.Min(confidence, sim));
                            }
                        }
                    }
                }
                int offset = nextDepth;
                cs = LineageLibrary.WordRecognizer(tokens, ref offset);
                foreach (var v in children.Values.Where(a => a.element.type == LineageType.value)) //handle values
                {
                    int relativeDepth = nextDepth;
                    var newpath = IncrementPath(path, v.element.lineage);
                    for (int n = nextDepth; n < tokens.Count; n++)
                    {//treat value: as defining any length of text that parses

                        var vf = LineageLibrary.HandleValues(v.element.lineage, tokens, ref relativeDepth, cs, v.element.lineage, Math.Min(n - relativeDepth + 1, tokens.Count));
                        if (!vf.unknown)
                        {
                            var localValues = new List<DarlVar>(values);
                            localValues.Add(vf);
                            v.Match(tokens, localValues, matches, defaultMatches, newpath, n + 1, fuzzy, confidence); //was relativeDepth
                        }
                    }
                }
                foreach (var w in cs)
                {
                    string lineage = w.lineage;
                    if (children.ContainsKey(lineage)) //handle lineage matches
                    {
                        var localValues = new List<DarlVar>(values);
                        var newpath = IncrementPath(path, children[lineage].element.lineage); //now contains element matched to.
                        children[lineage].Match(tokens, localValues, matches, defaultMatches, newpath, nextDepth + 1, fuzzy, confidence); //Other above calls have nextdepth + 1 
                    }
                }
            }
        }
        //strip parent. when no parents left return string.empty.
        private string GetLineageParent(string lineage)
        {
            var pos = lineage.LastIndexOf(',');
            if (pos == -1) return string.Empty;
            return lineage.Substring(0, pos);
        }

        private string IncrementPath(string path, string postfix)
        {
            return string.IsNullOrEmpty(path) ? postfix : path + "/" + postfix;
        }

        internal void ReadTree(StringBuilder sb)
        {
            if (element != null)
                sb.Append(element.lineage + " ");
            if (children.Count == 0)
                sb.AppendLine("");
            foreach (var c in children.Values)
            {
                c.ReadTree(sb);
            }

        }

        internal void RecursivelyCreate(LineageTemplate lt, string payload, int depth = 0)
        {
            if (depth >= lt.sequence.Count)
            {
                if (!string.IsNullOrEmpty(payload))
                {
                    if (annotation == null)
                        annotation = new LineageAnnotationNode();
                    this.annotation.darl.Add(payload);
                }
                return;
            }
            foreach (var t in lt.sequence[depth])
            {
                LineageMatchNode newNode;
                if (children.ContainsKey(t.lineage))
                    newNode = children[t.lineage];
                else
                {
                    newNode = new LineageMatchNode { element = t };
                    children.Add(t.lineage, newNode);
                }
                newNode.RecursivelyCreate(lt, payload, depth + 1);
            }
        }
        public override string ToString()
        {
            return element != null ? element.lineage : "Empty";
        }
    }

    internal class MergeSet
    {
        internal LineageMatchNode major { get; set; }
        internal LineageMatchNode minor { get; set; }
    }
}
