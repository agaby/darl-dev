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
    public class LineageMatchTreeOld
    {
        [ProtoMember(1)]
        public LineageMatchNode2 root { get; private set; } = new LineageMatchNode2() { element = null };


        public LineageMatchTreeOld(List<LineageTemplateSet> source)
        {
            foreach(var lts in source)
            {
                root.Create(lts);
            }
        }

        public LineageMatchTreeOld()
        {

        }

        public List<string> Match(List<string> tokens, List<DarlVar> values)
        {
            var matches = new List<string>();
            var defaultMatches = new List<string>();
            root.Match(tokens, values, matches, defaultMatches);
            return matches.Count > 0 ? matches : defaultMatches;
        }

        /// <summary>
        /// Navigates the tree for display purposes
        /// </summary>
        /// <param name="path">'/' delimited path</param>
        /// <returns></returns>
        public List<LineageMatchNode2> Navigate(string path)
        {
            var tokens = path.Split('/').ToList();
            return root.Navigate(tokens);
        }

        public void Delete(string path)
        {
            var tokens = path.Split('/').ToList();
            root.Delete(tokens);
        }

        public void Add(string parent,  string newName)
        {
            var tokens = string.IsNullOrEmpty(parent) ? new List<string>() : parent.Split('/').ToList();
            root.Add(tokens, newName);
        }

        public void Rename(string path, string newName)
        {
            var tokens = path.Split('/').ToList();
            root.Rename(tokens, newName);
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
            foreach(var n in nodes)
            {
                if(string.IsNullOrEmpty(copyRoot) || n.Count() < copyRoot.Count())
                {
                    copyRoot = n;
                }
            }
            root.Paste(tokens, Find(copyRoot));
            if(mode == "move_node")
            {
                Delete(copyRoot);
            }
                
        }

        public LineageMatchNode2 Find(string path)
        {
            var tokens = path.Split('/').ToList();
            return root.Find(tokens);
        }

        public void SaveAttributes(string path, string darl, List<string> implications)
        {
            var dest = Find(path);
            if(dest != null)
            {
                dest.darl = new List<string>() { darl };
                dest.implications = implications;
            }
        }

        /// <summary>
        /// return a list of paths to tree matches for the string supplied
        /// </summary>
        /// <param name="str"></param>
        /// <returns></returns>
        public List<string> FindSearchPaths(string str)
        {
            var tokens = LineageLibrary.SimpleTokenizer(str);
            var res = new List<string>();
            root.FindSearchPaths(tokens, new Stack<string>(), res);
            return res;
        }

        public void ReadTree(StringBuilder sb)
        {
            root.ReadTree(sb);
        }
/*
        public LineageTree Convert()
        {
            var lt = new LineageTree();
            lt.root = this.root.Convert();
            return lt;
        }
        */
    }
}
