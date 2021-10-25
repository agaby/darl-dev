using Darl.Licensing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl_standard_core.test
{
    [TestClass]
    public class LicenseTest
    {
        string testLicense = "YwEAAB+LCAAAAAAAAApVkNFOgzAUhu+X7B36AGYwzMhiukaoZNMADotL9K6DhnVSytqig6cXG3WanJs/5//ynRwY84I1mqHpBACY9y1DxNCmpKqEjo12EZ1brqjhskF5x67A3AMJ7YHnei5w3Rs7YJ3k0PnTtCTutJGCKZvGnFLBUFC3Byp4UwIi6+6rrEFsRqPdfjcjQXmNlKzHc2a65UzpWwsmI/jLzQopRqvtWqHzzwgJrxpqOsVQEmX4PlL7RX16KPfDM/ZfB5ZWFZZZdNA99fMAb4bHJ58Id/FGjwEPEy8Ot8NOvJzINuyDI45zI8l6Fyzfe3m9ufPwR5Fmy3NmqtUKOhfXdAKdn89+Ao5T0p5jAQAA";
        [TestMethod]
        [Ignore]
        public void TestLicense()
        {
            Assert.IsFalse(DarlLicense.licensed);
            DarlLicense.license = testLicense;
            Assert.IsTrue(DarlLicense.licensed);
            DarlLicense.license = "waggawaggabagga";
            Assert.IsFalse(DarlLicense.licensed);
        }
    }
}
