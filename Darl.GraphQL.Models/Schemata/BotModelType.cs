using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Services;
using Darl.Lineage;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class BotModelType : ObjectGraphType<BotModel>
    {
        public BotModelType(ILineageModelService models)
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
