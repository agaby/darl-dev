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

//           BotModel
//                create an empty model
//                create a default model
//                Delete
//            Authorization
//                Create
//                Update
//                Delete
//            BotConnection
//                Create
//                Update 
//                Delete
//            LineageModel
//                Create from default
//                Delete
//            ServiceConnectivity
//                Edit AzureCredentials
//                Edit SellerCenter
//                Etc..
//            Contact
//                Create
//                Update
//                Delete
//            Default
//                Create
//                Update
//                Delete
//            MLModel
//                Create MLSpec as an object
//                Update as an object
//                Delete
//            BotFormat
//                Create as object
//                Add, update Delete constants
//                Add, update Delete stores
//                Add, update Delete sequences
//                Add, update Delete strings
//            BotInputFormat
//                Create as object
//                Update as object
//                Delete
//            BotOutputFormat
//                Create as object
//                Update as object
//                delete

              


        }
    }
}