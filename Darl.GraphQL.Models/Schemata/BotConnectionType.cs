using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class BotConnectionType : ObjectGraphType<BotConnection>
    {
        public BotConnectionType(IConnectivity connectivity)
        {
            Name = "BotConnection";
            Description = "Details of a bot in the bot framework";
            Field(c => c.AppId);
            Field(c => c.Password);
            Field(c => c.FreindlyName);
            Field<ListGraphType<UserUsageType>>("usageHistory", resolve: context => context.Source.UsageHistory);
        }
    }
}
