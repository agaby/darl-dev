using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlMutation : ObjectGraphType<object>
    {
        public DarlMutation()
        {
            Name = "Mutation";

/*            FieldAsync<OrderType>(
    "startOrder",
    arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "orderId" }),
    resolve: async context =>
    {
        var orderId = context.GetArgument<string>("orderId");
        return await context.TryAsyncResolve(
            async c => await orders.StartAsync(orderId));
    }
);*/
        }
    }
}