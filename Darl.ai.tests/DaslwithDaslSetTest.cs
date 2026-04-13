/// <summary>
/// </summary>

﻿using DarlCommon;
using Dasl.TemporalDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Darl_standard_core.test
{
    /// <summary>
    /// Summary description for DaslwithDaslSetTest
    /// </summary>
    [TestClass]
    public class DaslwithDaslSetTest
    {
        public DaslwithDaslSetTest()
        {
            //
            // TODO: Add constructor logic here
            //
        }

        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public async Task TestDaslSetWithDasl()
        {
            string source =
            @"ruleset logisticMap
			{
				input numeric in1 ;
				output numeric out1;
				if anything then out1 will be 4 * in1 * ( 1 - in1);
			}
			mapoutput map1;
			mapinput in1;
			wire logisticMap.out1 map1;
			wire in1 logisticMap.in1;
			delay map1 logisticMap.in1 {0};
			";
            var runtime = new DaslLanguage.DaslRunTime();
            var tree = runtime.CreateTree(source);
            Assert.IsFalse(tree.HasErrors());
            var history = new DaslSet();
            var state1 = new DaslState { timeStamp = new DateTime(2016, 1, 1, 0, 0, 0), values = new List<DarlVar> { new DarlVar { name = "in1", Value = "0.3" } } };
            var state2 = new DaslState { timeStamp = new DateTime(2016, 1, 1, 0, 0, 1), values = new List<DarlVar>() };
            var state3 = new DaslState { timeStamp = new DateTime(2016, 1, 1, 0, 1, 20), values = new List<DarlVar>() };
            history.events.Add(state1);
            history.events.Add(state2);
            history.events.Add(state3);
            history.sampleTime = new TimeSpan(0, 0, 1);
            File.WriteAllText("initialhistory.json", JsonConvert.SerializeObject(history, Formatting.Indented));
            var el = new EventList();
            el.events = history.events;
            el.sample = history.sampleTime;
            var sampled = el.SampleData();
            var res = await runtime.Simulate(sampled, sampled.Count, tree);
            history.events = el.ConvertToEvents(res);
            File.WriteAllText("finalhistory.json", JsonConvert.SerializeObject(history, Formatting.Indented));
        }
    }
}
