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
            configuration.Setup(a => a[It.Is<string>(s => s == "Licensing:publicLicenseGeneratorKey")]).Returns("MFkwEwYHKoZIzj0CAQYIKoZIzj0DAQcDQgAEv5iZM5k8XaSHaEg7g7IBAQKAGgdjt5ePjWXWJwLJnYgiotX/uYt4uKrimOsz5jR5U5b+sG+EuT9d3hHRZld/UQ==");
            configuration.Setup(a => a[It.Is<string>(s => s == "Licensing:privateLicensePassPhrase")]).Returns("excaliber");
            configuration.Setup(a => a[It.Is<string>(s => s == "Licensing:privateLicenseGeneratorKey")]).Returns("MHcwIwYKKoZIhvcNAQwBAzAVBBB3+q8AI4Ce3T0S9Bc3wSIkAgEKBFDM5PVY0MLQXN2ii1X7zsi6y/WBYiMgRa54eZndt6PFxBAuzod04TLuKKcTRMi//VhZPw31fWJNb6oeZl3LAO1BonlEXsNw9FCemjLbpGJPiA==");
             var licensing = new ProductLicensing(configuration.Object);
            var license = licensing.CreateKey(DateTime.Now + new TimeSpan(1000, 0, 0, 0), "Dr Andy's IP Ltd", "support@darl.ai");
            Assert.IsTrue(licensing.CheckKey(license));
            license = licensing.CreateKey(DateTime.Now - new TimeSpan(30, 0, 0, 0), "farts ltd", "poop@farts.com");
            Assert.IsFalse(licensing.CheckKey(license));
            Assert.IsFalse(licensing.CheckKey("uggawuggabugga"));
        }
    }
}
