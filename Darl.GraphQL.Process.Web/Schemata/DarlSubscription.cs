using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Schemata;
using GraphQL.Types;

namespace Darl.GraphQL.Web.Models.Schemata
{
    public class DarlSubscription : ObjectGraphType<object>
    {
        public DarlSubscription(IConnectivity connectivity)
        {
            Name = "Subscription";
            Description = "Darl.dev does not use subscriptions";
            AddField(new EventStreamFieldType()
            { Name = "unsupported", Type = typeof(UpdateType) });
        }
    }
}