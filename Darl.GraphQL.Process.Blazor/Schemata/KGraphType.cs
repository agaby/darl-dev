using Darl.GraphQL.Process.Blazor.Models;
using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class KGraphType : ObjectGraphType<KGraph>
    {
        public KGraphType(IGraphProcessing graph)
        {
            Name = "kGraph";
            Description = "A Knowledge Graph and its status.";
            Field(c => c.Name).Description("The unique name of the knowledge graph");
            Field<GraphModelType>("model").ResolveAsync(async context => await graph.GetModel(context.Source.userId, context.Source.Name));
        }
    }
}
