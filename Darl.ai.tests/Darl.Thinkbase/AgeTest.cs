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
            res1.Normalise(false,true);
            res2 = new DarlResult(DataType.temporal, 1.0);
            res2.values.Add(2.0);
            res2.Normalise(false,true);
            res3 = Age(res1, res2);
            Assert.IsTrue(res3.IsUnknown());
            res1 = new DarlResult(DataType.temporal, 1.0);
            res1.values.Add(1.0);
            res1.values.Add(2.0);
            res1.Normalise(false,true);
            res2 = new DarlResult(DataType.temporal, 1.0);
            res2.values.Add(1.5);
            res2.values.Add(3.0);
            res2.Normalise(false,true);
            res3 = Age(res1, res2);
            Assert.AreEqual(2,res3.values.Count());
            Assert.AreEqual(0.0, res3.values[0]);
            Assert.AreEqual(2.0, res3.values[1]);
            res2 = new DarlResult(DataType.temporal, 1.0);
            res2.values.Add(3.0);
            res2.Normalise(false,true);
            res3 = Age(res1, res2);
            Assert.IsTrue(res3.IsUnknown());
            res1 = new DarlResult(DataType.temporal, 1.0);
            res1.values.Add(1.0);
            res1.values.Add(2.0);
            res1.values.Add(3.0);
            res1.Normalise(false);
            res2 = new DarlResult(DataType.temporal, 1.0);
            res2.values.Add(2.0);
            res2.Normalise(false,true);
            res3 = Age(res1, res2);
            Assert.AreEqual(3, res3.values.Count());
            Assert.AreEqual(0.0, res3.values[0]);
            Assert.AreEqual(0.0, res3.values[1]);
            Assert.AreEqual(1.0, res3.values[2]);
            res2 = new DarlResult(DataType.temporal, 1.0);
            res2.values.Add(1.8);
            res2.Normalise(false, true);
            res3 = Age(res1, res2);
            Assert.AreEqual(3, res3.values.Count());
            Assert.AreEqual(0.0, res3.values[0]);
            Assert.AreEqual(0.0, res3.values[1]);
            Assert.AreEqual(0.8, res3.values[2]); 
            res1.values.Add(4.0);
            res1.Normalise(false);
            res2.values.Clear();
            res2.values.Add(3.0);
            res2.Normalise(false,true);
            res3 = Age(res1, res2);
            Assert.AreEqual(4, res3.values.Count());
            Assert.AreEqual(0.0, res3.values[0]);
            Assert.AreEqual(0.0, res3.values[1]);
            Assert.AreEqual(1.0, res3.values[2]);
            Assert.AreEqual(2.0, res3.values[3]);
            res2.values.Clear();
            res2.values.Add(3.2);
            res2.Normalise(false,true);
            res3 = Age(res1, res2);
            Assert.AreEqual(4, res3.values.Count());
            Assert.AreEqual(0.0, res3.values[0]);
            Assert.AreEqual(0.2, (double)res3.values[1], 0.001);
            Assert.AreEqual(1.2, (double)res3.values[2], 0.001);
            Assert.AreEqual(2.2, (double)res3.values[3], 0.001);
        }
    }
}
