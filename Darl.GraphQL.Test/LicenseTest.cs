using Darl.GraphQL.Models.Connectivity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class LicenseTest
    {

        [TestMethod]
        public void TestLicenseCreation()
        {
            var licensing = new ProductLicensing();
            var license = licensing.CreateKey(DateTime.Now + new TimeSpan(30, 0, 0, 0), "farts ltd", "poop@farts.com");
            Assert.IsTrue(licensing.CheckKey(license));
            license = licensing.CreateKey(DateTime.Now - new TimeSpan(30, 0, 0, 0), "farts ltd", "poop@farts.com");
            Assert.IsFalse(licensing.CheckKey(license));
            Assert.IsFalse(licensing.CheckKey("uggawuggabugga"));
        }
    }
}
