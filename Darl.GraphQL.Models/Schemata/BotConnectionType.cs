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
            Field(c => c.AppId).Description("The id of the bot in the remote framework");
            Field(c => c.Password).Description("The password of the bot in the remote framework");
            Field(c => c.FriendlyName).Description("User freindly name to identify bot");
            Field<ListGraphType<UserUsageType>>("usageHistory", resolve: context => context.Source.UsageHistory);
            Field(c => c.userId).Description("The account owning the bot");
        }
    }
}
