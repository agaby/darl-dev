using Darl.GraphQL.Models.Connectivity;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
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
            //This needs dummy public and private keys
            var configuration = new Mock<IConfiguration>();
            var licensing = new ProductLicensing(configuration.Object);
            var license = licensing.CreateKey(DateTime.Now + new TimeSpan(30, 0, 0, 0), "farts ltd", "poop@farts.com");
            Assert.IsTrue(licensing.CheckKey(license));
            license = licensing.CreateKey(DateTime.Now - new TimeSpan(30, 0, 0, 0), "farts ltd", "poop@farts.com");
            Assert.IsFalse(licensing.CheckKey(license));
            Assert.IsFalse(licensing.CheckKey("uggawuggabugga"));
        }
    }
}
