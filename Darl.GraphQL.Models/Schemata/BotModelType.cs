using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class BotModelType : ObjectGraphType<BotModel>
    {
        public BotModelType(IConnectivity connectivity)
        {
            Name = "BotModel";
            Description = "A bot model and its status.";

            Field(c => c.Name).Description("The the unique name of the Bot model");
            Field<LineageModelType>("model", resolve: context => connectivity.GetLineageModel(context.Source.Name));
            Field<ListGraphType<BotConnectionType>>("connections", resolve: context => context.Source.botconnections);
            Field<ListGraphType<AuthorizationType>>("authorizations", resolve: context => context.Source.Authorizations);
            Field<ServiceConnectivityType>("serviceConnectivity", resolve: context => context.Source.serviceConnectivity);
        }
    }
}
