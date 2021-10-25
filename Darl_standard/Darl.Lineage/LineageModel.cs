using DarlCommon;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using ProtoBuf;
using System.Diagnostics;
using Newtonsoft.Json;
using Darl.Licensing;
using DarlLanguage.Processing;

namespace Darl.Lineage
{
    /// <summary>
    /// holds a single bot personality
    /// </summary>
    [ProtoContract]
    public class LineageModel
    {
        /// <summary>
        /// holds the text/lineage to darl associations
        /// </summary>
        [ProtoMember(1)]
        public LineageMatchTree tree { get; set; } = null;

        /// <summary>
        /// holds implicit knowledge as key value pairs
        /// </summary>
        [ProtoMember(2)]
        public Dictionary<string, string> modelSettings = new Dictionary<string, string>(); //{ { "name", "{\"name\": \"name\", \"unknown\": false, \"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\", \"value\": \"DarlBot\"}" },{"copyright","{\"name\": \"copyright\",\"unknown\": false,\"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\",\"value\": \"(c) 2017 Dr Andy's IP\"}" }, {"version","{\"name\": \"version\",\"unknown\": false,\"weight\": 1.0,\"approximate\": false,\"dataType\": \"textual\",\"value\": \"1.0.0\"}" } };

        /// <summary>
        /// holds the outer ruke set shell used to assemble a rule set from fragments
        /// </summary>
        [ProtoMember(3)]
        public string ruleSkeleton { get; set; } = "ruleset botRuleset \r\n{\r\ninput textual name;\r\ninput textual copyright;\r\ninput textual version;\r\ninput textual user_name;\r\noutput textual response;\r\noutput textual redirect;\r\n\r\n/*%% rule_insertion_point %%*/\r\n}";

        /// <summary>
        /// holds presentation formatting information for inputs and outputs
        /// </summary>
        [ProtoMember(4)]
        public string form { get; set; } = "{\"InputFormatList\": [ {\"Name\": \"user_name\",\"InType\": \"textual\",\"Categories\": null,\"NumericMax\": 0,\"NumericMin\": 0,\"Regex\": \"\"}],\"OutputFormatList\": [{\"Name\": \"response\",\"OutputType\": \"textual\",\"displayType\": \"Text\"},{ \"Name\": \"redirect\", \"OutputType\": \"textual\",\"displayType\": \"Redirect\"}]}";
        /// <summary>
        /// holds a multilingual set of texts that can be used in conversations.
        /// </summary>
        [ProtoMember(5)]
        public string texts { get; set; }


        public static string insertionPointText = "/*%% rule_insertion_point %%*/";

        public LineageModel()
        {
            if (!DarlLicense.licensed)
                throw new RuleException("license not set or invalid");
        }

        /// <summary>
        /// Find a list of actual or default matches
        /// </summary>
        /// <param name="text">The text to match</param>
        /// <param name="values">A repository for values found</param>
        /// <returns></returns>
        public List<MatchedElement> Match(string text, List<DarlVar> values, bool fuzzy = false)
        {
            var matches =  tree.Match(LineageLibrary.SimpleTokenizer(text), values,fuzzy);
            if (matches.Count > 0)
                Trace.WriteLine($"{matches.Count} matches to text : {text}");
            return matches;
        }

        /// <summary>
        /// Create a model from a stream
        /// </summary>
        /// <param name="stream">The stream</param>
        /// <returns>The model</returns>
        public static LineageModel Load(Stream stream)
        {
            var lm = Serializer.Deserialize<LineageModel>(stream);
            stream.Close();
            if (lm.tree == null)
                lm.tree = new LineageMatchTree();
            lm.tree.CreateExecutionTree();
            return lm;
        }

        /// <summary>
        /// Load just the tree of a model
        /// </summary>
        /// <param name="stream">The tree, streamed</param>
        /// <returns>The model</returns>
        public static LineageModel LoadFromTree(Stream stream)
        {
            var lm = new LineageModel();
            lm.tree = Serializer.Deserialize<LineageMatchTree>(stream);
            return lm;
        }

        /// <summary>
        /// Save a model to a stream
        /// </summary>
        /// <param name="stream">The stream</param>
        public void Store(Stream stream)
        {
            Serializer.Serialize<LineageModel>(stream,this);
//            stream.Close();
        }

        /// <summary>
        /// Save the tree part of a model
        /// </summary>
        /// <param name="stream">The stream for the tree</param>
        public void StoreToTree(Stream stream)
        {
            Serializer.Serialize<LineageMatchTree>(stream, this.tree);
        }

        /// <summary>
        /// Emit a tree as text
        /// </summary>
        /// <param name="sb"></param>
        public void ReadTree(StringBuilder sb)
        {
            tree.ReadTree(sb);
        }

        /// <summary>
        /// Add descriptions where needed
        /// </summary>
        public void AddDescriptions()
        {
            tree.AddDescriptions();
        }

        /// <summary>
        /// Check and combine tree elements. 
        /// </summary>
        /// <returns>A report</returns>
        /// <remarks>Looks for duplicates where a lineage subsumes another lineage or a literal. Ensures values don't have children.</remarks>
        public string Rationalize()
        {
            var sb = new StringBuilder();
            tree.Rationalize(sb);
            return sb.ToString();
        }

        /// <summary>
        /// Increment the version in the model settings
        /// </summary>
        public void IncrementVersion()
        {
            string versionString = "version";
            if (modelSettings.ContainsKey(versionString))
            {
                var vdv = JsonConvert.DeserializeObject<DarlVar>(modelSettings[versionString]);
                switch (vdv.dataType)
                {
                    case DarlVar.DataType.textual:
                        {
                            try
                            {
                                Version v = new Version(vdv.Value);
                                Version vnew = new Version(v.Major, v.Minor, v.Build, v.Revision + 1);
                                modelSettings[versionString] = JsonConvert.SerializeObject(new DarlVar { unknown = false, dataType = DarlVar.DataType.textual, Value = vnew.ToString(), name= versionString });
                            }
                            catch
                            { //non-standard format, ignore

                            }
                            break;
                        }
                    case DarlVar.DataType.numeric:
                        {
                            try
                            {
                                double version = double.Parse(vdv.Value);
                                version += 1.0;
                                modelSettings[versionString] = JsonConvert.SerializeObject(new DarlVar { unknown = false, dataType = DarlVar.DataType.numeric, Value = version.ToString(), name = versionString });
                            }
                            catch
                            {

                            }
                            break;
                        }
                }
            }
        }

        /// <summary>
        /// Find the best prototype match
        /// </summary>
        /// <remarks>A piece of text is presented that does not match the tree contents. 
        /// Finds the longest match in the tree, prioritising early matches over late.
        /// It then adds the remaining words to the path, recognising numeric values.
        /// </remarks>
        /// <param name="text">The text to match</param>
        /// <returns>The best match found</returns>
        public string BestMatch(string text)
        {
            var res = tree.BestMatch(LineageLibrary.SimpleTokenizer(text));
            int bestdepth = -1;
            SearchCandidate s = null;
            foreach(var p in res)
            {
                if(p.depth > bestdepth)
                {
                    s = p;
                    bestdepth = p.depth;
                }
            }
            if(s != null)
            {
                return s.fullpath;
            }
            return string.Empty;
        }
    }
}
