using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class MLModelType : ObjectGraphType<MLModel>
    {
        public MLModelType(IConnectivity connectivity)
        {
            Name = "MLModel";
            Description = "A Machine learning model and record data";
            Field(c => c.Name);
            Field<MLSpecType>("mlmodel", resolve: context => context.Source.model);
            Field<ListGraphType<MLResultType>>("results", resolve: context => context.Source.results);
            Field<ListGraphType<UserUsageType>>("usageHistory", resolve: context => context.Source.UsageHistory);
        }
    }
}
