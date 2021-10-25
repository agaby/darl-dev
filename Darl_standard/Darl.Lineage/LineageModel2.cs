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

namespace Darl.Lineage
{
    [ProtoContract]
    public class LineageModel2
    {
        /// <summary>
        /// holds the text/lineage to darl associations
        /// </summary>
        [ProtoMember(1)]
        public LineageTree tree { get; set; } = null;

        /// <summary>
        /// holds implicit knowledge as key value pairs
        /// </summary>
        [ProtoMember(2)]
        public Dictionary<string, string> modelSettings = new Dictionary<string, string>();

        /// <summary>
        /// holds the outer ruke set shell used to assemble a rule set from fragments
        /// </summary>
        [ProtoMember(3)]
        public string ruleSkeleton { get; set; } = "ruleset botRuleset \n{\ninput textual topic;\ninput textual it;\ninput textual he;\ninput textual she;\ninput textual like;\ninput textual does;\ninput textual they;\ninput textual job;\ninput textual has;\noutput textual response;\n /*%% rule_insertion_point %%*/\n}";

        /// <summary>
        /// holds presentation formatting information for inputs and outputs
        /// </summary>
        [ProtoMember(4)]
        public string form { get; set; }
        /// <summary>
        /// holds a multilingual set of texts that can be used in conversations.
        /// </summary>
        [ProtoMember(5)]
        public string texts { get; set; }


        public static string insertionPointText = "/*%% rule_insertion_point %%*/";

        public LineageModel2()
        {

        }

        public List<string> Match(string text, out List<DarlVar> values)
        {
            values = new List<DarlVar>();
            var matches = tree.Match(LineageLibrary.SimpleTokenizer(text), values);
            if (matches.Count > 0)
                Trace.WriteLine($"{matches.Count} matches to text : {text}");
            return matches;

        }

        public static LineageModel Load(Stream stream)
        {
            var lm = Serializer.Deserialize<LineageModel>(stream);
            stream.Close();
            return lm;
        }

        public static LineageModel LoadFromTree(Stream stream)
        {
            var lm = new LineageModel();
            lm.tree = Serializer.Deserialize<LineageMatchTree>(stream);
            return lm;
        }


        public void Store(Stream stream)
        {
            Serializer.Serialize<LineageModel2>(stream, this);
            //            stream.Close();
        }

        public void StoreToTree(Stream stream)
        {
            Serializer.Serialize<LineageTree>(stream, this.tree);
        }


        public void ReadTree(StringBuilder sb)
        {
            tree.ReadTree(sb);
        }

    }
}
