using Darl_standard.Darl.Meta;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Darl_standard_core.test
{
    [TestClass]
    public class DarlMetaTests
    {

        [TestMethod]
        public void TestSimpleMDarl()
        {
            var str = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl_standard_core.test.ParkrJob.mdarl"));
            var source = str.ReadToEnd();
            var runtime = new DarlMetaRunTime();
            var tree = runtime.CreateTree(source);
        }
    }
}
