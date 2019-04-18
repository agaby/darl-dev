using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class BotModelType : ObjectGraphType<BotModel>
    {
        public BotModelType()
        {
            Name = "BotModel";
            Description = "A bot model and its status.";

            Field(c => c.Name).Description("The the unique name of the Bot model");
            Field<LineageModelType>("model", resolve: context => context.Source.Model);
            Field<ListGraphType<ConnectivityViewType>>("connections", resolve: context => context.Source.botconnections);
            Field<ListGraphType<StringGraphType>>("authorizations", resolve: context =>context.Source.Authorizations);
            Field<ServiceConnectivityType>("serviceConnectivity", resolve: context => context.Source.serviceConnectivity);
        }
    }
}
