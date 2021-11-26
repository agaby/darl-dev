using Darl.Thinkbase.Meta;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Darl.Thinkbase.Meta.DarlResult;

namespace Darl.ai.tests
{
    [TestClass()]
    public  class AgeTest
    {
        [TestMethod()]
        public void SimpleTests()
        {
            var res1 = new DarlResult(DataType.temporal, 0.0);
            var res2 = new DarlResult(DataType.temporal, 0.0);
            var res3 = Age(res1, res2);
            Assert.IsTrue(res3.IsUnknown());
            res1 = new DarlResult(DataType.temporal, 1.0);
            res1.values.Add(1.0);
            res1.Normalise(false);
            res2 = new DarlResult(DataType.temporal, 1.0);
            res2.values.Add(2.0);
            res2.Normalise(false);
            res3 = Age(res1, res2);
            Assert.IsTrue(res3.IsUnknown());
            res1 = new DarlResult(DataType.temporal, 1.0);
            res1.values.Add(1.0);
            res1.values.Add(2.0);
            res1.Normalise(false);
            res2 = new DarlResult(DataType.temporal, 1.0);
            res2.values.Add(1.5);
            res2.values.Add(3.0);
            res2.Normalise(false);
            res3 = res2 - res1;
            res3.Normalise(true);
            res3 = Age(res1, res2);
            Assert.AreEqual(2,res3.values.Count());
            Assert.AreEqual(0.5, res3.values[0]);
            Assert.AreEqual(1.0, res3.values[1]);
            res2 = new DarlResult(DataType.temporal, 1.0);
            res2.values.Add(3.0);
            res2.Normalise(false);
            res3 = Age(res1, res2);
            Assert.IsTrue(res3.IsUnknown());
            res1 = new DarlResult(DataType.temporal, 1.0);
            res1.values.Add(1.0);
            res1.values.Add(2.0);
            res1.values.Add(3.0);
            res1.Normalise(false);
            res2 = new DarlResult(DataType.temporal, 1.0);
            res2.values.Add(2.0);
            res2.Normalise(false);
            res3 = res2 - res1;
            res3.Normalise(true);
            res3 = Age(res1, res2);

        }
    }
}
