using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class BotRuntimeModelType : ObjectGraphType<BotRuntimeModel>
    {
        public BotRuntimeModelType(IConnectivity connectivity)
        {
            Name = "botRuntimeModel";
            Description = "Contains the view of a bot relevant to a remote bot framework";
            Field(c => c.password).Description("Expected remote password");
            Field<LineageModelType>("model", resolve: context => connectivity.GetLineageModel(context.Source.userId, context.Source.botModelName));
            Field<ListGraphType<AuthorizationType>>("authorizations", resolve: context => context.Source.Authorizations);
            Field<ServiceConnectivityType>("serviceConnectivity", resolve: context => context.Source.serviceConnectivity);
        }
    }
}
