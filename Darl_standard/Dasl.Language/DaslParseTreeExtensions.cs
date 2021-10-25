// ***********************************************************************
// Assembly         : DaslLanguage
// Author           : Andrew
// Created          : 08-25-2015
//
// Last Modified By : Andrew
// Last Modified On : 08-26-2015
// ***********************************************************************
// <copyright file="DaslParseTreeExtensions.cs" company="Dr Andy's IP LLC">
//     Copyright ©  2015
// </copyright>
// <summary></summary>
// ***********************************************************************
using System.Collections.Generic;
using DarlCompiler.Parsing;
using DarlLanguage.Processing;
using System.Xml.Linq;

namespace DaslLanguage
{
    /// <summary>
    /// Extensions to DASL functionality
    /// </summary>
    public static class DaslParseTreeExtensions
    {

        /// <summary>
        /// Draws a GraphML graph.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <returns>GraphML representation</returns>
        public static string DrawGraph(this ParseTree tree)
        {
            XDocument doc = new XDocument();
            XNamespace ns = "http://graphml.graphdrawing.org/xmlns";
            doc.Add(new XElement(ns + "graphml"));
            var graphroot = new XElement(ns + "graph", new XAttribute("id", "model"), new XAttribute("edgedefault", "directed"));
            doc.Root.Add(graphroot);
            var root = tree.Root.AstNode as MapRootNode;
            foreach (var output in root.outputs.Keys)
            {
                graphroot.Add(new XElement(ns + "node", new XAttribute("id", output), new XElement(ns + "port", new XText("real")), new XElement(ns + "port", new XText("sim")), new XElement(ns + "port", new XText("out"))));
            }

            int ncount = 0;
            int ecount = 0;
            foreach (var delay in root.delays)
            {
                string delayiD = "d" + ncount++;
                graphroot.Add(new XElement(ns + "node", new XAttribute("id", delayiD), new XElement(ns + "port", new XText("in")), new XElement(ns + "port", new XText("out"))));
                graphroot.Add(new XElement(ns + "edge", new XAttribute("id", "e" + ecount++), new XAttribute("source", delay.sourcename), new XAttribute("target", delayiD), new XAttribute("sourceport", "out"), new XAttribute("targetport", "in")));
                graphroot.Add(new XElement(ns + "edge", new XAttribute("id", "e" + ecount++), new XAttribute("source", delayiD), new XAttribute("target", delay.destname), new XAttribute("sourceport", "out"), new XAttribute("targetport", "sim")));
            }

            foreach (var wire in root.wires)
            {

            }
            return doc.ToString();
        }


        /// <summary>
        /// Updates the DASL diagram.
        /// </summary>
        /// <param name="tree">The tree.</param>
        /// <returns>DaslGraph.</returns>

        public static DaslGraph CreateDaslDiagram(this ParseTree tree)
        {
            if (tree.HasErrors())
                return null;
            var graph = new DaslGraph(true);
            var simvertices = new Dictionary<string, DaslVertex>();
            var realvertices = new Dictionary<string, DaslVertex>();
            var root = tree.Root.AstNode as MapRootNode;
            foreach (var output in root.outputs.Keys)
            {
                var v = new DaslVertex(output, VertexType.observable);
                graph.AddVertex(v);
                simvertices.Add(output, v);
            }
            foreach (var rset in root.rulesets.Keys)
            {
                string rsetName;
                if (simvertices.ContainsKey(rset))
                {
                    rsetName = rset + "_ruleset";
                }
                else
                {
                    rsetName = rset;
                }
                var v = new DaslVertex(rsetName, VertexType.ruleset);
                graph.AddVertex(v);
                realvertices.Add(rsetName, v);
            }
            int dCount = 0;
            foreach (var delay in root.delays)
            {
                string dname = "delay" + dCount++;
                var v = new DaslVertex(dname, VertexType.delay);
                graph.AddVertex(v);
                AddNewGraphEdge(simvertices[delay.sourcename], v, graph);
                AddNewGraphEdge(v, realvertices[delay.destRuleset], graph);
            }

            foreach (var wire in root.wires)
            {
                if (wire.wiretype == WireDefinitionNode.WireType.wireout)
                {
                    if (realvertices.ContainsKey(wire.sourceRuleset))
                        AddNewGraphEdge(realvertices[wire.sourceRuleset], simvertices[wire.destname], graph);
                    else
                        AddNewGraphEdge(realvertices[wire.sourceRuleset + "_ruleset"], simvertices[wire.destname], graph);
                }
            }
            return graph;
        }

        /// <summary>
        /// Adds the new graph edge.
        /// </summary>
        /// <param name="from">From.</param>
        /// <param name="to">To.</param>
        /// <param name="graph">The graph.</param>
        /// <returns>DaslEdge.</returns>
        private static DaslEdge AddNewGraphEdge(DaslVertex from, DaslVertex to, DaslGraph graph)
        {
            string edgeString = string.Format("{0}-{1} Connected", from.ID, to.ID);

            DaslEdge newEdge = new DaslEdge(edgeString, from, to);
            graph.AddEdge(newEdge);
            return newEdge;
        }

    }
}
