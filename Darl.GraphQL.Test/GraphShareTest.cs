using Microsoft.VisualStudio.TestTools.UnitTesting;
using Darl.GraphQL.Models.Connectivity;
using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using Darl.Thinkbase;
using DarlLanguage.Processing;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using Darl.Thinkbase.Meta;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class GraphShareTest
    {
        private IGraphProcessing _graph;
        private IConfiguration _config;
        private IGraphPrimitives _primitives;
        private ILocalStore _graphStore;
        private IConnectivity _conn;
        private IFormApi _form;
        private IRuleFormInterface _rf;
        private ITrigger _trigger;
        private ILogger<BotProcessing> _bplogger;
        private IHttpContextAccessor _context;
        private IMetaStructureHandler _meta;
        private IProducts _prods;

        private static string sourceLineage = "noun:01,4,04,02,21,16";
        private static string destinationLineage = "noun:01,0,0,15,15,3";
        private static string processLineage = "noun:00,4";
        private static string defaultLineage = "noun:01,0,0,15,07,02,06,05";//constant
        private static string valueLineage = "noun:01,4,04,02,07,01";//text
        private static string collateralLineage = "noun:00,1,00,3,10,09,07";//document
        private static string firstNameLineage = "noun:01,3,14,01,06,13";//first name
        private static string lastNameLineage = "noun:01,3,14,01,06,11";//surname
        private static string emailLineage = "noun:01,0,2,00,38,00,06,1";//email
        private static string phoneLineage = "noun:01,4,07,01";//phone
        private static string occupationLineage = "noun:01,0,2,00,23,19";//occupation
        private static string noteLineage = "noun:01,4,05,21,28,1";//note
        private static string companyLineage = "noun:01,2,07,10";//organization
        private static string countryLineage = "noun:01,2,06,35";//nation
        private static string sectorLineage = "noun:01,0,0,15,07,02,04,1,02,1";//sector
        private static string keyLineage = "noun:01,4,09,01,7,3,0";//key
        private static string subscriptionLineage = "noun:01,0,2,00,34,6,1,5,0";
        private static string idLineage = "noun:01,4,09,01,7,3,5";
        private static string stateLineage = "noun:01,1,00";
        private static string typeLineage = "noun:01,0,0,15,07,02,02,0,01";
        private static string existenceLineage = "noun:01,5,03,3,018";//life

        private static string graphName = "backoffice_test.graph";

        [TestInitialize()]
        public void Initialize()
        {
            var configuration = new Mock<IConfiguration>();

            //            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("7ecb39be-fb44-4c13-92df-68ec152a4edb");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("2495b08b-93c3-4498-85b7-f4bdd36b6f01");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("e438440e-9d90-46e8-87ed-080e19c43aed");
            //configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("33db770b-29e9-46ae-8a19-c1947bd775d8");
            //            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("5ee43551-c05c-4cff-8582-c08f23f84c14");
            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("a26560b3-7778-410b-a54b-b65da6a9649a");//andy@darl.ai account
            //            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinLocation")]).Returns("local");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevUrl")]).Returns("https://darl.dev/graphql/");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:BlobContainer")]).Returns("darldevgraphs");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:GraphContainer")]).Returns("darldevgraphs");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StorageConnectionString")]).Returns("DefaultEndpointsProtocol=https;AccountName=darlai;AccountKey=errnwefiVeXcDr0aKbHDxXjblOQhwFwHkeG4qR4caChkABnzp9MNeBBX0FP1jc4DnXPGztI67pbEBXDqA1dPCw==");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:MongoDatabase")]).Returns("darlai");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:MongoConnectionString")]).Returns("mongodb://darlai:VqbEsCyGXAkTlWUuOb3Y4RQbFqmZs3VZaAWDtYrDO3054dicQHbWMo2OhbabuWK4szQM5VmgoUHe8jGihAIWdQ==@darlai.documents.azure.com:10255/?ssl=true&replicaSet=globaldb");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StripeAPIKey")]).Returns("sk_live_gKHiW6CmjAgGjhb9x3FY6n9H");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StripeCorporateLicensePlan")]).Returns("plan_DINGKECtG7jIs5");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StripeCorporateUsagePlan")]).Returns("plan_DINOybSsS0vMXf");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:boaiuserid")]).Returns("8c663676-a7dc-4561-af3d-89b38555837d");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:BackOfficeKG")]).Returns("backoffice_test.graph");
            //           configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StripeTrialPeriodDays")]).Returns<int>(a >= 30);



            var logger = new Mock<ILogger<GraphLocalStore>>();
            var blogger = new Mock<ILogger<BlobConnectivity>>();//
            var bgplogger = new Mock<ILogger<BlobGraphPrimitives>>();
            var glogger = new Mock<ILogger<GraphProcessing>>();
            var context = new Mock<IHttpContextAccessor>();
            _config = configuration.Object;
            context.Setup(a => a.HttpContext.User.Identity.Name).Returns(_config["userId"]);
            var blob = new BlobGraphConnectivity(_config, blogger.Object);
            var cache = new Mock<IDistributedCache>();
            cache.Setup(a => a.GetAsync(It.IsAny<string>(), default)).Returns(Task.FromResult<byte[]>(null));
            var clogger = new Mock<ILogger<CosmosDBConnectivity>>();
            var clicense = new Mock<ILicensing>();
            _meta = new MetaStructureHandler();
            _conn = new CosmosDBConnectivity(_config, clogger.Object, clicense.Object, cache.Object);
            var trans = new Mock<IKGTranslation>();
            _primitives = new BlobGraphPrimitives(new List<IBlobConnectivity> { blob }, cache.Object, _conn, bgplogger.Object);
            _graph = new GraphProcessing(_primitives, glogger.Object,_meta);
            _graphStore = new GraphLocalStore(_config, logger.Object, context.Object, _graph);
            var form = new Mock<IFormApi>();
            _form = form.Object;
            var rf = new Mock<IRuleFormInterface>();
            _rf = rf.Object;
            var trigger = new Mock<ITrigger>();
            _trigger = trigger.Object;
            var bplogger = new Mock<ILogger<BotProcessing>>();
            _bplogger = bplogger.Object;
            _context = context.Object;
            _prods = new Mock<IProducts>().Object;
        }

        [TestMethod]
//        [Ignore]
        public async Task ShareText()
        {
            var demoUser = _config["AppSettings:boaiuserid"];
//            await _conn.UpdateSubscriptionType(daveUser, DarlUser.SubscriptionType.corporate);
            await _conn.ShareKGraph(_config["userId"],graphName, demoUser, false,true);
        }

        /// <summary>
        /// Shares descriptions from the masters to shared KGs in the demo account
        /// </summary>
        /// <returns></returns>
        [TestMethod]
        [Ignore]
        public async Task ShareDescriptions()
        {
            var dest = await _conn.GetKGraphsAsync(_config["AppSettings:boaiuserid"]);
            foreach(var kg in dest)
            {
                if(kg.Shared)
                {
                    var shared = await _conn.GetKGModel(kg.OwnerId, kg.Name);
                    if(shared != null)
                    {
                        await _conn.UpdateKGraph(_config["AppSettings:boaiuserid"], kg.Name, new KGraphUpdate { Description = shared.Description });
                    }
                }
            }
        }

        [TestMethod]
        public async Task CorrectRecognitionLabels()
        {
            //read a KG
            //access the recognition objects,
            var compositeName = $"{_config["userId"]}_{graphName}";
            var rec = await _graph.GetRecognitionDisplayGraph(compositeName);
            foreach(var ob in rec.nodes)
            {
                var recObj = await _graph.GetRecognitionObjectById(compositeName, ob.id);
                if (string.IsNullOrEmpty(recObj.name) || recObj.name == recObj.lineage)
                {
                    if (recObj.lineage == "navigation:")
                    {
                        recObj.name = "navigation root";
                    }
                    else
                    {
                        //convert the name to be the typeword if present.
                        var typeword = await _conn.GetTypeWordForLineage(recObj.lineage);
                        if (!string.IsNullOrEmpty(typeword))
                            recObj.name = $"~{typeword}";
                    }
                }
            }
            //save kgraph.
            await _graph.Store(compositeName);
        }

        [TestMethod]
        public async Task AddDefaultRecognitionTree()
        {
            var compositeName = $"{_config["userId"]}_{graphName}";
            var model = await _graph.GetModel(_config["userId"], graphName) as BlobGraphContent;
            var p = _primitives as BlobGraphPrimitives;
            model.recognitionVertices.Clear();
            model.recognitionRoots.Clear();
            model.recognitionEdges.Clear();
            p.AddDefaultContent(model);
            await _graph.Store(compositeName);
        }

        [TestMethod]
        public async Task FindTextInProperties()
        {
            var target = "female";
            var compositeName = $"{_config["userId"]}_{graphName}";
            var model = await _graph.GetModel(_config["userId"], graphName) as BlobGraphContent;
            foreach (var v in model.vertices)
            {
                if (v.Value.properties != null)
                {
                    foreach (var p in v.Value.properties)
                    {
                        if(p.value.Contains(target))
                        {
                            break;
                        }
                    }
                }
            }
            foreach (var v in model.virtualVertices)
            {
                if (v.Value.properties != null)
                {
                    foreach (var p in v.Value.properties)
                    {
                        if (p.value.Contains(target))
                        {
                            break;
                        }
                    }
                }
            }
        }

        [TestMethod]
        [Ignore]
        public async Task LoadInitialText()
        {
            var name = "primary_math.graph";
            var text = "Teach me mathematics";
            await _conn.UpdateKGraph(_config["userId"], name, new KGraphUpdate { InitialText =  text});
            await _conn.UpdateKGraph(_config["AppSettings:boaiuserid"], name, new KGraphUpdate { InitialText = text });
        }

        [TestMethod]
        [Ignore]
        public async Task SwapModel()
        {
            await _graph.CopyRenameKG(_config["userId"], "primary_math_old.graph", "primary_math.graph");
        }

        [TestMethod]
        public async Task ReplaceLineageLiterals()
        {
            var model = await _graph.GetModel(_config["userId"], graphName) as BlobGraphContent;
            var compositeName = $"{_config["userId"]}_{graphName}";
            var msh = new MetaStructureHandler();
            var runtime = new DarlMetaRunTime(msh);
            var reverse = new Dictionary<string, string>();
            foreach (var c in msh.CommonLineages)
                reverse.Add(c.Value, c.Key);
            foreach (var v in model.vertices)
            {
                if (v.Value.properties != null)
                {
                    foreach (var p in v.Value.properties)
                    {
                        if(p.name == "text")
                        {
                            p.type = GraphAttribute.DataType.markdown;
                            continue;
                        }
                        if (p.type == GraphAttribute.DataType.ruleset)
                        {
                            var cursor = 0;
                            p.value = ReplaceLiterals(p.value, reverse, "noun:", ref cursor);
                            p.value = ReplaceLiterals(p.value, reverse, "verb:", ref cursor);
                            p.value = ReplaceLiterals(p.value, reverse, "adjective:", ref cursor);
                            try
                            {
                                runtime.CreateTree(p.value, v.Value, model);
                            }
                            catch(Exception err)
                            {

                            }
                        }

                    }
                }
            }
            foreach (var v in model.virtualVertices)
            {
                if (v.Value.properties != null)
                {
                    foreach (var p in v.Value.properties)
                    {
                        if (p.type == GraphAttribute.DataType.ruleset)
                        {
                            var cursor = 0;
                            p.value = ReplaceLiterals(p.value, reverse, "noun:",ref cursor);
                            p.value = ReplaceLiterals(p.value, reverse, "verb:",ref cursor);
                            p.value = ReplaceLiterals(p.value, reverse, "adjective:", ref cursor);
                            try
                            {
                                runtime.CreateTree(p.value, v.Value, model);
                            }
                            catch (Exception err)
                            {

                            }
                        }
                    }
                }
            }
            //add recognition
            await _graph.Store(compositeName);
        }

        private string ReplaceLiterals(string code, Dictionary<string,string> reverse, string preamble, ref int cursor)
        {
            bool complete = false;
            StringBuilder sb = new StringBuilder();
            var foundSoFar = new Dictionary<string, string>();
            var offset = cursor;
            if (cursor > 0)
            {
                sb.Append(code.Substring(0, cursor));
            }
            while (!complete)
            {
                var start = code.IndexOf(preamble, cursor);
                if(start == -1)
                {
                    complete = true;
                    sb.Append(code.Substring(cursor));
                }
                else
                {
                    var end = code.IndexOf("\"", start + 1);
                    var lineage = code.Substring(start, end - start);

                    if(reverse.ContainsKey(lineage))
                    {
                        sb.Append(code.Substring(cursor, start - cursor - 1));
                        sb.Append(reverse[lineage]);
                        end++;
                    }
                    else
                    {
                        if(foundSoFar.ContainsKey(lineage))
                        {
                            var typeword = foundSoFar[lineage];
                            sb.Append(code.Substring(cursor, start - cursor - 1));
                            sb.Append(typeword);
                            end++;
                        }
                        else if (LineageLibrary.lineages.ContainsKey(lineage))
                        {
                            var typeword = LineageLibrary.lineages[lineage].typeWord;
                            var insert = $"lineage {typeword} \"{lineage}\";\n";
                            sb.Insert(0, insert);
                            offset += insert.Length;
                            sb.Append(code.Substring(cursor, start - cursor - 1));
                            sb.Append(typeword);
                            foundSoFar.Add(lineage, typeword);
                            end++;
                        }
                        else if (lineage.Contains("+")) //composite lineage
                        {
                            int pos = lineage.IndexOf("+");
                            var primary = lineage.Substring(0, pos);
                            var secondary = lineage.Substring(pos + 1);
                            if (LineageLibrary.lineages.ContainsKey(primary) && LineageLibrary.lineages.ContainsKey(secondary))
                            {
                                var pword = LineageLibrary.lineages[primary].typeWord + "_" + LineageLibrary.lineages[secondary].typeWord;
                                var insert = $"lineage {pword} \"{lineage}\";\n";
                                sb.Insert(0, insert);
                                offset += insert.Length; 
                                sb.Append(code.Substring(cursor, start - cursor - 1));
                                sb.Append(pword);
                                foundSoFar.Add(lineage, pword);
                                end++;
                            }
                        }
                        else
                        {
                            sb.Append(code.Substring(cursor, end - cursor));
                        }
                    }
                    cursor = end;
                }
            }
            cursor = offset;
            return sb.ToString();
        }




        [TestMethod]
        public async Task UpdateAttributeConfidence()
        {
            var m = await _graph.GetModel(_config["AppSettings:boaiuserid"], "backoffice.graph");
            foreach(var v in m.vertices.Values)
            {
                if(v.properties != null)
                {
                    foreach(var a in v.properties)
                    {
                        a.confidence = 1.0;
                    }
                }
            }
            await _graph.Store(_config["AppSettings:boaiuserid"] + "_" + "backoffice.graph");
        }

        [TestMethod]
        public void Testproducts()
        {
            var prods = new Products(_config);

        }

        [TestMethod]
        [Ignore]
        public async Task RemoveUsersInTest()
        {
            var m = await _graph.GetModel(_config["AppSettings:boaiuserid"], "backoffice_test.graph");
            var comp = _config["AppSettings:boaiuserid"] + "_" + "backoffice_test.graph";
            var ids = new List<string>();
            foreach (var v in m.vertices.Values)
            {
                if(v.lineage.StartsWith(_meta.CommonLineages["person"]))
                {
                    ids.Add(v.id);
                }
            }
            foreach(var i in ids)
            {
                await _graph.DeleteGraphObject(comp, i);
            }
            await _graph.Store(comp);
        }


        [TestMethod]
        [Ignore]
        public async Task GenerateModelObjects()
        {
            var comp = $"{_config["AppSettings:boaiuserid"]}_backoffice_test.graph";
            var m = await _graph.GetModel(_config["AppSettings:boaiuserid"], "backoffice_test.graph");
            await _graph.ClearGraphContent(comp);
            await _graph.CreateGraphObject(comp, new GraphObjectInput { externalId = "collateral", lineage = collateralLineage, name = "collateral" }, OntologyAction.build);
            await _graph.CreateGraphObject(comp, new GraphObjectInput { externalId = "default", lineage = defaultLineage, name = "default" }, OntologyAction.build);
            await _graph.CreateGraphObject(comp, new GraphObjectInput { externalId = "person", lineage = _meta.CommonLineages["person"], name = "person" }, OntologyAction.build);
            await _graph.CreateGraphObject(comp, new GraphObjectInput { externalId = "update", lineage = processLineage, name = "update" }, OntologyAction.build);
            await _graph.Store(comp);
        }

        [TestMethod]
        public async Task CreateMailshot()
        {

        }

    }
}
