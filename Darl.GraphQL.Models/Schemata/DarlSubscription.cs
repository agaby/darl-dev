using Darl.GraphQL.Models.Connectivity;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlSubscription : ObjectGraphType<object>
    {
        public DarlSubscription(IConnectivity connectivity)
        {

        }
    }
}