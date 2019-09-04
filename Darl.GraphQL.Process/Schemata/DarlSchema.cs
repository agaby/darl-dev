using System;
using System.Collections.Generic;
using System.Text;
using Darl.GraphQL.Process.Middleware;
using GraphQL;
using GraphQL.Types;
using GraphQL.Utilities;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlSchema : Schema
    {
        public DarlSchema(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Query = serviceProvider.GetRequiredService<DarlQuery>();
            Mutation = serviceProvider.GetRequiredService<DarlMutation>();
            Subscription = serviceProvider.GetRequiredService<DarlSubscription>();
            Filter = serviceProvider.GetRequiredService<AdminFilter>();
        }
    }
}
