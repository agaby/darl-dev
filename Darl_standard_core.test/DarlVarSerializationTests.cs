using DarlCommon;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Darl_standard_core.test
{
    [TestClass]
    public class DarlVarSerializationTests
    {

        [TestMethod]
        public void TestSerialization()
        {
            var dv = new DarlVar { dataType = DarlVar.DataType.sequence, sequence = new List<List<string>> { new List<string> { "a", "b", "c" }, new List<string> { "d", "e" } } };
            DarlVar dv2;
            using (MemoryStream ms = new MemoryStream())
            {
                Serializer.Serialize(ms, dv);
                ms.Position = 0;
                dv2 = Serializer.Deserialize<DarlVar>(ms);
            }
            Assert.AreEqual("a", dv2.sequence[0][0]);
            Assert.AreEqual("b", dv2.sequence[0][1]);
            Assert.AreEqual("c", dv2.sequence[0][2]);
            Assert.AreEqual("d", dv2.sequence[1][0]);
            Assert.AreEqual("e", dv2.sequence[1][1]);
            Assert.AreEqual(DarlVar.DataType.sequence, dv2.dataType);
        }
    }
}
