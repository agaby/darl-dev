using Darl.Connectivity.Models;
using Darl.GraphQL.Models.Connectivity;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class BotConnectionType : ObjectGraphType<ConnectivityView>
    {
        public BotConnectionType(IConnectivity connectivity)
        {
            Name = "BotConnection";
            Description = "Details of a bot in the bot framework";
            Field(c => c.AppId);
            Field(c => c.Password);
            Field<ListGraphType<BotUsageType>>("usageHistory", resolve: context => connectivity.GetBotUsage(context.Source.AppId));
        }
    }
}
