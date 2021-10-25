using Darl.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace Darl.Thinkbase.Tests
{
    [TestClass()]
    public class DarlTimeTests
    {
        [TestMethod()]
        public void DarlTimeTest()
        {
            var dt = new DarlTime(-10);
            Assert.AreEqual(DarlTime.secondsPerYear, dt.precision);
            Assert.AreEqual(DarlTime.secondsPerYear * -11, dt.raw);
            Assert.AreEqual(DateTime.MinValue, dt.dateTime);
            dt = new DarlTime(-10, 2); //summer, 10BC
            Assert.AreEqual(DarlTime.secondsPerYear / 4.0, dt.precision);
            Assert.AreEqual((DarlTime.secondsPerYear * -11) + DarlTime.secondsPerYear * 0.5, dt.raw);
            Assert.AreEqual(DateTime.MinValue, dt.dateTime);
            dt = new DarlTime(1955, 11, 06, 17, 15); //ANE Birth
            Assert.AreEqual(new DateTime(1955, 11, 06, 17, 15, 0), dt.dateTime);
        }
    }
}