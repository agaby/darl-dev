using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Middleware;
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

            Field(c => c.Name).Description("The unique name of the Bot model");
            Field<LineageModelType>("model", resolve: context => connectivity.GetLineageModel(connectivity.GetCurrentUserId(context.UserContext), context.Source.Name));
            Field<ListGraphType<AuthorizationType>>("authorizations", resolve: context => context.Source.Authorizations).AuthorizeWith("UserPolicy");
            Field<ServiceConnectivityType>("serviceConnectivity", resolve: context => context.Source.serviceConnectivity).AuthorizeWith("UserPolicy");
            Field<ListGraphType<UserUsageType>>("usageHistory", resolve: context => context.Source.UsageHistory).AuthorizeWith("UserPolicy");
        }
    }
}
