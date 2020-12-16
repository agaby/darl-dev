using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Middleware;
using Darl.GraphQL.Models.Models;
using Darl.Thinkbase;
using DarlCommon;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlMutation : ObjectGraphType<object>
    {
        public DarlMutation(IConnectivity connectivity, IEmailProcessing email, IGraphProcessing graph, IConfiguration _config, ISoftMatchProcessing cmp)
        {
            Name = "Mutation";
            Description = "Make changes to the contents of your account.";
            this.AuthorizeWith("UserPolicy");
            // BotModel
            //    create an empty model
            FieldAsync<BotModelType>("createEmptyBotModel", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: async context =>
             {
                 var name = context.GetArgument<string>("name");
                 var userId = connectivity.GetCurrentUserId(context.UserContext);
                 return await context.TryAsyncResolve(
                     async c => await connectivity.CreateEmptyModel(userId, name));
             });

            // create a default model
            FieldAsync<BotModelType>("createDefaultBotModel", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var userId = connectivity.GetCurrentUserId(context.UserContext);

                return await context.TryAsyncResolve(
                    async c => await connectivity.CreateDefaultModel(userId, name));
            });

            // Delete
            FieldAsync<BotModelType>("deleteBotModel", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var userId = connectivity.GetCurrentUserId(context.UserContext);
                return await context.TryAsyncResolve(
                    async c => await connectivity.DeleteBotModel(userId, name));
            });

            // Authorization
            //  Create
 /*           FieldAsync<AuthorizationType>("createAuthorization", 
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<AuthorizationUpdateType>> { Name = "authorization" }

                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var authorization = context.GetArgument<Authorization>("authorization");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateAuthorization(botModelName, authorization));
                });*/
            //  Delete
            FieldAsync<AuthorizationType>("deleteAuthorization", 
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }
                    ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var name = context.GetArgument<string>("name");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.DeleteAuthorization(userId, botModelName, name));
                });

            // LineageTree

            //  CreateNode
            FieldAsync<LineageNodeDefinitionType>("createLineageNode",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "parent" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "newName" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var parent = context.GetArgument<string>("parent");
                    var newName = context.GetArgument<string>("newName");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);

                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateLineageNode(userId, botModelName, parent, newName));
                });
            //  RenameNode
            FieldAsync<LineageNodeDefinitionType>("renameLineageNode",
                           arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                           new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id" },
                           new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "newName" }
                           ),
                           resolve: async context =>
                           {
                               var botModelName = context.GetArgument<string>("botModelName");
                               var id = context.GetArgument<string>("id");
                               var newName = context.GetArgument<string>("newName");
                               var userId = connectivity.GetCurrentUserId(context.UserContext);
                               return await context.TryAsyncResolve(
                                   async c => await connectivity.RenameLineageNode(userId, botModelName, id, newName));
                           });

            //  DeleteNode
            FieldAsync<LineageNodeDefinitionType>("deleteLineageNode",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var id = context.GetArgument<string>("id");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.DeleteLineageNode(userId, botModelName, id));
                });

            //  PasteNode 
            FieldAsync<LineageNodeDefinitionType>("pasteLineageNode",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "parent" },
                new QueryArgument<NonNullGraphType<ListGraphType<StringGraphType>>> { Name = "nodes" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "mode" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var parent = context.GetArgument<string>("parent");
                    var nodes = context.GetArgument<List<string>>("nodes");
                    var mode = context.GetArgument<string>("mode");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.PasteLineageNode(userId, botModelName, parent, nodes, mode));
                });

            //  CreatePhrase
            FieldAsync<LineageNodeDefinitionType>("createPhrase",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "path" },
                new QueryArgument<NonNullGraphType<LineageNodeAttributeUpdateType>> { Name = "attribute" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var path = context.GetArgument<string>("path");
                    var attribute = context.GetArgument<LineageNodeAttributes>("attribute");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreatePhrase(userId, botModelName, path, attribute));
                });
            //  DeletePhrase
            FieldAsync<LineageNodeAttributeType>("deletePhrase",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "phrase" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var phrase = context.GetArgument<string>("phrase");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.DeletePhrase(userId, botModelName, phrase));
                });


            //  SaveAttributes
            FieldAsync<LineageNodeAttributeType>("updateAttribute",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                new QueryArgument<NonNullGraphType<LineageNodeAttributeUpdateType>> { Name = "attribute" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "path" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var path = context.GetArgument<string>("path");
                    var attribute = context.GetArgument<LineageNodeAttributeUpdate>("attribute");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateAttribute(userId, botModelName, path, attribute));
                });
            //                
            // ServiceConnectivity
            //  Edit AzureCredentials
            FieldAsync<AzureCredentialsType>("updateAzureCredentials",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ModelName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "apiKey" },
                new QueryArgument<NonNullGraphType<ModelTypeEnum>> { Name = "modelType" }
                ),
                resolve: async context =>
                {
                    var ModelName = context.GetArgument<string>("ModelName");
                    var apiKey = context.GetArgument<string>("apiKey");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var modelType = context.GetArgument<ModelType>("modelType");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateAzureCredentials(userId,ModelName, apiKey, modelType));
                });
            FieldAsync<AzureCredentialsType>("deleteAzureCredentials",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ModelName" },
                new QueryArgument<NonNullGraphType<ModelTypeEnum>> { Name = "modelType" }
                ),
                resolve: async context =>
                {
                    var ModelName = context.GetArgument<string>("ModelName");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var modelType = context.GetArgument<ModelType>("modelType");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.DeleteAzureCredentials(userId, ModelName, modelType));
                });
            //  Edit SellerCenter
            FieldAsync<SellerCenterCredentialsType>("updateSellerCenterCredentials",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                new QueryArgument<NonNullGraphType<BooleanGraphType>> { Name = "liveMode" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "merchantId" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "stripeApiKey" },
                new QueryArgument<NonNullGraphType<ModelTypeEnum>> { Name = "modelType" }
                ),
                resolve: async context =>
                {
                    var ModelName = context.GetArgument<string>("ModelName");
                    var liveMode = context.GetArgument<bool>("liveMode");
                    var merchantId = context.GetArgument<string>("merchantId");
                    var stripeApiKey = context.GetArgument<string>("stripeApiKey");
                    var modelType = context.GetArgument<ModelType>("modelType");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateSellerCenterCredentials(userId, ModelName, liveMode, merchantId, stripeApiKey, modelType));
                });
            FieldAsync<SellerCenterCredentialsType>("deleteSellerCenterCredentials",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ModelName" },
                new QueryArgument<NonNullGraphType<ModelTypeEnum>> { Name = "modelType" }
                ),
                resolve: async context =>
                {
                    var ModelName = context.GetArgument<string>("ModelName");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var modelType = context.GetArgument<ModelType>("modelType");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.DeleteSellereCenterCredentials(userId, ModelName, modelType));
                });
            //  Twilio
            FieldAsync<TwilioCredentialsType>("updateTwilioCredentials",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ModelName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "sMSAccountFrom" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "sMSAccountIdentification" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "sMSAccountPassword" },
                new QueryArgument<NonNullGraphType<ModelTypeEnum>> { Name = "modelType" }
                ),
                resolve: async context =>
                {
                    var ModelName = context.GetArgument<string>("ModelName");
                    var sMSAccountFrom = context.GetArgument<string>("sMSAccountFrom");
                    var sMSAccountIdentification = context.GetArgument<string>("sMSAccountIdentification");
                    var sMSAccountPassword = context.GetArgument<string>("sMSAccountPassword");
                    var modelType = context.GetArgument<ModelType>("modelType");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateTwilioCredentials(userId, ModelName, sMSAccountFrom, sMSAccountIdentification, sMSAccountPassword, modelType));
                });
            FieldAsync<TwilioCredentialsType>("deleteTwilioCredentials",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ModelName" },
                new QueryArgument<NonNullGraphType<ModelTypeEnum>> { Name = "modelType" }
                ),
                resolve: async context =>
                {
                    var ModelName = context.GetArgument<string>("ModelName");
                    var modelType = context.GetArgument<ModelType>("modelType");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.DeleteTwilioCredentials(userId, ModelName, modelType));
                });
            //  SendGrid
            FieldAsync<SendGridCredentialsType>("updateSendgridCredentials",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ModelName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "sendGridAPIKey" },
                new QueryArgument<NonNullGraphType<ModelTypeEnum>> { Name = "modelType" }
                ),
                resolve: async context =>
                {
                    var ModelName = context.GetArgument<string>("ModelName");
                    var sendGridAPIKey = context.GetArgument<string>("sendGridAPIKey");
                    var modelType = context.GetArgument<ModelType>("modelType");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateSendgridCredentials(userId, ModelName, sendGridAPIKey, modelType));
                });
            FieldAsync<SendGridCredentialsType>("deleteSendgridCredentials",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ModelName" },
                new QueryArgument<NonNullGraphType<ModelTypeEnum>> { Name = "modelType" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var modelType = context.GetArgument<ModelType>("modelType");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.DeleteSendgridCredentials(userId, botModelName,modelType));
                });
            //  GraphQL
            FieldAsync<GraphQLCredentialsType>("updateGraphQLCredentials",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ModelName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "url" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "header" },
                new QueryArgument<NonNullGraphType<ModelTypeEnum>> { Name = "modelType" }
                ),
                resolve: async context =>
                {
                    var ModelName = context.GetArgument<string>("ModelName");
                    var url = context.GetArgument<string>("url");
                    var header = context.GetArgument<string>("header");
                    var modelType = context.GetArgument<ModelType>("modelType");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateGraphQLCredentials(userId, ModelName, url,header, modelType));
                });
            FieldAsync<GraphQLCredentialsType>("deleteGraphQLCredentials",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ModelName" },
                new QueryArgument<NonNullGraphType<ModelTypeEnum>> { Name = "modelType" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var modelType = context.GetArgument<ModelType>("modelType");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.DeleteGraphQLCredentials(userId, botModelName, modelType));
                });

            // Contact
            //  Create
            Field<ContactType>(
                "createContact",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<ContactInputType>> { Name = "contact" }),
                resolve: context =>
                {
                    var contactInput = context.GetArgument<ContactInput>("contact");
                    var id = Guid.NewGuid().ToString();
                    var contact = new Contact { Company = contactInput.Company, Country = contactInput.Country, Created = DateTime.Now, Email = contactInput.Email.ToLower(), FirstName = contactInput.FirstName, IntroSent = contactInput.IntroSent, LastName = contactInput.LastName, Notes = contactInput.Notes, Phone = contactInput.Phone, Id = id, Sector = contactInput.Sector, Source = contactInput.Source, Title = contactInput.Title };
                    return connectivity.CreateContactAsync(contact);
                }
            ).AuthorizeWith("AdminPolicy");
            //  Update
            Field<ContactType>(
                    "updateContact",
                    arguments: new QueryArguments(
                        new QueryArgument<NonNullGraphType<ContactUpdateType>> { Name = "contact" }),
                    resolve: context =>
                    {
                        var contactUpdate = context.GetArgument<ContactUpdate>("contact");
                        var contact = new Contact { Company = contactUpdate.Company, Country = contactUpdate.Country, Email = contactUpdate.Email, FirstName = contactUpdate.FirstName, IntroSent = contactUpdate.IntroSent, LastName = contactUpdate.LastName, Notes = contactUpdate.Notes, Phone = contactUpdate.Phone, Id = contactUpdate.Id, Sector = contactUpdate.Sector, Source = contactUpdate.Source, Title = contactUpdate.Title };
                        return connectivity.UpdateContactAsync(contact);
                    }
                ).AuthorizeWith("AdminPolicy");
            //  Delete
            Field<ContactType>(
                "deleteContact",
                arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" }),
                    resolve: context =>
                    {
                        var emailname = context.GetArgument<string>("email");
                        var contact = connectivity.GetContactById(emailname);
                        connectivity.DeleteContactAsync(emailname);
                        return contact;
                    }
               ).AuthorizeWith("AdminPolicy");
            // Default
            //  Create
            FieldAsync<DefaultType>("createDefault", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }, new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "value" }), 
                resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var value = context.GetArgument<string>("value");
                return await context.TryAsyncResolve(
                    async c => await connectivity.CreateDefault(name, value));
            }).AuthorizeWith("AdminPolicy");
            FieldAsync<DefaultType>("updateDefault", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }, new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "value" }),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var value = context.GetArgument<string>("value");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateDefault(name, value));
                }).AuthorizeWith("AdminPolicy");
            //  Delete
            FieldAsync<DefaultType>("deleteDefault", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.DeleteDefault(name));
                }).AuthorizeWith("AdminPolicy");
            //MLModel
            //  Create MLSpec as an object
            FieldAsync<MLModelType>("createEmptyMLModel", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var userId = connectivity.GetCurrentUserId(context.UserContext);
                return await context.TryAsyncResolve(
                    async c => await connectivity.CreateEmptyMLModel(userId, name));
            });
            //  Update
            FieldAsync<MLModelType>("updateMLModel", arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" },
                new QueryArgument<NonNullGraphType<MLSpecUpdateType>> { Name = "mlspec" }
                ), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var mlspec = context.GetArgument<MLSpecUpdate>("mlspec");
                var userId = connectivity.GetCurrentUserId(context.UserContext);
                return await context.TryAsyncResolve(
                    async c => await connectivity.UpdateMLSpec(userId, name, mlspec));
            });
            //  Delete
            FieldAsync<MLModelType>("deleteMLModel", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var userId = connectivity.GetCurrentUserId(context.UserContext);
                return await context.TryAsyncResolve(
                    async c => await connectivity.DeleteMLModel(userId, name));
            });

            // BotFormat
            //  CreateUpdateConstant
            FieldAsync<StringDoublePairType>("createUpdateConstant", 
                arguments: 
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" }, 
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" },
                    new QueryArgument<NonNullGraphType<FloatGraphType>> { Name = "value" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var name = context.GetArgument<string>("name");
                    var value = context.GetArgument<double>("value");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateUpdateConstant(userId, botModelName, name, value));
                });
            //  Delete constant
            FieldAsync<StringDoublePairType>("deleteConstant", arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var name = context.GetArgument<string>("name");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.DeleteConstant(userId, botModelName, name));
                });

            //  CreateUpdateStore
            FieldAsync<StringGraphType>("createUpdateStore",
                arguments:
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var name = context.GetArgument<string>("name");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateUpdateStore(userId, botModelName, name));
                });
            //  Delete store
            FieldAsync<StringDoublePairType>("deleteStore", arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var name = context.GetArgument<string>("name");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.DeleteStore(userId, botModelName, name));
                });
            //  Add, update Delete sequences
            //  Add, update Delete strings
            FieldAsync<StringStringPairType>("createUpdateString",
                arguments:
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "value" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var name = context.GetArgument<string>("name");
                    var value = context.GetArgument<string>("value");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateUpdateString(userId, botModelName, name, value));
                });
            //  Delete constant
            FieldAsync<StringStringPairType>("deleteString", arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var name = context.GetArgument<string>("name");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.DeleteString(userId, botModelName, name));
                });
            // BotInputFormat
            //  Update
            FieldAsync<BotInputFormatType>("updateBotFormInputFormat",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "inputName" },
                new QueryArgument<NonNullGraphType<InputFormatUpdateType>> { Name = "inputUpdate" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var inputName = context.GetArgument<string>("inputName");
                    var inputUpdate = context.GetArgument<InputFormatUpdate>("inputUpdate");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateBotModelInputFormat(userId, botModelName, inputName, inputUpdate));
                });

            //  BotOutputFormat
            FieldAsync<BotOutputFormatType>("updateBotFormOutputFormat",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "outputName" },
                new QueryArgument<NonNullGraphType<BotOutputFormatUpdateType>> { Name = "outputUpdate" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var outputName = context.GetArgument<string>("outputName");
                    var outputUpdate = context.GetArgument<BotOutputFormatUpdate>("outputUpdate");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateBotModelOutputFormat(userId, botModelName, outputName, outputUpdate));
                });
            // RuleForm
            //  create/update from DARL
            FieldAsync<RuleFormType>("updateRuleSetDarl", "Updates the DARL code and rebuilds the input/output and language definitions to suit.", 
                arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "Ruleset to update"}, 
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "darl", Description = "DARL code to update" }), 
                resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var darl = context.GetArgument<string>("darl");
                var userId = connectivity.GetCurrentUserId(context.UserContext);
                return await context.TryAsyncResolve(
                    async c => await connectivity.CreateRuleFormFromDarl(userId, name, darl));
            });
            // FormFormat
            //  Update Input
            FieldAsync<InputFormatType>("updateRuleSetInputFormat", "Update the format of an input",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName" }, 
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "inputName" },
                new QueryArgument<NonNullGraphType<InputFormatUpdateType>> { Name = "inputUpdate" }
                ), 
                resolve: async context =>
            {
                var ruleSetName = context.GetArgument<string>("ruleSetName");
                var inputName = context.GetArgument<string>("inputName");
                var inputUpdate = context.GetArgument<InputFormatUpdate>("inputUpdate");
                var userId = connectivity.GetCurrentUserId(context.UserContext);
                return await context.TryAsyncResolve(
                    async c => await connectivity.UpdateRuleFormInputFormat(userId, ruleSetName, inputName, inputUpdate));
            });

            //  Update Output
            FieldAsync<OutputFormatType>("updateRuleSetOutputFormat", "Update the format of an output",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "outputName" },
                new QueryArgument<NonNullGraphType<OutputFormatUpdateType>> { Name = "outputUpdate" }
                ),
                resolve: async context =>
                {
                    var ruleSetName = context.GetArgument<string>("ruleSetName");
                    var outputName = context.GetArgument<string>("outputName");
                    var outputUpdate = context.GetArgument<OutputFormatUpdate>("outputUpdate");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateRuleFormOutputFormat(userId, ruleSetName, outputName, outputUpdate));
                });

            // Language
            //  update text
            FieldAsync<LanguageTextType>("updateRuleSetLanguageText", "Update the text displayed for a given interaction",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "languageName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "languageText" }
                ),
                resolve: async context =>
                {
                    var ruleSetName = context.GetArgument<string>("ruleSetName");
                    var languageName = context.GetArgument<string>("languageName");
                    var languageText = context.GetArgument<string>("languageText");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.UpdateRuleFormLanguageText(userId, ruleSetName, languageName, languageText));
                });
            //  update variant
            FieldAsync<VariantTextType>("updateRuleSetVariantText","Update the text for a given interaction in a given language",
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
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.UpdateRuleFormVariantText(userId, ruleSetName, languageName, isoLanguageName, variantText));
                });
            //  Update Trigger
            FieldAsync<TriggerViewType>("updateRuleSetTrigger", "Update the events that occur when a ruleset completes",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName" },
                new QueryArgument<NonNullGraphType<TriggerViewInputType>> { Name = "trigger" }
                ),
                resolve: async context =>
                {
                    var ruleSetName = context.GetArgument<string>("ruleSetName");
                    var trigger = context.GetArgument<TriggerViewInput>("trigger");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateRuleFormTrigger(userId, ruleSetName, trigger));
                });
            FieldAsync<ModelDetailsType>("updateRuleSetDetails", "Update the details, author, copyright, pricing etc. of a ruleset",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName", Description = "The name of the ruleset to update" },
                new QueryArgument<NonNullGraphType<ModelDetailsInputType>> { Name = "details", Description = "the details to change"}
                ),
                resolve: async context =>
                {
                    var ruleSetName = context.GetArgument<string>("ruleSetName");
                    var details = context.GetArgument<ModelDetails>("details");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateRuleFormDetails(userId, ruleSetName, details));
                });
            // RuleSet
            //  Create Empty
            FieldAsync<RuleSetType>("createEmptyRuleSet", "Create an empty rule set and set default values",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), 
                resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var userId = connectivity.GetCurrentUserId(context.UserContext);
                return await context.TryAsyncResolve(
                    async c => await connectivity.CreateEmptyRuleSet(userId, name));
            });
            //  Delete
            FieldAsync<RuleSetType>("deleteRuleSet", "Delete a ruleset",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), 
                resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var userId = connectivity.GetCurrentUserId(context.UserContext);
                return await context.TryAsyncResolve(
                    async c => await connectivity.DeleteRuleSet(userId, name));
            });
            // DarlUser
            //   Create/update
            //   Delete
            Field<DarlUserType>(
                "createUser",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<DarlUserInputType>> { Name = "user" }
                    ),
                resolve: context =>
                {

                    var darlUser = context.GetArgument<DarlUserInput>("user");
                    return connectivity.CreateUserAsync(darlUser);
                }
            ).AuthorizeWith("AdminPolicy");
            //  Update
            Field<DarlUserType>(
                    "updateUser",
                    arguments: new QueryArguments(
                     new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "userId" },
                       new QueryArgument<NonNullGraphType<DarlUserUpdateType>> { Name = "user" }
                       ),
                    resolve: context =>
                    {
                        var userId = context.GetArgument<string>("userId");
                        var darlUserUpdate = context.GetArgument<DarlUserUpdate>("user");
                        return connectivity.UpdateUserAsync(userId, darlUserUpdate);
                    }
                ).AuthorizeWith("AdminPolicy");
            //  Delete
            Field<DarlUserType>(
                "deleteUser",
                arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "userId" }),
                    resolve: context =>
                    {
                        var userId = context.GetArgument<string>("userId");
                        return connectivity.DeleteUser(userId);
                    }
               ).AuthorizeWith("AdminPolicy");

            // Actions

            //                Test ruleset

            //                Machine learning run
            FieldAsync<MLModelType>("machineLearnModel",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "mlmodelname" }
                ),
                resolve: async context =>
                {
                    var mlmodelname = context.GetArgument<string>("mlmodelname");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.MachineLearnModel(userId, mlmodelname));
                });
            //                 Update DARL
            FieldAsync<StringGraphType>("updateDarlInRuleset",
                 arguments: 
                 new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "darl" }
                 ),
                 resolve: async context =>
                 {
                     var ruleSetName = context.GetArgument<string>("ruleSetName");
                     var darl = context.GetArgument<string>("darl");
                     var userId = connectivity.GetCurrentUserId(context.UserContext);
                     return await context.TryAsyncResolve(
                                     async c => await connectivity.UpdateDarlInRuleset(userId, ruleSetName, darl));
                 });
            //              FactoryReset
            Field<BooleanGraphType>(
             "factoryReset",
             resolve: context =>
                 {
                     var userId = connectivity.GetCurrentUserId(context.UserContext);
                     return connectivity.FactoryReset(userId);
                 }
            );
            FieldAsync<StringGraphType>(
               "resetApiKey","Regenerate your API key",
               resolve: async context =>
               {
                   var userId = connectivity.GetCurrentUserId(context.UserContext);

                   return await context.TryAsyncResolve(
                       async c => await connectivity.UpdateUserAPIKey(userId));
               }
            );
            FieldAsync<CollateralType>(
                "updateCollateral",
                "update text used in responses",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the collateral" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "value", Description = "The value of the collateral" }
                ),
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var name = context.GetArgument<string>("name");
                    var value = context.GetArgument<string>("value");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateCollateral(userId, name, value ));
                }
            );
            FieldAsync<CollateralType>(
                "deleteCollateral",
                "Delete text used in responses",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the collateral" }
                ),
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var name = context.GetArgument<string>("name");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.DeleteCollateral(userId, name));
                }
            );
            FieldAsync<DateTimeGraphType>(
                "setLastUpdate",
                "Set the utc time of a system wide update.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "from", Description = "The source of the update" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "to", Description = "The destination of the update" }
                ),
                resolve: async context =>
                {
                    var from = context.GetArgument<string>("from");
                    var to = context.GetArgument<string>("to");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.SetLastUpdate(from, to));
                }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<BooleanGraphType>(
                "createSupportRequest",
                "Create a support request in the darl support system",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "customerName", Description = "Person reporting the bug or request" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "customerEmail", Description = "Email for Dr Andy to respond to" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "text", Description = "Request or bug text" }
                ),
                resolve: async context =>
                {
                    var customerName = context.GetArgument<string>("customerName");
                    var customerEmail = context.GetArgument<string>("customerEmail");
                    var text = context.GetArgument<string>("text");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateSupportRequest(customerName, customerEmail,text, _config["AppSettings:AzureProjectForWorkItem"]));
                }
            );
            FieldAsync<ConversationType>(
                "createConversation",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<ConversationInputType>> { Name = "conversation" }),
                resolve: async context =>
                {
                    var conversationInput = context.GetArgument<Conversation>("conversation");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateConversation(conversationInput));
                }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<UserUsageType>(
                "createUserUsage",
                "Attach a day of usage to a user",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<DateTimeGraphType>> { Name = "date", Description = "The date of the usage" },
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "count", Description = "The count of the usages" },
                     new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "userId", Description = "The user responsible for the usages" }
               ),
                resolve: async context =>
                {
                    var date = context.GetArgument<DateTime>("date");
                    var count = context.GetArgument<int>("count");
                    var userId = context.GetArgument<string>("userId");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateUserUsage(date,count,userId));
                }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<UserUsageType>(
                "createBotModelUsage",
                "Attach a day of usage to a user's botmodel",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<DateTimeGraphType>> { Name = "date", Description = "The date of the usage" },
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "count", Description = "The count of the usages" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "userId", Description = "The user responsible for the usages" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "model", Description = "The botmodel the usages relate to" }
               ),
                resolve: async context =>
                {
                    var date = context.GetArgument<DateTime>("date");
                    var count = context.GetArgument<int>("count");
                    var userId = context.GetArgument<string>("userId");
                    var model = context.GetArgument<string>("model");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateBotModelUsage(date, count, userId, model));
                }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<UserUsageType>(
                "createKGModelUsage",
                "Attach a day of usage to a user's Knowledge graph",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<DateTimeGraphType>> { Name = "date", Description = "The date of the usage" },
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "count", Description = "The count of the usages" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "userId", Description = "The user responsible for the usages" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "model", Description = "The KGraph the usages relate to" }
               ),
                resolve: async context =>
                {
                    var date = context.GetArgument<DateTime>("date");
                    var count = context.GetArgument<int>("count");
                    var userId = context.GetArgument<string>("userId");
                    var model = context.GetArgument<string>("model");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateKGModelUsage(date, count, userId, model));
                }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<UserUsageType>(
                "createRulesetUsage",
                "Attach a day of usage to a user's ruleset",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<DateTimeGraphType>> { Name = "date", Description = "The date of the usage" },
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "count", Description = "The count of the usages" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "userId", Description = "The user responsible for the usages" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "model", Description = "The ruleset the usages relate to" }
               ),
                resolve: async context =>
                {
                    var date = context.GetArgument<DateTime>("date");
                    var count = context.GetArgument<int>("count");
                    var userId = context.GetArgument<string>("userId");
                    var model = context.GetArgument<string>("model");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateRuleSetUsage(date, count, userId, model));
                }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<UserUsageType>(
                "createMLModelUsage",
                "Attach a day of usage to a user's ml model",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<DateTimeGraphType>> { Name = "date", Description = "The date of the usage" },
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "count", Description = "The count of the usages" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "userId", Description = "The user responsible for the usages" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "model", Description = "The ml model the usages relate to" }
               ),
                resolve: async context =>
                {
                    var date = context.GetArgument<DateTime>("date");
                    var count = context.GetArgument<int>("count");
                    var userId = context.GetArgument<string>("userId");
                    var model = context.GetArgument<string>("model");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateMLModelUsage(date, count, userId, model));
                }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<UserUsageType>(
                "createSimulationUsage",
                "Attach a day of usage to a user's simulation",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<DateTimeGraphType>> { Name = "date", Description = "The date of the usage" },
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "count", Description = "The count of the usages" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "userId", Description = "The user responsible for the usages" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "model", Description = "The ruleset the usages relate to" }
               ),
                resolve: async context =>
                {
                    var date = context.GetArgument<DateTime>("date");
                    var count = context.GetArgument<int>("count");
                    var userId = context.GetArgument<string>("userId");
                    var model = context.GetArgument<string>("model");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateSimulationUsage(date, count, userId, model));
                }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<UserUsageType>(
                "createBotUsage",
                "Attach a day of usage to a user",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<DateTimeGraphType>> { Name = "date", Description = "The date of the usage" },
                    new QueryArgument<NonNullGraphType<IntGraphType>> { Name = "count", Description = "The count of the usages" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "userId", Description = "The user responsible for the usages" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botId", Description = "The bot responsible for the usages" }
               ),
                resolve: async context =>
                {
                    var date = context.GetArgument<DateTime>("date");
                    var count = context.GetArgument<int>("count");
                    var userId = context.GetArgument<string>("userId");
                    var botId = context.GetArgument<string>("botId");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateBotUsage(date, count, userId, botId));
                }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<DocumentType>(
                "deleteDocument",
                "Delete a document used as a template",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the document" }
                ),
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var name = context.GetArgument<string>("name");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.DeleteDocument(userId, name));
                }
            );
            FieldAsync<DarlVarType>(
                "updateRulesetPreload",
                "Add or update a preloaded value to a ruleset",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "rulesetName", Description = "Name of the ruleset" },
                    new QueryArgument<NonNullGraphType<DarlVarInputType>> { Name = "preloadData", Description = "The data to load" }
               ),
                resolve: async context =>
                {
                    var rulesetName = context.GetArgument<String>("rulesetName");
                    var preloadData = context.GetArgument<DarlVar>("preloadData");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateRulesetPreload(userId, rulesetName, preloadData));
                }
            );
            FieldAsync<IntGraphType>(
                "mailshot",
                "send a mailshot",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "collateral", Description = "Collateral to use for the body" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "subject", Description = "Email subject" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "sendfrom", Description = "Source email" },
                    new QueryArgument<StringGraphType> { Name = "filter", Description = "Filter expression" },
                    new QueryArgument<BooleanGraphType> { Name = "test", DefaultValue=false, Description = "if true no emails are sent"}
                    ),
                resolve: async context =>
                {
                    var collateral = context.GetArgument<String>("collateral");
                    var subject = context.GetArgument<String>("subject");
                    var sendfrom = context.GetArgument<String>("sendfrom");
                    var filter = context.GetArgument<String>("filter");
                    var test = context.GetArgument<bool>("test");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await email.Mailshot(userId, collateral, subject, sendfrom, filter, test));
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<StringGraphType>(
                "email",
                "send an email",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "body", Description = "Body of the email" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "subject", Description = "Email subject" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "sendfrom", Description = "Source email" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "emailAddress", Description = "email of recipient" }
                    ),
                resolve: async context =>
                {
                    var body = context.GetArgument<String>("body");
                    var subject = context.GetArgument<String>("subject");
                    var sendfrom = context.GetArgument<String>("sendfrom");
                    var emailAddress = context.GetArgument<String>("emailAddress");
                    return await context.TryAsyncResolve(
                        async c => await email.SendEmail(body, subject, sendfrom, emailAddress));
                }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<StringGraphType>(
                "copyToReserveAccount",
                "Copy a resource in the current account to the reserve account",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<ResourceTypeEnum>> { Name = "resourceType", Description = "The kind of resource to copy" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name in the current account" },
                    new QueryArgument<StringGraphType> { Name = "newName", Description = "name in reserve account (null copies same name)" }
                    ),
                resolve: async context =>
                {
                    var resourceType = context.GetArgument<ResourceType>("resourceType");
                    var name = context.GetArgument<String>("name");
                    var newName = context.GetArgument<String>("newName");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CopyToReserveAccount(userId, resourceType, name, newName));
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<PurchaseType>(
                "reportPurchase",
                "Associate a purchase with a contact",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email", Description = "The purchasers email" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the purchaser" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "sessionId", Description = "The stripe id of the purchase)"},
                    new QueryArgument<NonNullGraphType<DateTimeGraphType>> { Name = "date", Description = "The date and time of the purchase)" }
                    ),
                resolve: async context =>
                {
                    var emailname = context.GetArgument<string>("email");
                    var name = context.GetArgument<String>("name");
                    var sessionId = context.GetArgument<String>("sessionId");
                    var date = context.GetArgument<DateTime>("date");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.ReportPurchase(emailname, name,sessionId,date));
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<ListGraphType<DarlVarType>>("inferFromDarl", "Make an inference with the attached darl code and attached inputs",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "code", Description="DARL source to infer from" },
                new QueryArgument<NonNullGraphType<ListGraphType<DarlVarInputType>>> { Name = "inputs" }
                ),
                resolve: async context =>
                {
                    var code = context.GetArgument<string>("code");
                    var inputs = context.GetArgument<List<DarlVarInput>>("inputs");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.InferFromDarlDarlVar(userId, code, inputs));
                });
            FieldAsync<GraphObjectType>("createGraphObject", "Add a new graph object", arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                    new QueryArgument<NonNullGraphType<GraphObjectInputType>> { Name = "graphObject", Description = "The object to add" },
                    new QueryArgument<OntologyActionEnum> { Name = "ontology", Description = "builds, checks against or ignores ontology" }
               ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var graphObject = context.GetArgument<GraphObjectInput>("graphObject");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var ontology = context.GetArgument<OntologyAction>("ontology");
                    return await context.TryAsyncResolve(
                        async c => await graph.CreateGraphObject(CompositeName(userId,graphName), graphObject, ontology));
                }
            ).AuthorizeWith("CorpPolicy");
            FieldAsync<GraphConnectionType>("createGraphConnection", "Add a new graph connection", arguments: new QueryArguments(
                     new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph to modify" },
                   new QueryArgument<NonNullGraphType<GraphConnectionInputType>> { Name = "graphConnection", Description = "The connection to add" },
                    new QueryArgument<OntologyActionEnum> { Name = "ontology", Description = "builds, checks against or ignores ontology" }
               ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var graphConnection = context.GetArgument<GraphConnectionInput>("graphConnection");
                    var ontology = context.GetArgument<OntologyAction>("ontology");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.CreateGraphConnection(CompositeName(userId, graphName), graphConnection,ontology));
                }
            ).AuthorizeWith("CorpPolicy");
            FieldAsync<GraphObjectType>("deleteGraphObject", "Delete a graphObject", arguments: new QueryArguments(
                     new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph to modify" },
                     new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The id of the object to delete" }
                ),
                 resolve: async context =>
                 {
                     var graphName = context.GetArgument<string>("graphName");
                     var id = context.GetArgument<string>("id");
                     var userId = connectivity.GetCurrentUserId(context.UserContext);
                     return await context.TryAsyncResolve(
                         async c => await graph.DeleteGraphObject(CompositeName(userId, graphName), id));
                 }
             ).AuthorizeWith("CorpPolicy");
            FieldAsync<GraphConnectionType>("deleteGraphConnection", "Delete a graph connection", arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph to modify" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The id of the connection to delete" }
               ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var id = context.GetArgument<string>("id");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.DeleteGraphConnection(CompositeName(userId, graphName), id));
                }
            ).AuthorizeWith("CorpPolicy");
            FieldAsync<GraphObjectType>("updateGraphObject", "Update a graph object", arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph to modify" },
                    new QueryArgument<NonNullGraphType<GraphObjectUpdateType>> { Name = "graphObject", Description = "The object to update" },
                    new QueryArgument<OntologyActionEnum> { Name = "ontology", Description = "builds, checks against or ignores ontology" }
                ),
                 resolve: async context =>
                 {
                     var graphName = context.GetArgument<string>("graphName");
                     var graphObject = context.GetArgument<GraphObjectUpdate>("graphObject");
                     var userId = connectivity.GetCurrentUserId(context.UserContext);
                     var ontology = context.GetArgument<OntologyAction>("ontology");
                     return await context.TryAsyncResolve(
                         async c => await graph.UpdateGraphObject(CompositeName(userId, graphName), graphObject,ontology));
                 }
             ).AuthorizeWith("CorpPolicy");
            FieldAsync<GraphConnectionType>("updateGraphConnection", 
                    "Update a graph connection", arguments: new QueryArguments(
                        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph to modify" },
                        new QueryArgument<NonNullGraphType<GraphConnectionUpdateType>> { Name = "graphConnection", Description = "The connection to update" },
                        new QueryArgument<OntologyActionEnum> { Name = "ontology", Description = "builds, checks against or ignores ontology" }
                   ),
                    resolve: async context =>
                    {
                        var graphName = context.GetArgument<string>("graphName");
                        var graphConnection = context.GetArgument<GraphConnectionUpdate>("graphConnection");
                        var userId = connectivity.GetCurrentUserId(context.UserContext);
                        var ontology = context.GetArgument<OntologyAction>("ontology");
                        return await context.TryAsyncResolve(
                            async c => await graph.UpdateGraphConnection(CompositeName(userId, graphName), graphConnection,ontology));
                    }
                ).AuthorizeWith("CorpPolicy");
            FieldAsync<SubscriptionTypeEnum>("updateSubscriptionType", "Change your subscription type",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<SubscriptionTypeEnum>> { Name = "type" }), 
                resolve: async context =>
                {
                    var type = context.GetArgument<DarlUser.SubscriptionType>("type");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);

                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateSubscriptionType(userId, type));
                });
            FieldAsync<BooleanGraphType>("closeAccount","Close your account. Charges incurred will be billed immediately.",
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);

                    return await context.TryAsyncResolve(
                        async c => await connectivity.CloseAccount(userId));
                });

            FieldAsync<StringGraphType>("createKey", "Create a licensing key", arguments: new QueryArguments(
                        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "company", Description = "The company granted the license" },
                        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email", Description = "The email address of the company" },
                        new QueryArgument<NonNullGraphType<DateTimeGraphType>> { Name = "endDate", Description = "The date the license ends" }
                   ),
                    resolve: async context =>
                    {
                        var company = context.GetArgument<string>("company");
                        var userId = connectivity.GetCurrentUserId(context.UserContext);
                        var email = context.GetArgument<string>("email");
                        var endDate = context.GetArgument<DateTime>("endDate");

                        return await context.TryAsyncResolve(
                            async c => await connectivity.CreateKey(userId,company,email,endDate));
                    }
                ).AuthorizeWith("AdminPolicy");
            FieldAsync<StringGraphType>("createSoftMatchModel", "Create a SoftMatch model from text/index pairs", arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "modelName", Description = "The unique name of the stored model for later reuse " },
                    new QueryArgument<NonNullGraphType<ListGraphType<StringStringPairInputType>>> { Name = "data", Description = "The text/index data to add to the SoftMatch model" },
                    new QueryArgument<BooleanGraphType> { Name = "rebuild", Description = "if false (default) add to existing model, otherwise create a new model.", DefaultValue = false }
                ),
                resolve: async context =>
                {
                    var treeName = context.GetArgument<string>("modelName");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var data = context.GetArgument<List<StringStringPair>>("data");

                    return await context.TryAsyncResolve(
                        async c => await cmp.CreateSoftMatchModel(userId, treeName, data));
                }
            );
            FieldAsync<StringGraphType>("deleteSoftMatchModel", "delete a SoftMatch model", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the SoftMatch model to delete" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await cmp.DeleteSoftMatchModel(userId, name));
                }
            );
            FieldAsync<StringGraphType>("createKG", "Create a Knowledge graph ", arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "modelName", Description = "The unique name of the stored model for later reuse " }
            ),
            resolve: async context =>
            {
                    var modelName = context.GetArgument<string>("modelName");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.CreateNewGraph(userId, modelName));
                }
            ).AuthorizeWith("CorpPolicy");
            FieldAsync<StringGraphType>("deleteKG", "Delete a Knowledge graph", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph to delete" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.DeleteGraph(userId, name));
                }
            ).AuthorizeWith("CorpPolicy");

            FieldAsync<KGraphType>("createKGraph", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var userId = connectivity.GetCurrentUserId(context.UserContext);
                return await context.TryAsyncResolve(
                    async c => await connectivity.CreateKGraph(userId, name));
            }).AuthorizeWith("CorpPolicy");

            FieldAsync<StringGraphType>("saveKGraph", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var userId = connectivity.GetCurrentUserId(context.UserContext);
                return await context.TryAsyncResolve(
                    async c => { await graph.Store(CompositeName(userId, name)); return ""; });
            }).AuthorizeWith("CorpPolicy");

            FieldAsync<StringGraphType>("inviteUser", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" }), resolve: async context =>
            {
                var newUserEmail = context.GetArgument<string>("email");
                var userId = connectivity.GetCurrentUserId(context.UserContext);
                return await context.TryAsyncResolve(
                    async c =>  await email.InviteUser(userId, newUserEmail));
            }).AuthorizeWith("CorpPolicy");

            FieldAsync<StringGraphType>("copyRenamKG", "copy and rename a Knowledge graph", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph to copy" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "newName", Description = "The new name of the copied Knowledge Graph" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var newName = context.GetArgument<string>("newName");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.CopyRenameKG(userId, name, newName));
                }
            ).AuthorizeWith("CorpPolicy");

            FieldAsync<GraphObjectType>("updateRecognitionObject", "update a GraphObject in the recognition trees", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<GraphObjectUpdateType>> { Name = "object", Description = "The object to update" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var obj = context.GetArgument<GraphObjectUpdate>("object");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.UpdateRecognitionObject(CompositeName(userId, name), obj));
                }
            ).AuthorizeWith("CorpPolicy");

            FieldAsync<GraphObjectType>("createRecognitionObject", "create a GraphObject in the recognition trees", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is to be in" },
                 new QueryArgument<NonNullGraphType<GraphObjectInputType>> { Name = "object", Description = "The object to create" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var obj = context.GetArgument<GraphObjectInput>("object");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.CreateRecognitionObject(CompositeName(userId, name), obj));
                }
            ).AuthorizeWith("CorpPolicy");

            FieldAsync<GraphConnectionType>("createRecognitionConnection", "create a GraphConnection in the recognition trees", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is to be in" },
                 new QueryArgument<NonNullGraphType<GraphConnectionInputType>> { Name = "connection", Description = "The connection to create" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var conn = context.GetArgument<GraphConnectionInput>("connection");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.CreateRecognitionConnection(CompositeName(userId, name), conn));
                }
            ).AuthorizeWith("CorpPolicy");

            FieldAsync<StringGraphType>("deleteRecognitionObject", "Delete a GraphObject in the recognition trees", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The id of the object to delete" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var id = context.GetArgument<string>("id");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.DeleteRecognitionObject(CompositeName(userId, name), id));
                }
            ).AuthorizeWith("CorpPolicy");

            FieldAsync<StringGraphType>("updateRecognitionObjectAttribute", "update or add an attribute of a GraphObject in the recognition trees", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The id of the parent object" },
                 new QueryArgument<NonNullGraphType<GraphAttributeInputType>> { Name = "att", Description = "The attribute to update" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var id = context.GetArgument<string>("id");
                    var att = context.GetArgument<GraphAttributeInput>("att");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.UpdateRecognitionObjectAttribute(CompositeName(userId, name), id, att));
                }
            ).AuthorizeWith("CorpPolicy");

            FieldAsync<StringGraphType>("updateVirtualObjectAttribute", "update or add an attribute of a virtual GraphObject", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "The lineage of the parent object" },
                 new QueryArgument<NonNullGraphType<GraphAttributeInputType>> { Name = "att", Description = "The attribute to update" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var lineage = context.GetArgument<string>("lineage");
                    var att = context.GetArgument<GraphAttributeInput>("att");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.UpdateVirtualObjectAttribute(CompositeName(userId, name), lineage, att));
                }
            ).AuthorizeWith("CorpPolicy");
            FieldAsync<StringGraphType>("deleteVirtualObjectAttribute", "update or add an attribute of a virtual GraphObject", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "The lineage of the parent object" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "attLineage", Description = "The lineage of the attribute to delete" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var lineage = context.GetArgument<string>("lineage");
                    var attLineage = context.GetArgument<string>("attLineage");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.DeleteVirtualObjectAttribute(CompositeName(userId, name), lineage, attLineage));
                }
            ).AuthorizeWith("CorpPolicy");
            FieldAsync<StringGraphType>("deleteRecognitionObjectAttribute", "delete an attribute of a recognition GraphObject", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The id of the parent object" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "attLineage", Description = "The lineage of the attribute to delete" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var id = context.GetArgument<string>("id");
                    var attLineage = context.GetArgument<string>("attLineage");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.DeleteRecognitionObjectAttribute(CompositeName(userId, name), id, attLineage));
                }
            ).AuthorizeWith("CorpPolicy");
            FieldAsync<StringGraphType>("deleteGraphObjectAttribute", "delete an attribute of a real GraphObject", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The id of the parent object" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "attLineage", Description = "The lineage of the attribute to delete" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var id = context.GetArgument<string>("id");
                    var attLineage = context.GetArgument<string>("attLineage");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.DeleteGraphObjectAttribute(CompositeName(userId, name), id, attLineage));
                }
            ).AuthorizeWith("CorpPolicy");
            FieldAsync<StringGraphType>("updateGraphObjectAttribute", "update or add an attribute of a real GraphObject", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The id of the parent object" },
                 new QueryArgument<NonNullGraphType<GraphAttributeInputType>> { Name = "att", Description = "The attribute to update" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var id = context.GetArgument<string>("id");
                    var att = context.GetArgument<GraphAttributeInput>("att");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.UpdateGraphObjectAttribute(CompositeName(userId, name), id, att));
                }
            ).AuthorizeWith("CorpPolicy");
            FieldAsync<GraphObjectType>("CreateRecognitionRoot", "Create a new root in the recognition trees", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "The lineage of the root" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var lineage = context.GetArgument<string>("lineage");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await graph.CreateRecognitionRoot(CompositeName(userId, name), lineage));
                }
            ).AuthorizeWith("CorpPolicy");
        }

        private string CompositeName(string userId, string graphName)
        {
            return $"{userId}_{graphName}";
        }
    }
}