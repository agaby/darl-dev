using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using Darl.Thinkbase;
using DarlCommon;
using DarlLanguage.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using QuickGraph;
using QuickGraph.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class BotRegressionTests
    {

        private ILocalStore _graphStore;
        private IConfiguration _config;
        private IBotProcessing _bot;
        private IConnectivity _conv;
        private IRuleFormInterface _rform;
        private IGraphProcessing _graph;

        private readonly string songLineage = "noun:01,4,14,1,10,33";
        private readonly string artistLineage = "noun:00,2,00,015,01";
        private readonly string followedByLineage = "verb:429";
        private readonly string sungByLineage = "verb:034,30,01,17,40";
        private readonly string writtenByLineage = "verb:023,36";
        private readonly string songTypeLineage = "noun:01,0,0,15,07,02,02,0,01";
        private readonly string performanceCountLineage = "noun:01,5,04,3,07";

        [TestInitialize()]
        public void Initialize()
        {
            var configuration = new Mock<IConfiguration>();
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinHostname")]).Returns("thinkbase.gremlin.cosmosdb.azure.com");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinPort")]).Returns("443");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinAuthKey")]).Returns("ffWKZWMJro4JHBaJAi4yG1o35ujaDvj0pIkrqsYEz4hCoHR9jvHr6YR3Pb2dxr8rw4obuO4ZvnJetejwJyrYQA==");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinDatabase")]).Returns("farleft");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinCollection")]).Returns("hypernymy");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinCollection")]).Returns("7d1a254f-d405-4385-acbc-308c8376f2e3");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("d70f1008-5758-41b5-9c44-bc90535aeabc");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("e438440e-9d90-46e8-87ed-080e19c43aed");
            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("786e46c2-fa33-4124-af67-1bb14625c216");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("5ee43551-c05c-4cff-8582-c08f23f84c14");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinLocation")]).Returns("azure");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinLocation")]).Returns("local");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevUrl")]).Returns("https://darl.dev/graphql/");
            configuration.Setup(a => a[It.Is<string>(s => s == "botmodel")]).Returns("thousandquestions.model");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:MongoConnectionString")]).Returns("mongodb://darlai:VqbEsCyGXAkTlWUuOb3Y4RQbFqmZs3VZaAWDtYrDO3054dicQHbWMo2OhbabuWK4szQM5VmgoUHe8jGihAIWdQ==@darlai.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:boaiuserid")]).Returns("8c663676-a7dc-4561-af3d-89b38555837d");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:MongoDatabase")]).Returns("darlai");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:BlobContainer")]).Returns("darldevgraphs");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StorageConnectionString")]).Returns("DefaultEndpointsProtocol=https;AccountName=darlai;AccountKey=errnwefiVeXcDr0aKbHDxXjblOQhwFwHkeG4qR4caChkABnzp9MNeBBX0FP1jc4DnXPGztI67pbEBXDqA1dPCw==");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:GraphContainer")]).Returns("darldevgraphs");




            _config = configuration.Object;
            var logger = new Mock<ILogger<GraphLocalStore>>();
            var botLogger = new Mock<ILogger<BotProcessing>>();
            var connLogger = new Mock<ILogger<CosmosDBConnectivity>>();
            var blobLogger = new Mock<ILogger<BlobConnectivity>>();
            var bgplogger = new Mock<ILogger<BlobGraphPrimitives>>();
            var glogger = new Mock<ILogger<GraphProcessing>>();
            var context = new Mock<IHttpContextAccessor>();
            var cache = new Mock<IDistributedCache>();
            context.Setup(a => a.HttpContext.User.Identity.Name).Returns(_config["userId"]);
            var licensing = new Mock<ILicensing>();
            _conv = new CosmosDBConnectivity(_config, connLogger.Object, licensing.Object, cache.Object);
            var trigger = new Mock<ITrigger>();
            var bc = new BlobGraphConnectivity(_config, blobLogger.Object);
            var conn = new Mock<IConnectivity>();
            var meta = new Mock<IMetaStructureHandler>();
            var trans = new Mock<IKGTranslation>();
            var blob = new BlobGraphPrimitives(new List<IBlobConnectivity>{ bc },cache.Object, conn.Object, bgplogger.Object);
            _graph = new GraphProcessing(blob, glogger.Object,meta.Object);
            _graphStore = new GraphLocalStore(configuration.Object, logger.Object, context.Object, _graph);
            var ghandler = new Mock<IGraphHandler>();
            var dg = new Mock<IDistributedCache>();
            _bot = new BotProcessing(_conv, botLogger.Object, _config, _graph, ghandler.Object,dg.Object);
        }




        /// <summary>
        /// Load grateful dead graph
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [Ignore]
        public async Task TestLoadGraphMLForTesting()
        {
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.grateful-dead.xml"));
            var ivf = new IdentifiableVertexFactory<SimpleVertex>(MakeVertex);
            var ief = new IdentifiableEdgeFactory<SimpleVertex, SimpleEdge>(MakeEdge);
            var graph = new TempGraph();
            graph.DeserializeFromGraphML<SimpleVertex, SimpleEdge, TempGraph>(docsource, ivf, ief);
            //now fill in vertices with lineages, etc.
            foreach(var v  in graph.vertices.Values)
            {
                if(v.labelV == "song")
                {
                    v.lineage = songLineage;
                }
                else if (v.labelV == "artist")
                {
                    v.lineage = artistLineage;
                }
                if(!string.IsNullOrEmpty(v.songType))
                    v.properties.Add(new GraphAttribute { name = songTypeLineage, value = v.songType });
                if(v.performances > 0)
                    v.properties.Add(new GraphAttribute { name = performanceCountLineage, value = v.performances.ToString() });
            }
            //same with edges
            foreach (var e in graph.edges.Values)
            {
                if (e.labelE == "followedBy")
                {
                    e.lineage = followedByLineage;
                }
                else if (e.labelE == "sungBy")
                {
                    e.lineage = sungByLineage;
                }
                else if (e.labelE == "writtenBy")
                {
                    e.lineage = writtenByLineage;
                }
            }
            //upload
            var nameIdLookup = new Dictionary<string, string>();
            var userId = _config["userId"];
            foreach (var lv in graph.vertices.Values)
            {
                var v = new GraphObjectInput { lineage = lv.lineage, name = lv.name ?? lv.id, externalId = lv.id, properties = lv.properties };
                try
                {
                    var res = await _graph.CreateGraphObject($"{userId}_grateful_dead_graph", v, OntologyAction.build);
                    nameIdLookup.Add(lv.id, res.id);
                }
                catch (Exception ex)
                {

                }
            }
            foreach (var le in graph.edges.Values)
            {
                var e = new Thinkbase.GraphConnectionInput { lineage = le.lineage, name = le.labelE, weight = le.weight, startId = nameIdLookup[le.Source.id], endId = nameIdLookup[le.Target.id] };
                try
                {
                    await _graph.CreateGraphConnection($"{userId}_grateful_dead_graph", e, OntologyAction.build);
                }
                catch (Exception ex)
                {

                }
            }
            await _graph.Store($"{userId}_grateful_dead_graph");
        }

        [TestMethod]
        public async Task TestShortestPath()
        { // "Networks and Algorithms, an introductory approach p154
            var userId = _config["userId"];
            string graphName = $"{userId}_simple_graph";
            var nameIdLookup = new Dictionary<string, string>();
            var res = await _graph.CreateGraphObject(graphName, new GraphObjectInput { name = "A", externalId = "A", lineage = "noun:00,0"}, OntologyAction.build);
            nameIdLookup.Add("A", res.id);
            res = await _graph.CreateGraphObject(graphName, new GraphObjectInput { name = "B", externalId = "B", lineage = "noun:00,0" }, OntologyAction.build);
            nameIdLookup.Add("B", res.id);
            res = await _graph.CreateGraphObject(graphName, new GraphObjectInput { name = "C", externalId = "C", lineage = "noun:00,0" }, OntologyAction.build);
            nameIdLookup.Add("C", res.id);
            res = await _graph.CreateGraphObject(graphName, new GraphObjectInput { name = "D", externalId = "D", lineage = "noun:00,0" }, OntologyAction.build);
            nameIdLookup.Add("D", res.id);
            res = await _graph.CreateGraphObject(graphName, new GraphObjectInput { name = "E", externalId = "E", lineage = "noun:00,0" }, OntologyAction.build);
            nameIdLookup.Add("E", res.id);
            res = await _graph.CreateGraphObject(graphName, new GraphObjectInput { name = "S", externalId = "S", lineage = "noun:00,0" }, OntologyAction.build);
            nameIdLookup.Add("S", res.id);
            res = await _graph.CreateGraphObject(graphName, new GraphObjectInput { name = "T", externalId = "T", lineage = "noun:00,0" }, OntologyAction.build);
            nameIdLookup.Add("T", res.id);
            await _graph.CreateGraphConnection(graphName, new GraphConnectionInput { weight = 7, startId = nameIdLookup["S"], endId = nameIdLookup["A"], lineage = "verb:248,02,01", name = "connects" }, OntologyAction.build);
            await _graph.CreateGraphConnection(graphName, new GraphConnectionInput { weight = 13, startId = nameIdLookup["S"], endId = nameIdLookup["B"], lineage = "verb:248,02,01", name = "connects" }, OntologyAction.build);
            await _graph.CreateGraphConnection(graphName, new GraphConnectionInput { weight = 28, startId = nameIdLookup["S"], endId = nameIdLookup["C"], lineage = "verb:248,02,01", name = "connects" }, OntologyAction.build);
            await _graph.CreateGraphConnection(graphName, new GraphConnectionInput { weight = 4, startId = nameIdLookup["A"], endId = nameIdLookup["B"], lineage = "verb:248,02,01", name = "connects" }, OntologyAction.build);
            await _graph.CreateGraphConnection(graphName, new GraphConnectionInput { weight = 10, startId = nameIdLookup["A"], endId = nameIdLookup["E"], lineage = "verb:248,02,01", name = "connects" }, OntologyAction.build);
            await _graph.CreateGraphConnection(graphName, new GraphConnectionInput { weight = 25, startId = nameIdLookup["A"], endId = nameIdLookup["D"], lineage = "verb:248,02,01", name = "connects" }, OntologyAction.build);
            await _graph.CreateGraphConnection(graphName, new GraphConnectionInput { weight = 5, startId = nameIdLookup["B"], endId = nameIdLookup["C"], lineage = "verb:248,02,01", name = "connects" }, OntologyAction.build);
            await _graph.CreateGraphConnection(graphName, new GraphConnectionInput { weight = 6, startId = nameIdLookup["B"], endId = nameIdLookup["D"], lineage = "verb:248,02,01", name = "connects" }, OntologyAction.build);
            await _graph.CreateGraphConnection(graphName, new GraphConnectionInput { weight = 3, startId = nameIdLookup["C"], endId = nameIdLookup["E"], lineage = "verb:248,02,01", name = "connects" }, OntologyAction.build);
            await _graph.CreateGraphConnection(graphName, new GraphConnectionInput { weight = 5, startId = nameIdLookup["D"], endId = nameIdLookup["T"], lineage = "verb:248,02,01", name = "connects" }, OntologyAction.build);
            await _graph.CreateGraphConnection(graphName, new GraphConnectionInput { weight = 12, startId = nameIdLookup["E"], endId = nameIdLookup["T"], lineage = "verb:248,02,01", name = "connects" }, OntologyAction.build);
            await _graph.Store(graphName);
            var result = await _graphStore.ReadAsync(new List<string> { "path", "simple_graph", "S", "T" });
            Assert.AreEqual("S -> connects -> A -> connects -> B -> connects -> D -> connects -> T", result.Value);
        }




        SimpleVertex MakeVertex(String id)
        {
            return new SimpleVertex { id = id };
        }

        SimpleEdge MakeEdge(SimpleVertex source, SimpleVertex target, String id)
        {
            //define lineage and label based on source and target
            var lineage = "";
            var name = "";
            lineage = "";
            return new SimpleEdge { id = id, Source = source, Target = target, lineage = lineage};
        }

        public class TempGraph : QuickGraph.IMutableVertexAndEdgeListGraph<SimpleVertex, SimpleEdge>
        {
            public Dictionary<string, SimpleVertex> vertices { get; set; } = new Dictionary<string, SimpleVertex>();
            public Dictionary<string, SimpleEdge> edges { get; set; } = new Dictionary<string, SimpleEdge>();

            public bool IsEdgesEmpty => throw new NotImplementedException();

            public int EdgeCount => throw new NotImplementedException();

            public IEnumerable<SimpleEdge> Edges => throw new NotImplementedException();

            public bool IsDirected => throw new NotImplementedException();

            public bool AllowParallelEdges => throw new NotImplementedException();

            public bool IsVerticesEmpty => throw new NotImplementedException();

            public int VertexCount => throw new NotImplementedException();

            public IEnumerable<SimpleVertex> Vertices => throw new NotImplementedException();

            public event VertexAction<SimpleVertex> VertexAdded;
            public event VertexAction<SimpleVertex> VertexRemoved;
            public event EdgeAction<SimpleVertex, SimpleEdge> EdgeAdded;
            public event EdgeAction<SimpleVertex, SimpleEdge> EdgeRemoved;

            public bool AddEdge(SimpleEdge edge)
            {
                edges.Add(edge.id, edge);
                return true;
            }

            public int AddEdgeRange(IEnumerable<SimpleEdge> edges)
            {
                throw new NotImplementedException();
            }

            public bool AddVertex(SimpleVertex v)
            {
                vertices.Add(v.id, v);
                return true;
            }

            public int AddVertexRange(IEnumerable<SimpleVertex> vertices)
            {
                throw new NotImplementedException();
            }

            public bool AddVerticesAndEdge(SimpleEdge edge)
            {
                throw new NotImplementedException();
            }

            public int AddVerticesAndEdgeRange(IEnumerable<SimpleEdge> edges)
            {
                throw new NotImplementedException();
            }

            public void Clear()
            {
                throw new NotImplementedException();
            }

            public void ClearOutEdges(SimpleVertex v)
            {
                throw new NotImplementedException();
            }

            public bool ContainsEdge(SimpleEdge edge)
            {
                throw new NotImplementedException();
            }

            public bool ContainsEdge(SimpleVertex source, SimpleVertex target)
            {
                throw new NotImplementedException();
            }

            public bool ContainsVertex(SimpleVertex vertex)
            {
                throw new NotImplementedException();
            }

            public bool IsOutEdgesEmpty(SimpleVertex v)
            {
                throw new NotImplementedException();
            }

            public int OutDegree(SimpleVertex v)
            {
                throw new NotImplementedException();
            }

            public SimpleEdge OutEdge(SimpleVertex v, int index)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<SimpleEdge> OutEdges(SimpleVertex v)
            {
                throw new NotImplementedException();
            }

            public bool RemoveEdge(SimpleEdge edge)
            {
                throw new NotImplementedException();
            }

            public int RemoveEdgeIf(EdgePredicate<SimpleVertex, SimpleEdge> predicate)
            {
                throw new NotImplementedException();
            }

            public int RemoveOutEdgeIf(SimpleVertex v, EdgePredicate<SimpleVertex, SimpleEdge> predicate)
            {
                throw new NotImplementedException();
            }

            public bool RemoveVertex(SimpleVertex v)
            {
                throw new NotImplementedException();
            }

            public int RemoveVertexIf(VertexPredicate<SimpleVertex> pred)
            {
                throw new NotImplementedException();
            }

            public void TrimEdgeExcess()
            {
                throw new NotImplementedException();
            }

            public bool TryGetEdge(SimpleVertex source, SimpleVertex target, out SimpleEdge edge)
            {
                throw new NotImplementedException();
            }

            public bool TryGetEdges(SimpleVertex source, SimpleVertex target, out IEnumerable<SimpleEdge> edges)
            {
                throw new NotImplementedException();
            }

            public bool TryGetOutEdges(SimpleVertex v, out IEnumerable<SimpleEdge> edges)
            {
                throw new NotImplementedException();
            }
        }


        [Serializable]
        public class SimpleVertex
        {
            [XmlAttribute]
            public string id { get; set; }

            [XmlAttribute]
            public string labelV { get; set; }

            [XmlAttribute]
            public string name { get; set; }

           [XmlAttribute]
            public string songType { get; set; }

            [XmlAttribute]
            public int performances { get; set; }

            public string lineage { get; set; }

            public List<GraphAttribute> properties { get; set; } = new List<GraphAttribute>();

        }

        public class SimpleEdge : IEdge<SimpleVertex>
        {
            [XmlAttribute]
            public string labelE { get; set; }
            [XmlAttribute]
            public double weight { get; set; }
            [XmlAttribute]
            public string id { get; set; }

            public SimpleVertex Source { get; set; }

            public SimpleVertex Target { get; set; }

            public string lineage { get; set; }
        }
    }
}
