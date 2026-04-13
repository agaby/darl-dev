/// <summary>
/// </summary>

﻿using DarlCommon;
using Dasl.TemporalDb;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darl_standard_core.test
{
    [TestClass]
    public class EventListTests
    {
        [TestMethod]
        public void TestTemporalDbSorting()
        {
            var source = new List<DaslState>();
            for (int n = 1; n < 31; n++)
                source.Add(new DaslState { timeStamp = new DateTime(2015, 11, n), values = new List<DarlVar> { new DarlVar { Value = n.ToString(), name = "count" } } });
            source.Reverse();
            var tdb = new EventList();
            tdb.events = source;
            tdb.sample = new TimeSpan(1, 0, 0, 0);
            Assert.AreEqual(new DateTime(2015, 11, 1), tdb.start);
            Assert.AreEqual(new DateTime(2015, 11, 30), tdb.end);
            var res = tdb.SampleData();
            Assert.AreEqual(30, res.Count);
            for (int n = 1; n < 31; n++)
            {
                Assert.AreEqual(n.ToString(), res[n - 1].First(a => a.name == "count").values[0].ToString());
            }
        }

        [TestMethod]
        public void TestMinimalTemporalDB()
        {
            var tdb = new EventList();
            var array = new DaslState[2];//simulate over 100 time periods of one second
            var startStamp = DateTime.Now;
            array[0] = new DaslState { timeStamp = startStamp, values = new List<DarlVar> { new DarlVar { name = "in1", Value = "0.3" } } };
            array[1] = new DaslState { timeStamp = startStamp + new TimeSpan(0, 1, 40), values = new List<DarlVar>() };

            tdb.events = array.ToList();
            tdb.sample = new TimeSpan(10000000L);

            var res = tdb.SampleData();
            Assert.AreEqual(101, res.Count);
            var events = tdb.ConvertToEvents(res);
            Assert.AreEqual(101, events.Count);
            Assert.AreEqual(startStamp, events[0].timeStamp);
            Assert.AreEqual(startStamp + new TimeSpan(0, 1, 40), events.Last().timeStamp);
        }
    }
}
