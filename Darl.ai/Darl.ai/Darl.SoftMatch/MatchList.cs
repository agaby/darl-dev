/// </summary>

﻿using Darl.Lineage;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Darl.SoftMatch
{
    /// Implements the SoftMatch algorithm using a topological sort
    /// </summary>
    /// <remarks>Answers the question: Given  a set of short texts with unique indexes, what is the index of the text most similar in meaning to this text?</remarks>
    [ProtoContract()]
    public class MatchList : ISoftMatch
    {
        [ProtoMember(1)]
        public SortedList<string, List<string>> dict { get; set; } = new SortedList<string, List<string>>();

        [ProtoMember(2)]
        public Dictionary<string, List<string>> properNouns { get; set; } = new Dictionary<string, List<string>>();

        /// Create a local text repository as a concept reverse lookup
        /// </summary>
        /// <param name="data"></param>
        public void CreateTree(List<KeyValuePair<string, string>> data)
        {
            foreach (var d in data)
            {
                var words = LineageLibrary.SimpleTokenizer(d.Value.Replace('"', ' '));
                int index = 0;
                while (index < words.Count)
                {
                    var lineages = LineageLibrary.WordRecognizer(words, ref index);
                    foreach (var l in lineages)
                    {
                        if (l.lineage.StartsWith("noun") || l.lineage.StartsWith("verb") || l.lineage.StartsWith("adjective") || l.lineage.StartsWith("adverb")) //only index nouns and verbs
                        {
                            if (!dict.ContainsKey(l.lineage))
                            {
                                dict.Add(l.lineage, new List<string>());
                            }
                            dict[l.lineage].Add(d.Key);
                        }
                        if (l.lineage.StartsWith("proper_noun"))
                        {
                            var word = words[index - 1];
                            if (!properNouns.ContainsKey(word))
                            {
                                properNouns.Add(word, new List<string>());
                            }
                            properNouns[word].Add(d.Key);
                        }
                    }
                }
            }
        }

        /// Find the nearest paraphrase of the given text using the concept reverse lookup
        /// </summary>
        /// <param name="example"></param>
        /// <returns></returns>
        public MatchResult Find(string example)
        {
            var words = LineageLibrary.SimpleTokenizer(example);
            int index = 0;
            var list = new Dictionary<string, MatchResult>();
            int nounAndVerbCount = 0;
            while (index < words.Count)
            {
                var lineages = LineageLibrary.WordRecognizer(words, ref index);
                foreach (var l in lineages)
                {
                    if (l.lineage.StartsWith("noun") || l.lineage.StartsWith("verb")) //nouns and verbs with hypernymy
                    {
                        nounAndVerbCount++;
                        if (dict.ContainsKey(l.lineage))
                        {
                            var exact = dict[l.lineage];
                            foreach (var i in exact)
                            {
                                var weight = Weight(exact.Count);
                                if (!list.ContainsKey(i))
                                    list.Add(i, new MatchResult { sourceText = example, index = i });
                                list[i].matchedWords += weight;
                                //distance is zero
                            }
                            break;
                        }
                        dict.Add(l.lineage, new List<string>());
                        var p = dict.IndexOfKey(l.lineage);
                        List<string> closest = null;
                        double firstDistance = double.MaxValue;
                        if (p > 0) //look at the lineage below
                        {
                            var candLineage = dict.Keys[p - 1];
                            //check they share the same POS and root index
                            if (l.lineage.Contains(','))
                            {
                                if (candLineage.Contains(','))
                                {
                                    if (candLineage.Substring(0, candLineage.IndexOf(',')) == l.lineage.Substring(0, candLineage.IndexOf(',')))
                                    {
                                        closest = dict[candLineage];
                                        firstDistance = LineageLibrary.Similarity(candLineage, l.lineage);
                                    }
                                }
                                else
                                {
                                    if (l.lineage.StartsWith(candLineage))
                                    {
                                        closest = dict[candLineage];
                                        firstDistance = LineageLibrary.Similarity(candLineage, l.lineage);
                                    }
                                }
                            }
                            else
                            {
                                if (candLineage.StartsWith(l.lineage))
                                {
                                    closest = dict[candLineage];
                                    firstDistance = LineageLibrary.Similarity(candLineage, l.lineage);
                                }

                            }
                        }
                        if (p < dict.Count - 1) //look at the lineage above.
                        {
                            var candLineage = dict.Keys[p + 1];
                            //check they share the same POS and root index
                            if (l.lineage.Contains(','))
                            {
                                if (candLineage.Contains(','))
                                {
                                    if (candLineage.Substring(0, candLineage.IndexOf(',')) == l.lineage.Substring(0, candLineage.IndexOf(',')))
                                    {
                                        var otherClosest = dict[candLineage];
                                        var thisDistance = LineageLibrary.Similarity(candLineage, l.lineage);
                                        if (thisDistance > firstDistance)
                                        {
                                            closest = otherClosest;
                                            firstDistance = thisDistance;
                                        }
                                    }
                                }
                                else
                                {
                                    if (l.lineage.StartsWith(candLineage))
                                    {
                                        var otherClosest = dict[candLineage];
                                        var thisDistance = LineageLibrary.Similarity(candLineage, l.lineage);
                                        if (thisDistance > firstDistance)
                                        {
                                            closest = otherClosest;
                                            firstDistance = thisDistance;
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (candLineage.StartsWith(l.lineage))
                                {
                                    var otherClosest = dict[candLineage];
                                    var thisDistance = LineageLibrary.Similarity(candLineage, l.lineage);
                                    if (thisDistance > firstDistance)
                                    {
                                        closest = otherClosest;
                                        firstDistance = thisDistance;
                                    }
                                }
                            }
                        }
                        if (closest != null)
                        {
                            foreach (var i in closest)
                            {
                                var weight = Weight(closest.Count);
                                if (!list.ContainsKey(i))
                                    list.Add(i, new MatchResult { sourceText = example, index = i });
                                list[i].matchedWords += weight;
                                list[i].distance += (1.0 - firstDistance);
                            }
                        }
                        //now look down the tree for children
                        if (p < dict.Count - 1)
                        {
                            int current = p + 1;
                            while (current < dict.Count && dict.Keys[current].StartsWith(l.lineage))
                            {
                                var candLineage = dict.Keys[current];
                                var currIndexes = dict[candLineage];
                                foreach (var i in currIndexes)
                                {
                                    var weight = Weight(currIndexes.Count);
                                    if (!list.ContainsKey(i))
                                        list.Add(i, new MatchResult { sourceText = example, index = i });
                                    list[i].matchedWords += weight;
                                    list[i].distance += (1.0 - LineageLibrary.Similarity(candLineage, l.lineage));
                                }
                                current++;
                            }
                        }
                        dict.RemoveAt(p);
                    }
                    else if (l.lineage.StartsWith("adjective") || l.lineage.StartsWith("adverb"))
                    {//no hypernymy, so exact match only
                        nounAndVerbCount++;
                        if (dict.ContainsKey(l.lineage))
                        {
                            var exact = dict[l.lineage];
                            foreach (var i in exact)
                            {
                                var weight = Weight(exact.Count);
                                if (!list.ContainsKey(i))
                                    list.Add(i, new MatchResult { sourceText = example, index = i });
                                list[i].matchedWords += weight;
                                //distance is zero
                            }
                            break;
                        }
                    }
                    else if (l.lineage.StartsWith("proper_noun"))
                    {
                        nounAndVerbCount++;
                        var word = words[index - 1];
                        if (properNouns.ContainsKey(word))
                        {
                            var weight = Weight(properNouns[word].Count);
                            foreach (var i in properNouns[word])
                            {
                                if (!list.ContainsKey(i))
                                    list.Add(i, new MatchResult { sourceText = example, index = i });
                                list[i].matchedWords += weight;
                                //distance is zero
                            }
                        }
                    }
                }
            }
            if (list.Count == 0)
                return null;
            //first pass return most matched words
            var sorted = list.Values.OrderByDescending(a => a.matchedWords).ToList();
            var r = sorted.First();
            int count = 0;
            foreach (var c in sorted)
            {
                if (c.matchedWords != r.matchedWords)
                    break;
                r.tieCount++;
                count++;
            }
            for (int n = 1; n < Math.Min(4, sorted.Count); n++)
            {
                var c = sorted[n];
                r.alternatives.Add(c.index, c.matchedWords);
            }
            r.confidence = (double)r.matchedWords / nounAndVerbCount;
            return r;
        }

        public byte[] SerializeGraph()
        {
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize<MatchList>(ms, this);
                ms.Position = 0;
                return ms.ToArray();
            }
        }

        public static MatchList DeserializeGraph(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            {
                ms.Position = 0;
                return Serializer.Deserialize<MatchList>(ms);
            }
        }

        public void Flush()
        {

        }

        /// Create a weighting that favours unusual concepts
        /// </summary>
        /// <param name="count"></param>
        /// <returns></returns>
        private double Weight(int count)
        {
            //return 1.0 / Math.Log(count);
            return 1.0 / Math.Pow(count, 0.5);
        }

    }
}
