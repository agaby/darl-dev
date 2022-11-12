using Darl.Licensing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System;

namespace Darl.ai.tests
{
    [TestClass]
    public class LicenseTest
    {
        private IConfiguration _config;


        [TestInitialize()]
        public void Initialize()
        {
            _config = new ConfigurationBuilder()
                .AddUserSecrets<LicenseTest>()
                .Build();
        }

        [TestMethod]
        public void TestLicenseCreation()
        {
            //This needs dummy public and private keys
            var logger = new Mock<ILogger<ProductLicensing>>();
            var licensing = new ProductLicensing(_config, logger.Object);
            var license = licensing.CreateKey(DateTime.Now + new TimeSpan(1000, 0, 0, 0), "Dr Andy's IP Ltd", "support@darl.ai");
            Assert.IsTrue(licensing.CheckKey(license));
            license = licensing.CreateKey(DateTime.Now - new TimeSpan(30, 0, 0, 0), "farts ltd", "poop@farts.com");
            Assert.IsFalse(licensing.CheckKey(license));
            Assert.IsFalse(licensing.CheckKey("uggawuggabugga"));
        }
    }
}
