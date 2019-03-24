using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlMutation : ObjectGraphType<object>
    {
        public DarlMutation()
        {
            Name = "Mutation";
        }
    }
}