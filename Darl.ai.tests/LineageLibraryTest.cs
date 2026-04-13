/// <summary>
/// </summary>

﻿using DarlCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;

namespace Darl.Lineage.Tests
{
    [TestClass]
    public class LineageLibraryTest
    {
        [TestMethod]
        public void TestLibraryStaticConstructor()
        {
            Assert.AreEqual(118118, LineageLibrary.lineages.Count);
            Assert.AreEqual(151521, LineageLibrary.words.Count);
        }

        [TestMethod]
        public void TestSimpleLineageMatch()
        {
            List<string> quitLineages = new List<string> { "verb:060", "verb:399,0", "verb:190", "verb:331,02", "verb:015,180,0", "verb:015,022,29", "verb:185,1,18", "verb:004,62" };
            List<string> backLineages = new List<string> { "verb:023,17,2", "verb:165,0,2,0", "verb:015,018,08,2", "verb:347", "verb:172,072" };
            List<string> helpLineages = new List<string> { "verb:397,2", "verb:023,24,02,4,0" };
            List<string> aboutLineages = new List<string> { "preposition:2", "noun:01,4,05,13,04", "noun:01,2,07,10,13,4", "noun:00,1,01,19,14,1" };
            List<string> historyLineages = new List<string> { "noun:01,4,09,01,3,4", "noun:01,1,02,09,0" };
            List<string> debugLineages = new List<string> { "verb:015,018,04,1" };
            Assert.IsTrue(LineageLibrary.Match(" exit ", quitLineages));
            Assert.IsFalse(LineageLibrary.Match(" poop ", quitLineages));
            Assert.IsTrue(LineageLibrary.Match(" leave ", quitLineages));
            Assert.IsTrue(LineageLibrary.Match(" back ", backLineages));
            Assert.IsFalse(LineageLibrary.Match(" whistle ", backLineages));
            Assert.IsTrue(LineageLibrary.Match(" go back ", backLineages));
        }

        [TestMethod]
        public void TestSequenceLineageMatch()
        {
            var sequence = new List<List<string>> { new List<string> { "noun:01,2,07,10,13,4" }, new List<string> { "verb:019,005,4", "verb:063", "verb:207,0,1,17", "verb:222,01" }, new List<string> { "noun:01,4,18", "noun:00,1,00,3,10,09,07", "noun:01,4,09,01,3,3,0,8", "noun:01,4,04,02,21" } };
            List<DarlVar> values;
            Assert.IsTrue(LineageLibrary.Match(" the company shall keep the documents ", sequence, out values));
            Assert.IsTrue(LineageLibrary.Match(" the company shall retain the accounts ", sequence, out values));
            Assert.IsFalse(LineageLibrary.Match(" There is a house in North Ontario", sequence, out values));
            Assert.IsFalse(LineageLibrary.Match(" with dream comfort memories to share ", sequence, out values));
            Assert.IsFalse(LineageLibrary.Match(" in my mind I've still got a place to go ", sequence, out values));
            Assert.IsFalse(LineageLibrary.Match(" all my changes were there ", sequence, out values));
        }

        [TestMethod]
        public void TestSequenceLineageMatchDate()
        {
            var sequence = new List<List<string>> { new List<string> { "noun:01,2,07,10,13,4" }, new List<string> { "verb:019,005,4", "verb:063", "verb:207,0,1,17", "verb:222,01" }, new List<string> { "noun:01,4,18", "noun:00,1,00,3,10,09,07", "noun:01,4,09,01,3,3,0,8", "noun:01,4,04,02,21" }, new List<string> { "value:date" } };
            List<DarlVar> values;
            Assert.IsTrue(LineageLibrary.Match(" the company shall retain the accounts until 20/06/2017", sequence, out values));
            Assert.AreEqual(1, values.Count);
            values.Clear();
            Assert.IsTrue(LineageLibrary.Match(" the company shall keep the documents until 5th June 2017 and burn them ", sequence, out values));
            Assert.AreEqual(1, values.Count);
            Assert.IsFalse(LineageLibrary.Match(" There is a house in North Ontario", sequence, out values));
            Assert.IsFalse(LineageLibrary.Match(" with dream comfort memories to share ", sequence, out values));
            Assert.IsFalse(LineageLibrary.Match(" in my mind I've still got a place to go ", sequence, out values));
            Assert.IsFalse(LineageLibrary.Match(" all my changes were there ", sequence, out values));
        }

        [TestMethod]
        [Ignore]//fix this
        public void TestSequenceLineageMatchPeriod()
        {
            var sequence = new List<List<string>> { new List<string> { "noun:01,2,07,10,13,4" }, new List<string> { "verb:019,005,4", "verb:063", "verb:207,0,1,17", "verb:222,01" }, new List<string> { "noun:01,4,18", "noun:00,1,00,3,10,09,07", "noun:01,4,09,01,3,3,0,8", "noun:01,4,04,02,21" }, new List<string> { "value:duration" } };
            List<DarlVar> values;

            Assert.IsTrue(LineageLibrary.Match(" the company shall keep the documents for a year and then bury them in a hole in the ground in Neasden ", sequence, out values));
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(DateTime.MinValue, values[0].times[0]);
            Assert.AreEqual(new TimeSpan(365, 0, 0, 0).ToString(), values[0].Value);

            Assert.IsTrue(LineageLibrary.Match(" the company shall keep the documents for two years and burn them ", sequence, out values));
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(DateTime.MinValue, values[0].times[0]);
            Assert.AreEqual(new TimeSpan(730, 0, 0, 0).ToString(), values[0].Value);

            Assert.IsTrue(LineageLibrary.Match(" the company shall retain the accounts 7 months, 3 days and 10 hours only", sequence, out values));
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(DateTime.MinValue, values[0].times[0]);
            Assert.AreEqual(new TimeSpan(213, 10, 0, 0).ToString(), values[0].Value);


            Assert.IsTrue(LineageLibrary.Match(" the company shall retain the accounts 5 months", sequence, out values));
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(DateTime.MinValue, values[0].times[0]);
            Assert.AreEqual(new TimeSpan(150, 0, 0, 0).ToString(), values[0].Value);

            Assert.IsTrue(LineageLibrary.Match(" the company shall keep the documents for a week and then use them as toilet paper. ", sequence, out values));
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual(DateTime.MinValue, values[0].times[0]);
            Assert.AreEqual(new TimeSpan(7, 0, 0, 0).ToString(), values[0].Value);


            Assert.IsFalse(LineageLibrary.Match(" There is a house in North Ontario", sequence, out values));
            Assert.IsFalse(LineageLibrary.Match(" with dream comfort memories to share ", sequence, out values));
            Assert.IsFalse(LineageLibrary.Match(" in my mind I've still got a place to go ", sequence, out values));
            Assert.IsFalse(LineageLibrary.Match(" all my changes were there ", sequence, out values));
        }

        [TestMethod]
        public void TestTokenizerNumbersDates()
        {
            Assert.AreEqual(6, LineageLibrary.SimpleTokenizer("the car cost 1,000.56 pounds sterling").Count);
            Assert.AreEqual(5, LineageLibrary.SimpleTokenizer("the event occurred on 2/07/2015").Count);

        }

        [TestMethod]
        [Ignore] //fix this
        public void TestSequenceLineageMatchNumber()
        {
            var sequence = new List<List<string>> { new List<string> { "noun:01,2,07,10,13,4" }, new List<string> { "verb:019,005,4", "verb:063", "verb:207,0,1,17", "verb:222,01" }, new List<string> { "noun:01,4,18", "noun:00,1,00,3,10,09,07", "noun:01,4,09,01,3,3,0,8", "noun:01,4,04,02,21" }, new List<string> { "value:number,integer" } };
            List<DarlVar> values;
            Assert.IsTrue(LineageLibrary.Match(" the company shall retain the accounts 536", sequence, out values));
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("536", values[0].Value);
            Assert.AreEqual(536, values[0].values[0]);
            Assert.IsTrue(LineageLibrary.Match(" the company shall retain the accounts five hundred and thirty six", sequence, out values));
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("536", values[0].Value);
            Assert.AreEqual(536, values[0].values[0]);
            Assert.IsTrue(LineageLibrary.Match(" the company shall retain the accounts five hundred and thirty six days only", sequence, out values));
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("536", values[0].Value);
            Assert.AreEqual(536, values[0].values[0]);
            Assert.IsTrue(LineageLibrary.Match(" the company shall retain the accounts one million, three hundred and ten thousand, four hundred and twenty two days", sequence, out values));
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("1310422", values[0].Value);
            Assert.AreEqual(1310422, values[0].values[0]);

        }

        [TestMethod]
        [Ignore]//fix this
        public void TestSequenceLineageMatchLocation()
        {
            var sequence = new List<List<string>> { new List<string> { "noun:01,2,07,10,13,4" }, new List<string> { "verb:019,005,4", "verb:063", "verb:207,0,1,17", "verb:222,01" }, new List<string> { "noun:01,4,18", "noun:00,1,00,3,10,09,07", "noun:01,4,09,01,3,3,0,8", "noun:01,4,04,02,21" }, new List<string> { "value:location" } };
            Assert.IsTrue(LineageLibrary.Match(" the company shall retain the accounts in London", sequence, out List<DarlVar> values));
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("london", values[0].Value, true);
            Assert.IsTrue(LineageLibrary.Match(" the company shall retain the accounts within the state of New York", sequence, out values));
            Assert.AreEqual(1, values.Count);
            Assert.AreEqual("New York", values[0].Value, true);

        }

        /// <summary>
        /// Illuminates strange case where the "phrase "is john" is recognized as a two word phrase with the same synsets as "is"
        /// </summary>
        [TestMethod]
        public void TestStrangeRecognition()
        {
            var res = LineageLibrary.LookupWord("i john");
            int nextDepth = 0;
            res = LineageLibrary.WordRecognizer(new List<string> { "is", "peter" }, ref nextDepth);
            Assert.AreEqual(1, nextDepth);
        }

        [TestMethod]
        public void SimilarityTest()
        {
            var res1 = LineageLibrary.Similarity("bum", "buggery");
            Assert.AreEqual(0.2857, res1, 0.001);
            var res2 = LineageLibrary.Similarity("poop", "whoop");
            var res3 = LineageLibrary.Similarity("john", "jon");
            var res4 = LineageLibrary.Similarity("lansmann", "lansman");
            var res5 = LineageLibrary.Similarity("mcdonel", "mcdonnell");
            var res6 = LineageLibrary.Similarity("mcdonell", "mcdonnell");
        }

        [TestMethod]
        public void SymSpellSimilarityTest()
        {
            var res1 = LineageLibrary.SimilarWordSuggestions("conected", 2);
            Assert.AreEqual(2, res1.Count);
            Assert.AreEqual("connected", res1[0].term);
            Assert.AreEqual("convected", res1[1].term);
        }

        [TestMethod]
        public void TestCheckLineage()
        {
            Assert.IsTrue(LineageLibrary.CheckLineage("value:"));
            Assert.IsTrue(LineageLibrary.CheckLineage("value:text"));
            Assert.IsFalse(LineageLibrary.CheckLineage("value:poop"));
            Assert.IsTrue(LineageLibrary.CheckLineage("noun:99,7574"));
            Assert.IsTrue(LineageLibrary.CheckLineage("auxiliary_verb:18"));
            Assert.IsFalse(LineageLibrary.CheckLineage("noun:44,bum"));
            Assert.IsFalse(LineageLibrary.CheckLineage("verb:"));
            Assert.IsFalse(LineageLibrary.CheckLineage("noun:99, 7574"));
            Assert.IsTrue(LineageLibrary.CheckLineage("terminus:"));
        }
    }
}
