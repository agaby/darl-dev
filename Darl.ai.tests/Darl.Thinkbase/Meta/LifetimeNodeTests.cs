using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using System.Collections.Generic;

namespace Darl.Thinkbase.Meta.Tests
{
    [TestClass()]
    public class LifetimeNodeTests
    {

        IGraphModel _model;

        [TestMethod()]
        public void InitTest()
        {
            DarlMetaRunTime.SetLicense("RwEAAB+LCAAAAAAAAApVkEtPwzAQhO+V+h984xCEyauUyrVoHqKOkjQlUVRxc4mhLnk6sUr49UQWCDh+s7Mzq0Uhf2F1z/B8BgDKxpbhdKB1QUWBoEI18D9aLujAmxpnJ3kNdBMEsgbGrWEB3VhZ9sq4B49RhuAfp9p0ZT80FROKJo5pxbAnwKYuxqsekASEw1Sl5G+LX1Fe4l62bSOGh+mU8obyKVnJKhT+S0Upf6vpIAXDkb93iR/LT+891+y83bsmLM6tvnBOZ/J6l5Ry8WQcD2PwrBVenmsb7ljEHHfJznMO22WXdHpXenlIuN4E8SKNDMs+biNikiVrLus1gr9d8xmCP9/7AhubQj1HAQAA");
            var model = new Mock<IGraphModel>();
            model.Setup(a => a.GetLineages(It.IsAny<GraphElementType>())).Returns(new List<Lineage.LineageRecord>());
            _model = model.Object;
            var source = "output categorical fred {true,false};\n" +
                "duration thirtydays 30.00:00:00.0;\n" +
                "if anything then fred will be true for thirtydays;\n";
            var runtime = new DarlMetaRunTime();
            var tree = runtime.CreateTree(source, new GraphObject(), _model);
            Assert.IsFalse(tree.HasErrors());
        }
    }
}