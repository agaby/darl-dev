using Darl.GraphQL.Models.Connectivity;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlSubscription : ObjectGraphType<object>
    {
        public DarlSubscription(IConnectivity connectivity)
        {
            Name = "Subscription";
            Description = "Darl.dev does not use subscriptions";
            AddField(new EventStreamFieldType()
            { Name = "unsupported", Type= typeof(UpdateType)});
        }
    }
}