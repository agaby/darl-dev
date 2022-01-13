using GraphQL.Types;
using GraphQL.Utilities;
using System;

namespace Darl.GraphQL.Container.Models.Schemata
{
    public class DarlSchema : Schema
    {
        public DarlSchema(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            Query = serviceProvider.GetRequiredService<DarlQuery>();
            Mutation = serviceProvider.GetRequiredService<DarlMutation>();
            Subscription = serviceProvider.GetRequiredService<DarlSubscription>();
        }
    }
}
