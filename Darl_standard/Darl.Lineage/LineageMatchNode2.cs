using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DarlCommon;
using ProtoBuf;

namespace Darl.Lineage
{
    /// <summary>
    /// Alternative implementation that uses much less space.
    /// </summary>
    [ProtoContract()]
    public class LineageMatchNode2
    {
        public static LineageComparer comp { get; set; } = new LineageComparer(); //single comparer

        [ProtoMember(1)]
        public LineageElement element { get; set; }

        [ProtoMember(2)]
        public SortedList<string, LineageMatchNode2> children { get; set; } = new SortedList<string, LineageMatchNode2>(comp);

        /// <summary>
        /// all the pieces of darl code associated with the sequence
        /// </summary>
        [ProtoMember(3)]
        public List<string> darl { get; set; } = new List<string>();

        /// <summary>
        /// all the implications associated with the sequence
        /// </summary>
        [ProtoMember(4)]
        public List<string> implications { get; set; } = new List<string>();

        public static string defaultLabel = "default:";

        public LineageMatchNode2()
        {

        }

        internal void Create(LineageTemplateSet lts)
        {
            foreach (var lt in lts.templates)
            {
                RecursivelyCreate(lt, lts.payload);
            }
        }

        internal List<LineageMatchNode2> Navigate(List<string> tokens)
        {
            return RecursivelyNavigate(tokens);
        }

        internal List<LineageMatchNode2> RecursivelyNavigate(List<string> tokens, int depth = 0)
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
            return new List<LineageMatchNode2>();
        }

        internal void Add(List<string> tokens, string newName, int depth = 0)
        {
            if (depth == tokens.Count || (tokens.Count == 1 && depth == 0 && tokens[0] == "#"))//# indicates at root.
            {
                if (newName.Contains(":") && LineageLibrary.lineages.ContainsKey(newName))
                {
                    children.Add(newName, new LineageMatchNode2() { element = LineageLibrary.lineages[newName] });
                }
                else
                {
                    children.Add(newName, new LineageMatchNode2() { element = new LineageElement() { lineage = newName, type = LineageType.literal } });
                }
                return;
            }
            var tok = tokens[depth];
            if (children.ContainsKey(tok)) //handle literal matches
            {
                children[tok].Add(tokens, newName, depth + 1);
            }
        }

        /// <summary>
        /// copy copyroot and it's children onto the end of the tokens path.
        /// </summary>
        /// <param name="tokens"></param>
        /// <param name="copyRoot"></param>
        internal void Paste(List<string> tokens, LineageMatchNode2 copyRoot, int depth = 0)
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
                    if (newName.Contains(":") ) //handle symbols
                    {
                        string label;
                        var parsedName = ParseValue(newName, out label);
                        if (LineageLibrary.lineages.ContainsKey(parsedName))
                        {
                            node.element = LineageLibrary.lineages[parsedName];
                            if (!string.IsNullOrEmpty(label))
                                node.implications.Add(label);
                        }
                        else
                        {
                            throw new Exception($"invalid symbol: {parsedName}");
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

        private string ParseValue(string name, out string label)
        {
            if(name.Contains("("))
            {
                int labelStart = name.IndexOf('(');
                int labelEnd = name.IndexOf(')');
                label = name.Substring(labelStart + 1, labelEnd == -1 ? name.Length - labelStart - 1 : labelEnd - labelStart - 1);
                return name.Substring(0, labelStart);
            }
            label = "";
            return name;
        }

        internal void FindSearchPaths(List<string> tokens, Stack<string> stack, List<string> results, int depth = 0)
        {
            if (depth >= tokens.Count)
            {
                StackToPath(stack, results);
                return;
            }
            else if(stack.Any())
            {
                StackToPath(stack, results);
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
                stack.Push(tok);
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
                        stack.Push(w.lineage);
                        children[w.lineage].FindSearchPaths(tokens, stack, results, nextDepth);
                        stack.Pop();
                    }
                }
            }
        }

        private void StackToPath(Stack<string> stack, List<string> results)
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
                    res += "/" + s;
                }
            }
            results.Add(res);
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

        internal LineageMatchNode2 Find(List<string> tokens, int depth = 0)
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

        internal void Match(List<string> tokens, List<DarlVar> values, List<string> matches, List<string> defaultMatches, int depth = 0)
        {
            if (depth >= tokens.Count)
            {
                if (darl != null)
                    matches.AddRange(darl);
                return;
            }
            int nextDepth = depth;
            List<LineageRecord> cs = null;
            if(children.ContainsKey(defaultLabel))//pick up any default rules, overwriting those further up the tree.
            {
                if (children[defaultLabel].darl != null)
                {
                    defaultMatches.Clear();
                    defaultMatches.AddRange(children[defaultLabel].darl);
                }
            }
            var tok = tokens[nextDepth];
            if (children.ContainsKey(tok)) //handle literal matches
            {
                children[tok].Match(tokens, values, matches, defaultMatches, nextDepth + 1);  
            }
            else //handle lineage matches and values - note, literal matches win
            {
                foreach (var v in children.Values.Where(a => a.element.type == LineageType.value)) //handle values
                {
                    cs = LineageLibrary.WordRecognizer(tokens, ref nextDepth);
                    values.Add(LineageLibrary.HandleValues(v.element, tokens, ref nextDepth, cs, v.implications.Count > 0 ? v.implications[0] : ""));
                    v.Match(tokens, values, matches, defaultMatches, nextDepth + 1);
                }
                if (cs == null)
                    cs = LineageLibrary.WordRecognizer(tokens, ref nextDepth);
                foreach (var w in cs)
                {
                    if (children.ContainsKey(w.lineage)) //handle lineage matches
                    {
                        children[w.lineage].Match(tokens, values, matches, defaultMatches, nextDepth);
                    }
                }
            }
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
                    if (darl == null)
                        darl = new List<string>();
                    this.darl.Add(payload);
                }
                return;
            }
            foreach (var t in lt.sequence[depth])
            {
                LineageMatchNode2 newNode;
                if (children.ContainsKey(t.lineage))
                    newNode = children[t.lineage];
                else
                {
                    newNode = new LineageMatchNode2 { element = t };
                    children.Add(t.lineage, newNode);
                }
                newNode.RecursivelyCreate(lt, payload, depth + 1);
            }
        }
        public override string ToString()
        {
            return element != null ? element.lineage : "Empty";
        }

        public LineageMatchNode Convert()
        {
            var lmn = new LineageMatchNode { element = this.element, annotation = darl.Count == 0 && implications.Count == 0 ? null : new LineageAnnotationNode { darl = this.darl, implications = this.implications } };
            foreach(var s in children.Keys)
            {
                lmn.children.Add(s, children[s].Convert());
            }
            return lmn;
        }
    }
}
