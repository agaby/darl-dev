/// </summary>

﻿using DarlLanguage.Processing;
using DaslLanguage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Darl_standard_core.test
{
    [TestClass]
    public class DaslRunTimeTest
    {
        [TestMethod, TestCategory("Dasl")]
        public async Task TestSimulate1()
        {
            var data = new List<List<DarlResult>>();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.Multivibrator.dasl"));
            data.Add(new List<DarlResult>());
            data[0].Add(new DarlResult("in1", "true"));
            var dasl = new DaslRunTime();
            var tree = dasl.CreateTree(reader.ReadToEnd());
            var results = await dasl.Simulate(data, 100, tree);
            Assert.AreEqual(100, results.Count());
            for (int n = 0; n < 100; n++)
            {
                Assert.AreEqual(n % 2 < 1 ? "false" : "true", (string)results[n].First(a => a.name == "map1").Value);
            }
        }


        [TestMethod, TestCategory("Dasl")]
        public async Task TestLogistic()
        {
            string code =
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
            var data = new List<List<DarlResult>>();
            data.Add(new List<DarlResult>());
            data[0].Add(new DarlResult("in1", 0.3));
            var dasl = new DaslRunTime();
            var tree = dasl.CreateTree(code);
            var results = await dasl.Simulate(data, 100, tree);
            Assert.AreEqual(0.84, (double)results[0].First(a => a.name == "map1").Value, 0.01);
            Assert.AreEqual(0.5376, (double)results[1].First(a => a.name == "map1").Value, 0.01);
            Assert.AreEqual(0.9943, (double)results[2].First(a => a.name == "map1").Value, 0.01);
            Assert.AreEqual(0.0224, (double)results[3].First(a => a.name == "map1").Value, 0.01);

        }

    }
}
