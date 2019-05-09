using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlMutation : ObjectGraphType<object>
    {
        public DarlMutation(IConnectivity connectivity)
        {
            Name = "Mutation";
            Description = "Make changes to the contents of your account.";
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

            // BotConnection
            //  Create
            FieldAsync<BotConnectionType>("createBotConnection",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "appId" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "password" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var appId = context.GetArgument<string>("appId");
                    var password = context.GetArgument<string>("password");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CreateBotConnection(userId, botModelName, appId, password));
                });

            //  Delete
            FieldAsync<BotConnectionType>("deleteBotConnection",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "appId" }
                    ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var appId = context.GetArgument<string>("appId");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.DeleteBotConnection(userId, botModelName, appId));
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
                                   FieldAsync<LineageNodeAttributeType>("createPhrase",
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
                                       arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                                       new QueryArgument<NonNullGraphType<LineageNodeAttributeUpdateType>> { Name = "attribute" }
                                       ),
                                       resolve: async context =>
                                       {
                                           var botModelName = context.GetArgument<string>("botModelName");
                                           var attribute = context.GetArgument<LineageNodeAttributeUpdate>("attribute");
                                           var userId = connectivity.GetCurrentUserId(context.UserContext);
                                           return await context.TryAsyncResolve(
                                               async c => await connectivity.UpdateAttribute(userId, botModelName, attribute));
                                       });
                                   //                
                                   // ServiceConnectivity
                                   //  Edit AzureCredentials
                                   FieldAsync<AzureCredentialsType>("updateAzureCredentials",
                                       arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                                       new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "apiKey" }
                                       ),
                                       resolve: async context =>
                                       {
                                           var botModelName = context.GetArgument<string>("botModelName");
                                           var apiKey = context.GetArgument<string>("apiKey");
                                           var userId = connectivity.GetCurrentUserId(context.UserContext);
                                           return await context.TryAsyncResolve(
                                               async c => await connectivity.UpdateAzureCredentials(userId, botModelName, apiKey));
                                       });
                                   FieldAsync<AzureCredentialsType>("deleteAzureCredentials",
                                       arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" }
                                      ),
                                       resolve: async context =>
                                       {
                                           var botModelName = context.GetArgument<string>("botModelName");
                                           var userId = connectivity.GetCurrentUserId(context.UserContext);
                                           return await context.TryAsyncResolve(
                                               async c => await connectivity.DeleteAzureCredentials(userId, botModelName));
                                       });
                                   //  Edit SellerCenter
                                   FieldAsync<SellerCenterCredentialsType>("updateSellereCenterCredentials",
                                       arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                                       new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "liveMode" },
                                       new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "merchantId" },
                                       new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "stripeApiKey" }
                                       ),
                                       resolve: async context =>
                                       {
                                           var botModelName = context.GetArgument<string>("botModelName");
                                           var liveMode = context.GetArgument<bool>("liveMode");
                                           var merchantId = context.GetArgument<string>("merchantId");
                                           var stripeApiKey = context.GetArgument<string>("stripeApiKey");
                                           var userId = connectivity.GetCurrentUserId(context.UserContext);
                                           return await context.TryAsyncResolve(
                                               async c => await connectivity.UpdateSellerCenterCredentials(userId, botModelName, liveMode, merchantId, stripeApiKey));
                                       });
                                   FieldAsync<SellerCenterCredentialsType>("deleteSellereCenterCredentials",
                                       arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" }
                                      ),
                                       resolve: async context =>
                                       {
                                           var botModelName = context.GetArgument<string>("botModelName");
                                           var userId = connectivity.GetCurrentUserId(context.UserContext);
                                           return await context.TryAsyncResolve(
                                               async c => await connectivity.DeleteSellereCenterCredentials(userId, botModelName));
                                       });
                                   //  Twilio
                                   FieldAsync<TwilioCredentialsType>("updateTwilioCredentials",
                                       arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                                       new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "sMSAccountFrom" },
                                       new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "sMSAccountIdentification" },
                                       new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "sMSAccountPassword" }
                                       ),
                                       resolve: async context =>
                                       {
                                           var botModelName = context.GetArgument<string>("botModelName");
                                           var sMSAccountFrom = context.GetArgument<string>("sMSAccountFrom");
                                           var sMSAccountIdentification = context.GetArgument<string>("sMSAccountIdentification");
                                           var sMSAccountPassword = context.GetArgument<string>("sMSAccountPassword");
                                           var userId = connectivity.GetCurrentUserId(context.UserContext);
                                           return await context.TryAsyncResolve(
                                               async c => await connectivity.UpdateTwilioCredentials(userId, botModelName, sMSAccountFrom, sMSAccountIdentification, sMSAccountPassword));
                                       });
                                   FieldAsync<TwilioCredentialsType>("deleteTwilioCredentials",
                                       arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" }
                                      ),
                                       resolve: async context =>
                                       {
                                           var botModelName = context.GetArgument<string>("botModelName");
                                           var userId = connectivity.GetCurrentUserId(context.UserContext);
                                           return await context.TryAsyncResolve(
                                               async c => await connectivity.DeleteTwilioCredentials(userId, botModelName));
                                       });
                                   //  SendGrid
                                   FieldAsync<SendGridCredentialsType>("updateSendgridCredentials",
                                        arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                                        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "sendGridAPIKey" }
                                        ),
                                        resolve: async context =>
                                        {
                                            var botModelName = context.GetArgument<string>("botModelName");
                                            var sendGridAPIKey = context.GetArgument<string>("sendGridAPIKey");
                                            var userId = connectivity.GetCurrentUserId(context.UserContext);
                                            return await context.TryAsyncResolve(
                                                async c => await connectivity.UpdateSendgridCredentials(userId, botModelName, sendGridAPIKey));
                                        });
                                   FieldAsync<SendGridCredentialsType>("deleteSendgridCredentials",
                                       arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" }
                                      ),
                                       resolve: async context =>
                                       {
                                           var botModelName = context.GetArgument<string>("botModelName");
                                           var userId = connectivity.GetCurrentUserId(context.UserContext);
                                           return await context.TryAsyncResolve(
                                               async c => await connectivity.DeleteSendgridCredentials(userId, botModelName));
                                       });
                                   //  Zendesk
                                   FieldAsync<ZendeskCredentialsType>("updateZendeskCredentials",
                                        arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                                        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "zendeskApiKey" },
                                        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "zendeskURL" },
                                        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "zendeskUser" }
                                        ),
                                        resolve: async context =>
                                        {
                                            var botModelName = context.GetArgument<string>("botModelName");
                                            var zendeskApiKey = context.GetArgument<string>("zendeskApiKey");
                                            var zendeskURL = context.GetArgument<string>("zendeskURL");
                                            var zendeskUser = context.GetArgument<string>("zendeskUser");
                                            var userId = connectivity.GetCurrentUserId(context.UserContext);
                                            return await context.TryAsyncResolve(
                                                async c => await connectivity.UpdateZendeskCredentials(userId, botModelName, zendeskApiKey, zendeskURL, zendeskUser));
                                        });
                                   FieldAsync<ZendeskCredentialsType>("deleteZendeskCredentials",
                                       arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" }
                                      ),
                                       resolve: async context =>
                                       {
                                           var botModelName = context.GetArgument<string>("botModelName");
                                           var userId = connectivity.GetCurrentUserId(context.UserContext);
                                           return await context.TryAsyncResolve(
                                               async c => await connectivity.DeleteZendeskCredentials(userId, botModelName));
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
            );
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
                );
            //  Delete
            Field<ContactType>(
                "deleteContact",
                arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" }),
                    resolve: context =>
                    {
                        var email = context.GetArgument<string>("email");
                        var contact = connectivity.GetContactById(email);
                        connectivity.DeleteContactAsync(email);
                        return contact;
                    }
               );
            // Default
            //  Create
            FieldAsync<DefaultType>("createDefault", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }, new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "value" }), 
                resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var value = context.GetArgument<string>("value");
                return await context.TryAsyncResolve(
                    async c => await connectivity.CreateDefault(name, value));
            });
            FieldAsync<DefaultType>("updateDefault", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }, new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "value" }),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var value = context.GetArgument<string>("value");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.UpdateDefault(name, value));
                });
            //  Delete
            FieldAsync<DefaultType>("deleteDefault", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.DeleteDefault(name));
                });
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
                new QueryArgument<NonNullGraphType<InputFormatUpdateType>> { Name = "outputUpdate" }
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
            // RuleSet
            //  Create Empty
            FieldAsync<MLModelType>("createEmptyRuleSet", "Create an empty rule set and set default values",
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
            FieldAsync<MLModelType>("deleteRuleSet", "Delete a ruleset",
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
            );
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
                );
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
               );

            // Actions

            //                Test ruleset


            //                BotModel step inference
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

        }
    }
}