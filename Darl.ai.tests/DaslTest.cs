/// <summary>
/// </summary>

﻿using CsvHelper;
using DarlCommon;
using DarlCompiler.Parsing;
using Dasl.TemporalDb;
using DaslLanguage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Darl_standard_core.test
{
    [TestClass]
    public class DaslTest
    {
        [TestMethod]
        public void TestDaslConstructor()
        {
            var dasl = new DaslGrammar();
        }

        [TestMethod]
        public void TestDaslParse()
        {
            LanguageData language = new LanguageData(new DaslGrammar());
            Parser parser = new Parser(language);
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Multivibrator.dasl"));
            ParseTree parseTree = parser.Parse(reader.ReadToEnd());
            ParseTreeNode root = parseTree.Root;
            Assert.IsNotNull(root);
        }

        [TestMethod]
        public async Task TestTradingExample() //depends on csvhelper
        {
            var initial_balance = "10000";
            //get the data and ruleset from the exe
            var csv = new CsvReader(new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.GBP_USD.csv")), System.Globalization.CultureInfo.InvariantCulture);
            var records = csv.GetRecords<TradingRecord>().ToList();
            var code = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.trading_simulation.darl")).ReadToEnd();
            //convert the csv records to a DaslSet.
            var history = new DaslSet();
            history.sampleTime = new TimeSpan(1, 0, 0, 0); // 1 day
            history.events = new List<DaslState>();
            foreach (var r in records)
            {
                if (r == records.Last()) //records are in reverse order, set the initial value of the balance and add the first price
                    history.events.Add(new DaslState { timeStamp = DateTime.Parse(r.Date), values = new List<DarlVar> { new DarlVar { name = "price", Value = r.Price, dataType = DarlVar.DataType.numeric }, new DarlVar { name = "balance", dataType = DarlVar.DataType.numeric, Value = initial_balance } } });
                else // add the day's price
                    history.events.Add(new DaslState { timeStamp = DateTime.Parse(r.Date), values = new List<DarlVar> { new DarlVar { name = "price", Value = r.Price, dataType = DarlVar.DataType.numeric } } });
            }
            DaslRunTime sruntime = new DaslRunTime();
            var tree = sruntime.CreateTree(code);
            Assert.IsFalse(tree.HasErrors());
            var el = new EventList();
            el.events = history.events;
            el.sample = history.sampleTime;

            var sampled = el.GetEventData();
            var res = await sruntime.Simulate(sampled, sampled.Count, tree);
            history.events = el.ConvertToEvents(res);
            Assert.AreEqual(new DateTime(2016, 06, 30), history.events[0].timeStamp);
            Assert.AreEqual(new DateTime(2018, 06, 29), history.events[525].timeStamp);
            Assert.AreEqual("9518.25432962368", history.events[525].values.Where(a => a.name == "sterling").First().Value);
        }


        public class TradingRecord
        {
            public string Date { get; set; }
            public string Price { get; set; }
            public string Open { get; set; }
            public string High { get; set; }
            public string Low { get; set; }
            public string Change { get; set; }
        }
    }
}
