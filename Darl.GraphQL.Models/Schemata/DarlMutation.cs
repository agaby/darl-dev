using Darl.Connectivity.Models;
using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Services;
using DarlCommon;
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
            FieldAsync<AuthorizationsType>("createAuthorization", 
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }

                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var name = context.GetArgument<string>("name");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateAuthorization(botModelName,name));
                });
            //                Delete
            FieldAsync<AuthorizationsType>("deleteAuthorization", 
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }
                    ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var name = context.GetArgument<string>("name");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.DeleteAuthorization(botModelName,name));
                });

            //            BotConnection
            //                Create
            FieldAsync<ConnectivityViewType>("createBotConnection",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "appId" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "password" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var appId = context.GetArgument<string>("appId");
                    var password = context.GetArgument<string>("password");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateBotConnection(botModelName, appId, password));
                });

            //                Delete
            FieldAsync<ConnectivityViewType>("deleteBotConnection",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "appId" }
                    ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var appId = context.GetArgument<string>("appId");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.DeleteBotConnection(botModelName, appId));
                });
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
            FieldAsync<DefaultType>("createUpdateDefault", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }, new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "value" }), 
                resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var value = context.GetArgument<string>("value");
                return await context.TryAsyncResolve(
                    async c => await connectivity.CreateUpdateDefault(name, value));
            });
            //                Delete
            FieldAsync<DefaultType>("deleteDefault", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.DeleteDefault(name));
                });
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
            //                create/update from DARL
            FieldAsync<RuleFormType>("createRuleFormFromDarl", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }, new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "darl" }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var darl = context.GetArgument<string>("darl");
                return await context.TryAsyncResolve(
                    async c => await connectivity.CreateRuleFormFromDarl(name, darl));
            });
            //            FormFormat
            //                Update Input
            FieldAsync<InputFormatType>("updateRuleFormInputFormat", 
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName" }, 
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "inputName" },
                new QueryArgument<NonNullGraphType<InputFormatUpdateType>> { Name = "inputUpdate" }
                ), 
                resolve: async context =>
            {
                var ruleSetName = context.GetArgument<string>("ruleSetName");
                var inputName = context.GetArgument<string>("inputName");
                var inputUpdate = context.GetArgument<InputFormatUpdate>("inputUpdate");
                return await context.TryAsyncResolve(
                    async c => await connectivity.UpdateRuleFormInputFormat(ruleSetName, inputName, inputUpdate));
            });

            //                Update Output
            FieldAsync<OutputFormatType>("updateRuleFormOutputFormat",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "outputName" },
                new QueryArgument<NonNullGraphType<InputFormatUpdateType>> { Name = "outputUpdate" }
                ),
                resolve: async context =>
                {
                    var ruleSetName = context.GetArgument<string>("ruleSetName");
                    var outputName = context.GetArgument<string>("outputName");
                    var outputUpdate = context.GetArgument<OutputFormatUpdate>("outputUpdate");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateRuleFormOutputFormat(ruleSetName, outputName, outputUpdate));
                });

            //            Language
            //                update text
            FieldAsync<LanguageTextType>("updateRuleFormLanguageText",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "languageName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "languageText" }
                ),
                resolve: async context =>
                {
                    var ruleSetName = context.GetArgument<string>("ruleSetName");
                    var languageName = context.GetArgument<string>("languageName");
                    var languageText = context.GetArgument<string>("languageText");
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.UpdateRuleFormLanguageText(ruleSetName, languageName, languageText));
                });
            //                update variant
            FieldAsync<VariantTextType>("updateRuleFormVariantText",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "languageName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "isoLanguageName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "variantText" }
                ),
                resolve: async context =>
                {
                    var ruleSetName = context.GetArgument<string>("ruleSetName");
                    var languageName = context.GetArgument<string>("languageName");
                    var isoLanguageName = context.GetArgument<string>("isoLanguageName");
                    var variantText = context.GetArgument<string>("variantText");
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.UpdateRuleFormVariantText(ruleSetName, languageName, isoLanguageName, variantText));
                });
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