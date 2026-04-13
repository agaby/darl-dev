/// <summary>
/// </summary>

﻿using Darl.SoftMatch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Darl_standard_core.test
{
    [TestClass]
    public class SoftMatchTest
    {

        /// <summary>
        /// Test the algorithm on the Microsoft Research Paraphrase dataset.
        /// </summary>
        [TestMethod]
        public void TestMSRParaphraseList()
        {
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.msr_paraphrase_train.txt"));
            var doc = docsource.ReadToEnd();
            var lines = doc.Split('\n').ToList();
            lines.RemoveAt(0); //get rid of header
            var equivalents = new Dictionary<string, string>();
            var dict = new List<KeyValuePair<string, string>>();
            var list = new MatchList();
            foreach (var line in lines)
            {
                var elements = line.Split('\t');
                if (elements.Count() != 5)
                {
                    continue;
                }
                if (elements[0] == "1")
                {
                    dict.Add(new KeyValuePair<string, string>(elements[1], elements[3]));
                    equivalents.Add(elements[1], elements[4].Trim());
                }
            }
            list.CreateTree(dict);
            //pass through serialization to test
            var model = list.SerializeGraph();
            list = MatchList.DeserializeGraph(model);

            var indices = new List<string>();
            var textList = new List<string>();
            foreach (var i in equivalents.Keys)
            {
                indices.Add(i);
                textList.Add(equivalents[i]);
            }
            var res = new List<MatchResult>();
            int index = 0;
            foreach (var text in textList)
            {
                res.Add(list.Find(text));
                index++;
            }
            int correct = 0;
            int noTieError = 0;
            int topThree = 0;
            var correctConfidenceDistribution = new List<double>();
            var incorrectConfidenceDistribution = new List<double>();
            for (int n = 0; n < 100; n++)
            {
                correctConfidenceDistribution.Add(0.0);
                incorrectConfidenceDistribution.Add(0.0);
            }

            for (int n = 0; n < indices.Count; n++)
            {
                if (res[n] != null)
                {
                    if (res[n].index == indices[n])
                    {
                        correct++;
                        topThree++;
                        correctConfidenceDistribution[(int)(res[n].confidence * 100)] += 1.0;
                    }
                    else
                    {
                        if (res[n].tieCount == 1)
                        {
                            noTieError++;
                        }
                        if (res[n].alternatives.ContainsKey(indices[n]))
                            topThree++;
                        incorrectConfidenceDistribution[(int)(res[n].confidence * 100)] += 1.0;
                    }
                }
            }
            var sb = new StringBuilder();
            for (int n = 0; n < 100; n++)
            {
                sb.AppendLine($"{correctConfidenceDistribution[n]}, {incorrectConfidenceDistribution[n]}");
            }
            File.WriteAllText("confidenceDistributions.csv", sb.ToString());
            Assert.IsTrue((correct / (double)textList.Count) >= 0.78);
        }

        [TestMethod]
        public void TestSectionFilter()
        {
            var filtergraph = new MatchList();
            var dict = new List<KeyValuePair<string, string>> { new KeyValuePair<string, string>("use", "skills qualifications responsibilities knowledge experience job requirements role") };
            filtergraph.CreateTree(dict);
            var res = filtergraph.Find("what we do");
            res = filtergraph.Find("in addition to the above finance functions, you'll also need to be able to commit to:");
        }
    }
}
