/// <summary>
/// </summary>

﻿using DarlLanguage;
using DarlLanguage.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Darl_standard_core.test
{
    [TestClass]
    public class ArrayTests
    {

        [TestMethod]
        public async Task TestImplicitAggregation()
        {
            DarlRunTime runtime = new DarlRunTime();
            var reader = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.brexit.darl"));
            string source = reader.ReadToEnd();
            var tree = runtime.CreateTree(source);
            var results = new List<DarlResult>
            {
                new DarlResult("choose[0]","yes") ,
                new DarlResult("choose[1]","yes"),
                new DarlResult("choose[2]","no")
            };
            var res = await runtime.Evaluate(tree, results);
        }

        [TestMethod]
        public void TestExplicitAggregation()
        {

        }


    }
}
