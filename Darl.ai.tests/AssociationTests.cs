/// <summary>
/// </summary>

﻿using DarlLanguage;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;

namespace Darl_standard_core.test
{
    [TestClass]
    public class AssociationTests
    {
        [TestMethod]
        [Ignore]
        public async Task CreateTrainingSet()
        {
            var str = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.groceries.csv"));
            string line = string.Empty;
            var groceries = new Groceries();
            while (!string.IsNullOrEmpty((line = str.ReadLine())))
            {
                var items = new List<string>();
                items.AddRange(line.Split(','));
                groceries.baskets.Add(new Basket { items = items });
            }
            var json = JsonConvert.SerializeObject(groceries);
            await File.WriteAllTextAsync(@"C:\Users\Andrew\documents\visual studio 2017\Projects\Darl_standard\Darl_standard_core.test\groceries.json", json);
        }

        [TestMethod]
        public async Task TestGroceries()
        {
            var code = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.groceries.darl")).ReadToEnd();
            var data = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.ai.tests.groceries.json")).ReadToEnd();
            var runtime = new DarlRunTime();
            var darl = await runtime.MineAssociationAsync(code, data);
        }
    }

    public class Basket
    {
        public List<string> items;
    }

    public class Groceries
    {
        public List<Basket> baskets = new List<Basket>();
    }
}
