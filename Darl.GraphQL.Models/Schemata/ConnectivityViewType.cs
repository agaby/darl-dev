using Darl.Connectivity.Models;
using Darl.GraphQL.Models.Connectivity;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class ConnectivityViewType : ObjectGraphType<ConnectivityView>
    {
        public ConnectivityViewType(IConnectivity connectivity)
        {
            Field(c => c.AppId);
            Field(c => c.Password);
            Field<ListGraphType<BotUsageType>>("usageHistory", resolve: context => connectivity.GetBotUsage(context.Source.AppId));
        }
    }
}
