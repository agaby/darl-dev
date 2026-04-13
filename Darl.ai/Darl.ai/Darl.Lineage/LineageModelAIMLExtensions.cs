/// </summary>

﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;

namespace Darl.Lineage
{
    public static class LineageModelAIMLExtensions
    {

        public static Dictionary<string, LineageTemplateSet> templatesByPayload { get; set; } = new Dictionary<string, LineageTemplateSet>();

        //used during aiml construction only
        public static Dictionary<string, List<LineageTemplateSet>> textLookup { get; set; } = new Dictionary<string, List<LineageTemplateSet>>();

        public static List<SraiLookup> sraiLookup = new List<SraiLookup>();

        public static double percentCoverage { get; set; } = 0.0;
        static int covered = 0;
        static int uncovered = 0;



        public static void LoadAIML(this LineageModel lm, Stream str)
        {
            var doc = XDocument.Load(str);
            foreach (var cat in doc.Root.Elements("category"))
            {
                try
                {
                    LineageTemplateSet lts;
                    var t = cat.Element("template").ToString();
                    if (string.IsNullOrEmpty(t))
                        continue;
                    var p = cat.Element("pattern").Value;
                    Trace.WriteLine(p);
                    if (cat.Element("template").Element("srai") != null)
                    {
                        //todo: if srai contains <star /> add it as *
                        var srai = cat.Element("template").Element("srai");
                        string rep = srai.Value;
                        if (srai.Element("star") != null) //template contains *
                        {
                            var sb = new StringBuilder();
                            foreach (var e in srai.Nodes())
                            {
                                if (e.NodeType == System.Xml.XmlNodeType.Element)
                                {
                                    sb.Append("* ");
                                }
                                else if (e.NodeType == System.Xml.XmlNodeType.Text)
                                {
                                    sb.Append(e.ToString() + " ");
                                }
                            }
                            rep = sb.ToString().Trim();
                        }
                        else
                            sraiLookup.Add(new SraiLookup { source = p, replace = rep });
                    }
                    else
                    {
                        var processedT = ProcessPayload(t);
                        if (templatesByPayload.ContainsKey(processedT))
                            lts = templatesByPayload[processedT];
                        else
                        {
                            lts = new LineageTemplateSet() { templates = new List<LineageTemplate>(), payload = processedT };
                            if (!string.IsNullOrEmpty(lts.payload))
                                templatesByPayload.Add(processedT, lts);
                            else
                                continue; //ignore this one
                        }
                        var lt = CreateLineageTemplate(p);
                        lts.templates.Add(lt);
                        if (!textLookup.ContainsKey(p))
                            textLookup.Add(p, new List<LineageTemplateSet>());
                        textLookup[p].Add(lts);
                    }
                }
                catch { }
            }
        }
        /// Tidy up after loading all AIML 
        /// </summary>
        public static void PostProcessAIML(this LineageModel lm)
        {
            foreach (var s in sraiLookup)
            {
                if (textLookup.ContainsKey(s.replace))
                {
                    foreach (var lts in textLookup[s.replace])
                    {
                        lts.templates.Add(CreateLineageTemplate(s.source));
                    }
                }
            }

            templatesByPayload.Clear();
            textLookup.Clear();
            sraiLookup.Clear();
            percentCoverage = covered / (double)(covered + uncovered);
        }

        public static LineageTemplate CreateLineageTemplate(string p)
        {
            var tokens = LineageLibrary.SimpleTokenizer(p);
            var lt = new LineageTemplate() { sequence = new List<List<LineageElement>>(), text = p };
            foreach (var tok in tokens)
            {
                if (tok == "*" || tok == "_") //these are wildcards as placeholders for textual values
                    lt.sequence.Add(new List<LineageElement>() { new LineageElement { lineage = "value:", type = LineageType.value } });
                else
                    lt.sequence.Add(new List<LineageElement>() { new LineageElement { lineage = tok.ToLower(), type = LineageType.literal } });
            }
            return lt;
        }

        private static string ProcessPayload(string t)
        {
            var e = XElement.Parse(t);
            var sb = new StringBuilder();
            //first scenario, just text. 
            if (e.Descendants().Count() == 0)
            {
                sb.AppendLine($"if anything then response will be \"{e.Value}\";"); //covers 58%
                covered++;
            }
            else //parse the aiml
            {
                bool handled = false;
                bool responseCreated = false;
                foreach (var n in e.Elements())
                {
                    if (RecurseNodes(n, sb, ref responseCreated))
                        handled = true;
                }
                if (!responseCreated)
                {
                    sb.AppendLine($"if anything then response will be \"{e.Value}\";");
                    handled = true;
                }
                if (handled)
                    covered++;
                else
                    uncovered++;
            }
            return sb.ToString();
        }
        private static bool RecurseNodes(XElement e, StringBuilder sb, ref bool responseCreated)
        {
            bool handled = false;
            switch (e.Name.LocalName)
            {
                case "set":
                    {
                        if (e.Parent.Name.LocalName != "set" && !string.IsNullOrEmpty(e.Parent.Value) && e.Parent.Value != e.Value)
                        {
                            sb.AppendLine($"if anything then response will be \"{e.Parent.Value}\";");
                            responseCreated = true;
                        }

                        sb.AppendLine($"if anything then {e.Attribute("name").Value} will be \"{e.Value}\";");
                        handled = true;
                    }
                    break;
                case "random":
                    {
                        sb.Append("if anything then response will be randomtext(");
                        var list = e.Elements("li").ToList();
                        int index = 0;
                        foreach (var li in list)
                        {
                            var c = ++index != list.Count ? "," : "";
                            sb.Append($"\"{li.Value}\"{c} ");
                        }
                        sb.Append(");");
                        responseCreated = true;
                    }
                    return true;
                case "bot":
                    {
                        e.ReplaceWith(new XText($"%% bot.{e.Attribute("name").Value} %%"));
                    }
                    break;
                case "get":
                    {
                        e.ReplaceWith(new XText($"%% {e.Attribute("name").Value} %%"));
                    }
                    break;
            }
            foreach (var n in e.Elements())
            {
                var h = RecurseNodes(n, sb, ref responseCreated);
                if (h)
                    handled = true;
            }
            return handled;
        }

    }



    public class SraiLookup
    {
        public string source { get; set; }

        public string replace { get; set; }
    }

    public class SetData : IComparable<SetData>
    {
        public int incidence { get; set; } = 0;

        public HashSet<string> values { get; } = new HashSet<string>();

        public string name { get; set; }

        public int CompareTo(SetData other)
        {
            return incidence.CompareTo(other.incidence);
        }

        public override string ToString()
        {
            var vals = "more than 5";
            if (values.Count < 5)
            {
                var sb = new StringBuilder();
                sb.Append("{");
                foreach (var s in values)
                {
                    sb.Append(s + ", ");
                }
                sb.Append("}");
                vals = sb.ToString();
            }
            return $"{name}, {incidence}, {vals}";
        }
    }
}
