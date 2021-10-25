using System;
using System.Collections.Generic;
using System.Text;
using DarlLanguage;
using DarlLanguage.Processing;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Darl_standard_core.test
{

 

    [TestClass]
    public class SecurityTest
    {

        private static readonly string privateKey = "MHcwIwYKKoZIhvcNAQwBAzAVBBB3+q8AI4Ce3T0S9Bc3wSIkAgEKBFDM5PVY0MLQXN2ii1X7zsi6y/WBYiMgRa54eZndt6PFxBAuzod04TLuKKcTRMi//VhZPw31fWJNb6oeZl3LAO1BonlEXsNw9FCemjLbpGJPiA==";

        private static readonly string passPhrase = "excaliber";




        [TestMethod]
        [Ignore]
        public void TestGenerateKeyPair()
        {
            var keyGenerator = Standard.Licensing.Security.Cryptography.KeyGenerator.Create();
            var keyPair = keyGenerator.GenerateKeyPair();
            var privateKey = keyPair.ToEncryptedPrivateKeyString("excaliber");
            var publicKey = keyPair.ToPublicKeyString();
        }

        [TestMethod]
        public void TestGenerateLicense()
        {
            var uid = Guid.NewGuid();
            var license = License.New()
    .WithUniqueIdentifier(uid)
    .As(LicenseType.Standard)
    .ExpiresAt(DateTime.Now.AddDays(1000))
    .LicensedTo("Dr Andy's IP", "support@darl.ai")
    .CreateAndSignWithPrivateKey(privateKey, passPhrase);
            var licenseText = license.ToString();

            DarlRunTime.license = licenseText;
            DarlRunTime.installationId = uid.ToString();
             var runtime = new DarlRunTime(); //doesn't throw exception
        }

        [TestMethod]
        [Ignore]
        public void TestGenerateBadLicense()
        {
            Assert.ThrowsException<RuleException>(() =>
            {
                var uid = Guid.NewGuid();
                var license = License.New()
                    .WithUniqueIdentifier(uid)
                    .As(LicenseType.Trial)
                    .ExpiresAt(DateTime.Now.AddDays(30))
                    .LicensedTo("Dr Andy's IP", "support@darl.ai")
                    .CreateAndSignWithPrivateKey(privateKey, passPhrase);
                var licenseText = license.ToString();
                DarlRunTime.installationId = uid.ToString();
                DarlRunTime.license = licenseText;
                DarlRunTime.license = DarlRunTime.license.Replace("2018", "2019");
                DarlRunTime.license = DarlRunTime.license.Replace("Sun", "Mon");
                var runtime = new DarlRunTime();
            });

        }
    }
}
