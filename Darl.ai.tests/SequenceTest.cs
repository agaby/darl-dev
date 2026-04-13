/// </summary>

﻿using DarlLanguage.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace DarlLanguage.Test
{
    [TestClass]
    public class SequenceTest
    {
        [TestMethod]
        public void TestSequenceFind()
        {
            var p = new List<List<int>> { new List<int> { 1, 2, 3 }, new List<int> { 4, 5, 6 }, new List<int> { 7, 8, 9 } };
            var q = new List<List<int>> { new List<int> { 2 }, new List<int> { 4 }, new List<int> { 9 } };
            var s = new List<List<int>> { new List<int> { 4 }, new List<int> { 4 }, new List<int> { 9 } };
            var t = new List<List<int>> { new List<int> { 10, 20, 30 }, new List<int> { 1, 2, 3 }, new List<int> { 4, 5, 6 }, new List<int> { 7, 8, 9 } };
            var r = new Sequence<int>();
            Assert.IsTrue(r.Find(p, q, new IntComparer()));
            Assert.IsFalse(r.Find(p, s, new IntComparer()));
            Assert.IsFalse(r.Find(q, s, new IntComparer()));
            Assert.IsTrue(r.Find(t, q, new IntComparer()));
        }
    }
    public class IntComparer : IComparer<int>
    {
        public int Compare(int x, int y)
        {
            if (x > y)
                return 1;
            if (x == y)
                return 0;
            return -1;
        }
    }
}
