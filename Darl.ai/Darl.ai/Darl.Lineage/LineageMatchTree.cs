using DarlCommon;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.Lineage
{
    [ProtoContract]
    public class LineageMatchTree
    {
        [ProtoMember(1)]
        public LineageMatchNode root { get; set; } = new LineageMatchNode() { element = null };

        public LineageMatchNode executionRoot { get; set; } = new LineageMatchNode() { element = null };

        public bool changed { get; protected set; } = false;

        public LineageMatchTree(List<LineageTemplateSet> source)
        {
            foreach (var lts in source)
            {
                root.Create(lts);
            }
        }

        public LineageMatchTree()
        {

        }

        public void CreateExecutionTree()
        {
            executionRoot = root.CreateExecutionGraph(new HashSet<LineageAnnotationNode>());
            changed = false;
        }

        public List<MatchedElement> Match(List<string> tokens, List<DarlVar> values, bool fuzzy = false)
        {
            if (changed)
                CreateExecutionTree();
            var matches = new List<MatchedAnnotation>();
            var defaultMatches = new List<DefaultAnnotation>();
            var stack = new Stack<LineageAnnotationNode>();
            //convert to make composite
            while (tokens.Count > 0)
            {
                string path = string.Empty;
                executionRoot.Match(tokens, values, matches, defaultMatches, path,0,fuzzy);
                tokens.RemoveAt(0);
            }
            matches.Sort();
            //choose the deepest first defaultmatch
            MatchedAnnotation lan = null;
            int lanDepth = 0;
            foreach (var match in defaultMatches)
            {
                if (lan == null)
                { 
                    lan = new MatchedAnnotation { annotation = match.Node, path = match.path };
                    lanDepth = match.Depth;
                }
                else if(match.Depth > lanDepth)
                {
                    lan = new MatchedAnnotation { annotation = match.Node, path = match.path };
                    lanDepth = match.Depth;
                }
            }
            var compMatches = new List<MatchedElement>();
            compMatches.Add(lan);
            compMatches.AddRange(matches);
            return compMatches;
        }

        /// <summary>
        /// Navigates the tree for display purposes
        /// </summary>
        /// <param name="path">'/' delimited path</param>
        /// <returns></returns>
        public List<LineageMatchNode> Navigate(string path)
        {
            var tokens = path.Split('/').ToList();
            return root.Navigate(tokens);
        }

        public List<LineageMatchNode> NavigateExecutionTree(string path)
        {
            var tokens = path.Split('/').ToList();
            if (!executionRoot.children.Any() || changed)
                CreateExecutionTree();
            return executionRoot.Navigate(tokens);
        }


        public void Delete(string path)
        {
            var tokens = path.Split('/').ToList();
            root.Delete(tokens);
            changed = true;
        }

        public LineageMatchNode Add(string parent, string newName)
        {
            var tokens = string.IsNullOrEmpty(parent) ? new List<string>() : parent.Split('/').ToList();
            changed = true;
            return root.Add(tokens, newName);
        }

        public void Rename(string path, string newName)
        {
            var tokens = path.Split('/').ToList();
            root.Rename(tokens, newName);
            changed = true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="parent">Address of the parent for move/copy</param>
        /// <param name="nodes">list of node addresses </param>
        /// <param name="mode">"copy_node" or "move_node"</param>
        public void Paste(string parent, List<string> nodes, string mode)
        {
            var tokens = parent.Split('/').ToList();
            //nodes contains a list of the nodes to copy. The shortest will be the top node
            string copyRoot = "";
            foreach (var n in nodes)
            {
                if (string.IsNullOrEmpty(copyRoot) || n.Count() < copyRoot.Count())
                {
                    copyRoot = n;
                }
            }
            root.Paste(tokens, Find(copyRoot));
            if (mode == "move_node")
            {
                Delete(copyRoot);
            }
            changed = true;
        }

        internal void AddDescriptions()
        {
            root.AddDescriptions();
        }

        internal void Rationalize(StringBuilder sb)
        {
            root.Rationalize(sb);
        }

        public LineageMatchNode Find(string path)
        {
            var tokens = path.Split('/').ToList();
            return root.Find(tokens);
        }

        public LineageMatchNode FindExecutionTree(string path)
        {
            var tokens = path.Split('/').ToList();
            if (!executionRoot.children.Any() || changed)
                CreateExecutionTree();
            return executionRoot.Find(tokens);
        }



        public void SaveAttributes(string path, string darl, List<string> implications, List<string> roles)
        {
            var dest = Find(path);
            if (dest != null)
            {
                if (dest.annotation == null)
                    dest.annotation = new LineageAnnotationNode();
                else
                {
                    dest.annotation.darl.Clear();
                    dest.annotation.implications.Clear();
                    dest.annotation.accessRoles.Clear();
                }
                dest.annotation.darl.Add(darl);
                dest.annotation.implications = implications;
                dest.annotation.accessRoles = roles;
            }
            changed = true;
        }

        /// <summary>
        /// return a list of paths to tree matches for the string supplied
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        internal List<SearchCandidate> FindSearchPaths(string str)
        {
            var tokens = LineageLibrary.SimpleTokenizer(str);
            var noPunctuation = new List<string>();
            foreach(var tok in tokens)
            {
                if(tok.Length > 1 || !Char.IsPunctuation(tok[0]))//strip punctuation
                {
                    noPunctuation.Add(tok);
                }
            }
            var res = new List<SearchCandidate>();
            root.FindSearchPaths(noPunctuation, new Stack<string>(), res);
            return res;
        }

        /// <summary>
        /// Return the sequence of nodes in a path
        /// </summary>
        /// <param name="path">the path</param>
        /// <returns>the sequence</returns>
        public List<LineageMatchNode> FindSequence(string path)
        {
            var list = new List<LineageMatchNode>();
            var tokens = path.Split('/').ToList();
            list.Add(root);
            root.FindSequence(tokens, list);
            return list;
        }

        public void ReadTree(StringBuilder sb)
        {
            root.ReadTree(sb);
        }

        private static bool CanHaveChildren(string text)
        {
            if (text.ToLower().Contains("default:"))
                return false;
            if (text.ToLower().Contains("value:"))
                return false;
            return true;
        }

        internal List<SearchCandidate> BestMatch(List<string> tokens)
        {
            if (changed)
                CreateExecutionTree();
            LineageMatchNode.comp.lineageMatch = true;
            var res = new List<SearchCandidate>();
            while (tokens.Count > 0)
            {
                executionRoot.FindSearchPaths(tokens, new Stack<string>(), res);
                tokens.RemoveAt(0);
            }
            LineageMatchNode.comp.lineageMatch = false;
            return res;
        }
    }

    internal class SearchCandidate
    {
        public string path { get; set; }
        public string fullpath { get; set; }
        public int depth { get; set; }
    }
}
