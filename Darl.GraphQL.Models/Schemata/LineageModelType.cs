using Darl.GraphQL.Models.Services;
using Darl.Lineage;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageModelType : ObjectGraphType<LineageModel>
    {
        public LineageModelType(IBotFormatService botFormat)
        {
            Field<BotFormatType>("form", resolve: context => botFormat.GetConvertedBotFormat(context.Source.form));
            Field(c => c.ruleSkeleton);
            Field(c => c.texts);
            Field<LineageMatchTreeType>("tree", resolve: context => context.Source.tree);//
        }
    }
}