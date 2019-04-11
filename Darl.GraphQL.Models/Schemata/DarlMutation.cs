using Darl.Connectivity.Models;
using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Services;
using GraphQL.Types;
using System;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlMutation : ObjectGraphType<object>
    {
        public DarlMutation(IBotModelService botmodels, IMLModelService mlmodels, IRuleSetService rules, IConnectivity connectivity)
        {
            Name = "Mutation";
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

                      Field<ContactType>(
                           "createContact",
                           arguments: new QueryArguments(
                               new QueryArgument<NonNullGraphType<ContactInputType>> { Name = "contact" }),
                           resolve: context =>
                           {
                               var contactInput = context.GetArgument<ContactInput>("contact");
                               var id = Guid.NewGuid().ToString();
                               var contact = new Contact { Company = contactInput.Company, Country = contactInput.Country, Created = DateTime.Now.ToString(), Email = contactInput.Email, FirstName = contactInput.FirstName, IntroSent = contactInput.IntroSent, LastName = contactInput.LastName, Notes = contactInput.Notes, Phone = contactInput.Phone, RowKey = id, Sector = contactInput.Sector, Source = contactInput.Source, Title = contactInput.Title };
                               return connectivity.CreateContactAsync(contact);
                           }
                       );
            //                Update
                         Field<ContactType>(
                            "updateContact",
                            arguments: new QueryArguments(
                                new QueryArgument<NonNullGraphType<ContactUpdateType>> { Name = "contact" }),
                            resolve: context =>
                            {
                                var contactUpdate = context.GetArgument<ContactUpdate>("contact");
                                var contact = new Contact { Company = contactUpdate.Company, Country = contactUpdate.Country, Created = DateTime.Now.ToString(), Email = contactUpdate.Email, FirstName = contactUpdate.FirstName, IntroSent = contactUpdate.IntroSent, LastName = contactUpdate.LastName, Notes = contactUpdate.Notes, Phone = contactUpdate.Phone, RowKey = contactUpdate.Id, Sector = contactUpdate.Sector, Source = contactUpdate.Source, Title = contactUpdate.Title };
                                return connectivity.UpdateContactAsync(contact);
                            }
                        );
            //                Delete
                        Field<ContactType>(
                           "deleteContact",
                           arguments: new QueryArguments(
                               new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id" }),
                           resolve: context =>
                           {
                               var id = context.GetArgument<string>("id");
                               var contact = connectivity.GetContactById(id);
                               connectivity.DeleteContactAsync(id);
                               return contact;
                           }
                       );
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
            //            RuleSet
            //                Create Empty
            FieldAsync<MLModelType>("createEmptyRuleSet", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                return await context.TryAsyncResolve(
                    async c => await rules.CreateEmptyRuleSet(name));
            });
            //                Delete
            FieldAsync<MLModelType>("deleteRuleSet", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                return await context.TryAsyncResolve(
                    async c => await rules.DeleteRuleSet(name));
            });




        }
    }
}