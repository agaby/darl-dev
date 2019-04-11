using Darl.GraphQL.Models.Services;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlMutation : ObjectGraphType<object>
    {
        public DarlMutation(IBotModelService botmodels, IMLModelService mlmodels)
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
           FieldAsync<BotModelType>("createEmptyBotModel", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                return await context.TryAsyncResolve(
                    async c => await botmodels.CreateEmptyModel(name));
            });

            //                create a default model
            FieldAsync<BotModelType>("createDefaultBotModel", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                return await context.TryAsyncResolve(
                    async c => await botmodels.CreateDefaultModel(name));
            });

            //                Delete
            FieldAsync<BotModelType>("deleteBotModel", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                return await context.TryAsyncResolve(
                    async c => await botmodels.DeleteModel(name));
            });

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
            FieldAsync<MLModelType>("createEmptyMLModel", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                return await context.TryAsyncResolve(
                    async c => await mlmodels.CreateEmptyModel(name));
            });
            //                Delete
            FieldAsync<MLModelType>("deleteMLModel", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                return await context.TryAsyncResolve(
                    async c => await mlmodels.DeleteModel(name));
            });

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
            //            RuleForm
            //                create from DARL
            //                Update DARL
            //            FormFormat
            //                Update Input
            //                Update Output
            //            Language
            //                update text
            //                update variant




        }
    }
}