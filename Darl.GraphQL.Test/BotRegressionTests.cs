using Darl.GraphQL.Models.Connectivity;
using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using Darl.Thinkbase;
using DarlLanguage.Processing;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
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
        private readonly IRuleFormInterface _rform;
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
            var blobLogger = new Mock<ILogger<BlobGraphConnectivity>>();
            var bgplogger = new Mock<ILogger<BlobGraphPrimitives>>();
            var glogger = new Mock<ILogger<GraphProcessing>>();
            var context = new Mock<IHttpContextAccessor>();
            var cache = new Mock<IDistributedCache>();
            context.Setup(a => a.HttpContext.User.Identity.Name).Returns(_config["userId"]);
            var licensing = new Mock<ILicensing>();
            _conv = new CosmosDBConnectivity(_config, connLogger.Object);
            var trigger = new Mock<ITrigger>();
            var bc = new BlobGraphConnectivity(_config, blobLogger.Object);
            var conn = new Mock<IConnectivity>();
            var meta = new Mock<IMetaStructureHandler>();
            var trans = new Mock<IKGTranslation>();
            var lic = new Mock<ILicensing>();
            var lcache = new Mock<IMemoryCache>();
            var blob = new BlobGraphPrimitives(bc, cache.Object, conn.Object, bgplogger.Object, lic.Object, lcache.Object, _config);
            var dataLoader = new DataLoader(meta.Object);
            _graph = new GraphProcessing(blob, glogger.Object, meta.Object, dataLoader);
            _graphStore = new GraphLocalStore(configuration.Object, logger.Object, context.Object, _graph);
            var ghandler = new Mock<IGraphHandler>();
            var dg = new Mock<IBotStateStorage>();
            _bot = new BotProcessing(botLogger.Object, _config, _graph, ghandler.Object, dg.Object);
        }





        SimpleVertex MakeVertex(String id)
        {
            return new SimpleVertex { id = id };
        }

        SimpleEdge MakeEdge(SimpleVertex source, SimpleVertex target, String id)
        {
            //define lineage and label based on source and target
            var lineage = "";
            lineage = "";
            return new SimpleEdge { id = id, Source = source, Target = target, lineage = lineage };
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
