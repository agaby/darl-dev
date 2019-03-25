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
            Field(c => c.LastModified).Description("The time the model was last modified.");
            Field(c => c.Size).Description("The size of the botmodel in bytes.");
            Field<LineageModelType>("model", resolve: context => models.GetLineageModelAsync(context.Source.Name));
            Field<ListGraphType<ConnectivityViewType>>("connections", resolve: context => models.GetBotConnectivity(context.Source.Name));
            Field<ListGraphType<AuthorizationsType>>("authorizations", resolve: context => models.GetAuthorizations(context.Source.Name));
        }
    }
}
