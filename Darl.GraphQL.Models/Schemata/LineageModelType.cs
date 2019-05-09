using Darl.Lineage;
using DarlCommon;
using GraphQL.Types;
using Newtonsoft.Json;

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
            Field<StringStringPairType>("modelSettings", resolve: c => BotFormatType.GetSSPairsFromDictionary(c.Source.modelSettings));
        }

        private BotFormat GetConvertedBotFormat(string source)
        {
            if (string.IsNullOrEmpty(source))
                return null;
            try
            {
                return JsonConvert.DeserializeObject<BotFormat>(source);
            }
            catch
            {
                return null;
            }
        }
    }
}
