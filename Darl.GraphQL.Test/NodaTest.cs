/// </summary>

﻿using Darl.GraphQL.Models.Models.Noda;
using Darl.GraphQL.Process.Models.Noda.Layout;
using Darl.Thinkbase;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using NJsonSchema;
using System.IO;
using System.Reflection;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class NodaTest
    {

        [TestMethod]
        public void ImportNodaTest()
        {
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.nodaExample.json"));
            var doc = docsource.ReadToEnd();
            var nd = JsonConvert.DeserializeObject<NodaDocument>(doc);
            var fd = new ForceDirected3D(nd, 81.76, 40000.0, 0.5);
            var bb = fd.GetBoundingBox();
            var diagonal = bb.topRightBack - bb.bottomLeftFront;
            var length = diagonal.Magnitude();
        }
        [TestMethod]
        public void CreateSchemaTest()
        {
            var schema = JsonSchema.FromType<GraphObject>();
            var schemaJson = schema.ToJson();
            File.WriteAllText("GraphObject_Schema.json", schemaJson);
        }
    }
}
