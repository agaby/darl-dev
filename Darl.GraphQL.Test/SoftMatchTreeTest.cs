using Darl.GraphQL.Models.Connectivity;
using Darl.SoftMatch;
using Darl.Thinkbase;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace Darl.GraphQL.Test
{
    [TestClass]
    public class SoftMatchTreeTest
    {
        ISoftMatchProcessing cmp;
        IConfiguration _config;
        ISoftMatch softmatch;


        [TestInitialize()]
        public void Initialize()
        {
            var configuration = new Mock<IConfiguration>();

            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinHostname")]).Returns("thinkbase.gremlin.cosmosdb.azure.com");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinPort")]).Returns("443");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinAuthKey")]).Returns("ffWKZWMJro4JHBaJAi4yG1o35ujaDvj0pIkrqsYEz4hCoHR9jvHr6YR3Pb2dxr8rw4obuO4ZvnJetejwJyrYQA==");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinDatabase")]).Returns("farleft");
            configuration.Setup(a => a[It.Is<string>(s => s == "gremlinCollection")]).Returns("hypernymy");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevAPiKey")]).Returns("e438440e-9d90-46e8-87ed-080e19c43aed");
            configuration.Setup(a => a[It.Is<string>(s => s == "darlDevUrl")]).Returns("https://darl.dev/graphql/");
            configuration.Setup(a => a[It.Is<string>(s => s == "userId")]).Returns("5ee43551-c05c-4cff-8582-c08f23f84c14");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:StorageConnectionString")]).Returns("DefaultEndpointsProtocol=https;AccountName=darlai;AccountKey=errnwefiVeXcDr0aKbHDxXjblOQhwFwHkeG4qR4caChkABnzp9MNeBBX0FP1jc4DnXPGztI67pbEBXDqA1dPCw==");
            configuration.Setup(a => a[It.Is<string>(s => s == "AppSettings:BlobContainer")]).Returns("darldevblobs");
            var bloblogger = new Mock<ILogger<BlobGraphConnectivity>>();
            var cmplogger = new Mock<ILogger<SoftMatchProcessing>>();
            //            var bc = new BlobConnectivity(configuration.Object, bloblogger.Object);
            var bc = new LocalBlob();
            cmp = new SoftMatchProcessing(bc, cmplogger.Object);
            _config = configuration.Object;
        }

        [TestCleanup()]
        public void Cleanup()
        {

        }

        [TestMethod]
        public async Task TestLoadSource()
        {
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.learning_outcomes.txt"));
            var doc = docsource.ReadToEnd();
            int index = 1;
            var lines = doc.Split("\r\n");
            var dict = new List<StringStringPair>();
            foreach (var line in lines)
            {
                dict.Add(new StringStringPair(index.ToString(), line));
                index++;
            }
            await cmp.CreateSoftMatchModel(_config["userId"], "learning_outcomes", dict, true);
        }

        [TestMethod]
        public async Task TestInference()
        {
            await cmp.InferFromSoftMatchModel(_config["userId"], "learning_outcomes", new List<string> { "who are you fuckwad" });
        }

        [TestMethod]
        public async Task TestLoadJsonSource()
        {
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.input2.json"));
            var doc = docsource.ReadToEnd();
            var records = JsonConvert.DeserializeObject<List<Record>>(doc);
            //load records 100 at a a time, resetting the tree on the first pass
            int offset = 0;
            int blockSize = 100;
            while (offset < records.Count)
            {
                var dict = new List<StringStringPair>();
                foreach (var rec in records.Skip(offset).Take(blockSize))
                {
                    dict.Add(new StringStringPair(rec.Id, rec.Text));
                }
                await cmp.CreateSoftMatchModel(_config["userId"], "learning_outcomes_2", dict, offset == 0);
                offset += blockSize;
            }
            // test recall
            var failures = new List<Record>();
            int correct = 0;
            int noTieError = 0;
            int topThree = 0;
            var textList = new List<string>();
            foreach (var r in records)
            {
                textList.Add(r.Text);
            }
            var res = await cmp.InferFromSoftMatchModel(_config["userId"], "learning_outcomes_2", textList);
            for (int n = 0; n < records.Count; n++)
            {
                if (res[n] != null)
                {
                    if (res[n].index == records[n].Id)
                    {
                        correct++;
                        topThree++;
                    }
                    else
                    {
                        if (res[n].tieCount == 1)
                        {
                            noTieError++;
                        }
                        if (res[n].alternatives.ContainsKey(records[n].Id))
                            topThree++;
                        else
                        {
                            failures.Add(records[n]);
                        }
                    }
                }
            }
            /*           foreach (var r in records)
                       {
                           var res = await cmp.InferFromConceptMatchTree(_config["userId"], "learning_outcomes_2", new List<string> { r.Text });
                           if (res[0].index == r.Id)
                           {
                               correct++;
                               topThree++;
                           }
                           else
                           {
                               if (res[0].tieCount == 1)
                               {
                                   noTieError++;
                               }
                               if (res[0].alternatives.ContainsKey(r.Id))
                                   topThree++;
                               else
                               {
                                   failures.Add(r);
                               }
                           }
                       }*/

            /*            foreach (var r in failures)
                        {
                            var res2 = await cmp.InferFromConceptMatchTree(_config["userId"], "learning_outcomes_2", new List<string> { r.Text });

                        }*/
        }

        [TestMethod]
        public async Task TestMSRParaphrase()
        {
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.msr_paraphrase_train.txt"));
            var doc = docsource.ReadToEnd();
            var lines = doc.Split('\n').ToList();
            lines.RemoveAt(0); //get rid of header
            //load records 100 at a a time, resetting the tree on the first pass
            int offset = 0;
            int blockSize = 100;
            var equivalents = new Dictionary<string, string>();
            while (offset < lines.Count)
            {
                var dict = new List<StringStringPair>();
                foreach (var line in lines.Skip(offset).Take(blockSize))
                {
                    var elements = line.Split('\t');
                    if (elements.Count() != 5)
                    {
                        continue;
                    }
                    if (elements[0] == "1")
                    {
                        dict.Add(new StringStringPair(elements[1], elements[3]));
                        equivalents.Add(elements[1], elements[4].Trim());
                    }
                }
                await cmp.CreateSoftMatchModel(_config["userId"], "learning_outcomes_2", dict, offset == 0);
                offset += blockSize;
            }
            var indices = new List<string>();
            var textList = new List<string>();
            foreach (var i in equivalents.Keys)
            {
                indices.Add(i);
                textList.Add(equivalents[i]);
            }
            var res = await cmp.InferFromSoftMatchModel(_config["userId"], "learning_outcomes_2", textList);
            int correct = 0;
            int noTieError = 0;
            int topThree = 0;
            for (int n = 0; n < indices.Count; n++)
            {
                if (res[n] != null)
                {
                    if (res[n].index == indices[n])
                    {
                        correct++;
                        topThree++;
                    }
                    else
                    {
                        if (res[n].tieCount == 1)
                        {
                            noTieError++;
                        }
                        if (res[n].alternatives.ContainsKey(indices[n]))
                            topThree++;
                    }
                }
            }
        }

        [TestMethod]
        public async Task TestQuoraLoad()
        {
            var docsource = new StreamReader(Assembly.GetExecutingAssembly().GetManifestResourceStream("Darl.GraphQL.Test.quora_duplicate_questions.tsv"));
            var doc = docsource.ReadToEnd();
            var lines = doc.Split('\n').ToList();
            var softMatch = new MatchList();
            lines.RemoveAt(0); //get rid of header
            //load records 100 at a a time, resetting the tree on the first pass
            int offset = 0;
            int blockSize = 100;
            var equivalents = new Dictionary<string, string>();
            while (offset < lines.Count)
            {
                var dict = new List<KeyValuePair<string, string>>();
                foreach (var line in lines.Skip(offset).Take(blockSize))
                {
                    var elements = line.Split('\t');
                    if (elements.Count() != 6)
                    {
                        continue;
                    }
                    dict.Add(KeyValuePair.Create<string, string>(elements[1], elements[3]));
                    dict.Add(KeyValuePair.Create<string, string>(elements[2], elements[4]));
                }
                offset += blockSize;
                softMatch.CreateTree(dict);
            }
        }
    }

    public class Record
    {
        public string Id { get; set; }

        public string Text { get; set; }
    }

    public class LocalBlob : IBlobConnectivity
    {
        readonly Dictionary<string, byte[]> localData = new Dictionary<string, byte[]>();

        public string implementation => nameof(LocalBlob);

        public string CreateTimedAccessUrl(string name)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Delete(string name)
        {
            throw new NotImplementedException();
        }

        public Task<bool> Exists(string name)
        {
            return Task.FromResult(localData.ContainsKey(name));
        }

        public List<string> List(string prefix)
        {
            throw new NotImplementedException();
        }

        public async Task<byte[]> Read(string name)
        {
            return localData[name];
        }

        public Task Write(string name, byte[] data)
        {
            if (localData.ContainsKey(name))
                localData[name] = data;
            else
                localData.Add(name, data);
            return Task.CompletedTask;
        }
    }


}
