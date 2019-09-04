using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using DarlCommon;
using GraphQL.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageModelType : ObjectGraphType<LineageModel>
    {
        public LineageModelType()
        {
            Name = "LineageModel";
            Description = "A bot model";
            Field<BotFormatType>("form", resolve: context => GetConvertedBotFormat(context.Source.form));
            Field(c => c.ruleSkeleton);
            Field(c => c.texts,true);
            Field<LineageMatchTreeType>("tree", resolve: context => context.Source.tree);
            Field<ListGraphType<StringDarlVarPairType>>("modelSettings", resolve: c => GetSDPairsFromDictionary(c.Source.modelSettings));
        }

        private BotFormat GetConvertedBotFormat(string source)
        {
            if (string.IsNullOrEmpty(source))
                return null;
            try
            {
                return JsonConvert.DeserializeObject<BotFormat>(source, new StringEnumConverter());
            }
            catch
            {
                return null;
            }
        }

        public static List<StringDarlVarPair> GetSDPairsFromDictionary(Dictionary<string, string> dict)
        {
            var list = new List<StringDarlVarPair>();
            foreach (var k in dict.Keys)
            {
                list.Add(new StringDarlVarPair { Name = k, Value=JsonConvert.DeserializeObject<DarlVar>(dict[k], new StringEnumConverter()) });
            }
            return list;
        }
    }
}
