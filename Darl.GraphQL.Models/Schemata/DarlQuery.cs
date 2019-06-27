using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using DarlCommon;
using GraphQL.Server.Authorization.AspNetCore;
using GraphQL.Types;
using System;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlQuery : ObjectGraphType<object>
    {
        public DarlQuery(IConnectivity connectivity, IBotProcessing bot)
        {
            Name = "Query";
            Description = "View the contents of your account.";
            FieldAsync<ListGraphType<RuleSetType>>(
                "rulesets",
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetRuleSetsAsync(userId));
                }
            );

            FieldAsync<ListGraphType<MLModelType>>(
               "mlmodels",
               resolve: async context =>
               {
                   var userId = connectivity.GetCurrentUserId(context.UserContext);

                   return await context.TryAsyncResolve(
                       async c => await connectivity.GetMlModelsAsync(userId));
               }
            );

            FieldAsync<ListGraphType<BotModelType>>(
              "botmodels",
              resolve: async context =>
              {
                  var userId = connectivity.GetCurrentUserId(context.UserContext);
                  return await context.TryAsyncResolve(
                      async c => await connectivity.GetBotModelsAsync(userId));
              }
            );

            FieldAsync<ListGraphType<ContactType>>(
              "contacts",
                  resolve: async context =>
                  {
                      return await context.TryAsyncResolve(
                                  async c => await connectivity.GetContacts());
                  }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<ListGraphType<DarlUserType>>(
              "users",
                  resolve: async context =>
                  {
                      return await context.TryAsyncResolve(
                                  async c => await connectivity.GetUsers());
                  }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<ListGraphType<ContactType>>("contactsByLastName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lastName" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetContactsByLastName(c.GetArgument<String>("lastName"))); })
                .AuthorizeWith("AdminPolicy");

            FieldAsync<ContactType>("contactByEmail",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetContactByEmail(c.GetArgument<String>("email"))); })
                .AuthorizeWith("AdminPolicy");

            FieldAsync<ListGraphType<DefaultType>>("defaults",
                resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetDefaults()); })
                .AuthorizeWith("AdminPolicy");

            FieldAsync<StringGraphType>("defaultValue",
            arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
            resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetDefaultValue(c.GetArgument<String>("name"))); })
                .AuthorizeWith("AdminPolicy");

            FieldAsync<RuleSetType>(
                "rulesetByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetRuleSet(userId, c.GetArgument<String>("name"))
                    );
                }
            );

            FieldAsync<MLModelType>(
                "mlmodelByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetMlModel(userId, c.GetArgument<String>("name"))
                    );
                }
            );

            FieldAsync<BotModelType>(
                "botmodelByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetBotModel(userId, c.GetArgument<String>("name"))
                    );
                }
            );

            FieldAsync<ListGraphType<BotConnectionType>>(
                "botConnectionsByModel",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetBotConnectivity(userId, c.GetArgument<String>("name"))
                    );
                }
            );

            FieldAsync<ListGraphType<UserUsageType>>(
                "botUsages",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "appId" }
                    ),
                resolve: async context =>
                {
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetBotUsage( c.GetArgument<String>("appId"))
                    );
                }
            ).AuthorizeWith("User");

            FieldAsync<ListGraphType<LineageNodeDefinitionType>>("getChildrenLineageNodes",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "path" },
                    new QueryArgument<NonNullGraphType<BooleanGraphType>> { Name = "isRoot" }
                    ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var path = context.GetArgument<string>("path");
                    var isRoot = context.GetArgument<bool>("isRoot");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                         async c => await connectivity.GetChildrenLineageNodes(userId, botModelName, path, isRoot));
                });

            FieldAsync<ListGraphType<LineageRecordType>>("getLineagesForWord",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "word", Description = "The word to look up" },
                    new QueryArgument<StringGraphType> { Name = "isoLanguage", DefaultValue = "en", Description = "language for lookup (Only en currently supported)" }
                    ),
                resolve: async context =>
                {
                    var isoLanguage = context.GetArgument<string>("isoLanguage");
                    var word = context.GetArgument<string>("word");
                    return await context.TryAsyncResolve(
                         async c => await connectivity.GetLineagesForWord(word, isoLanguage));
                });

            FieldAsync<LineageNodeAttributeType>("getAttribute",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "phrase" }
                    ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var phrase = context.GetArgument<string>("phrase");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                         async c => await connectivity.GetAttribute(userId, botModelName, phrase));
                });

            FieldAsync<LineageNodeAttributeType>("getAttributeFromPath",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "path" }
                    ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var path = context.GetArgument<string>("path");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                         async c => await connectivity.GetAttributeFromPath(userId, botModelName, path));
                });
            FieldAsync<ListGraphType<DarlUserType>>("usersByEmail",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetUsersByEmail(c.GetArgument<String>("email"))); })
                .AuthorizeWith("AdminPolicy");
            FieldAsync<DarlUserType>("userById",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "userId" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetUserById(c.GetArgument<String>("userId"))); })
                .AuthorizeWith("AdminPolicy");
            FieldAsync<DarlUserType>("userByStripeId",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "stripeCustomerId" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetUserByStripeId(c.GetArgument<String>("stripeCustomerId"))); })
                .AuthorizeWith("AdminPolicy");

            //                GetExampleInputs
            FieldAsync<ListGraphType<DarlVarType>>("getExampleInputs",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName" }
                ),
                resolve: async context =>
                {
                    var ruleSetName = context.GetArgument<string>("ruleSetName");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.GetExampleInputs(userId, ruleSetName));
                });
            //  Whole ruleset inference
            FieldAsync<ListGraphType<DarlVarType>>("inferFromRuleSet", "Make an inference with the selected ruleset and attached inputs",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName" },
                new QueryArgument<NonNullGraphType<ListGraphType<DarlVarInputType>>> { Name = "inputs" }
                ),
                resolve: async context =>
                {
                    var ruleSetName = context.GetArgument<string>("ruleSetName");
                    var inputs = context.GetArgument<List<DarlVar>>("inputs");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.InferFromRuleSetDarlVar(userId, ruleSetName, inputs));
                });
            //   Get darl for editing
            FieldAsync<StringGraphType>("getDarlFromRuleSet", "Gets the DARL code element of a ruleset",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName" }
                ),
                resolve: async context =>
                {
                    var ruleSetName = context.GetArgument<string>("ruleSetName");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.GetDarlFromRuleset(userId, ruleSetName));
                });
            //                Lint Ruleset
            FieldAsync<ListGraphType<DarlLintErrorType>>("lintDarl", "Read code in DARL and return any syntax errors",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "darl" },
                    new QueryArgument<StringGraphType> { Name = "skeleton" },
                    new QueryArgument<StringGraphType> { Name = "insertion" }
                    ),
                resolve: async context =>
                {
                    var darl = context.GetArgument<string>("darl");
                    var skeleton = context.GetArgument<string>("skeleton");
                    var insertion = context.GetArgument<string>("insertion");
                    return await context.TryAsyncResolve(
                                     async c => await connectivity.LintDarl(darl, skeleton, insertion));
                });
            //                Ruleset step inference
            FieldAsync<QuestionSetType>("beginQuestionnaire",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName", Description = "The ruleset to run" },
                    new QueryArgument<StringGraphType> { Name = "language", DefaultValue = "en", Description = "The ISO language" },
                    new QueryArgument<IntGraphType> { Name = "questCount", DefaultValue = 1, Description = "The number of questions to ask at a time" }
                ),
                resolve: async context =>
                {
                    var ruleSetName = context.GetArgument<string>("ruleSetName");
                    var language = context.GetArgument<string>("language");
                    var questCount = context.GetArgument<int?>("questCount");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);

                    return await context.TryAsyncResolve(
                                    async c => await connectivity.BeginQuestionnaire(userId, ruleSetName, language ?? "en", questCount ?? 1));
                });
            FieldAsync<QuestionSetType>("continueQuestionnaire",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<QuestionSetInputType>> { Name = "responses" }
                ),
                resolve: async context =>
                {
                    var responses = context.GetArgument<QuestionSetInput>("responses");
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.ContinueQuestionnaire(responses));
                });
            FieldAsync<QuestionSetType>("backtrackQuestionnaire",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ieToken" }
                ),
                resolve: async context =>
                {
                    var ieToken = context.GetArgument<string>("ieToken");
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.BacktrackQuestionnaire(ieToken));
                });
            FieldAsync<ListGraphType<InteractResponseType>>("interact",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName", Description = "The bot model to run" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "conversationId", Description = "The unique conversation identifier" },
                    new QueryArgument<NonNullGraphType<DarlVarInputType>> { Name = "conversationData", Description = "The input from the other conversers." }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var conversationId = context.GetArgument<string>("conversationId");
                    var conversationData = context.GetArgument<DarlVar>("conversationData");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await bot.InteractAsync(userId, botModelName, conversationId, conversationData));
                });
            FieldAsync<StringGraphType>(
               "getApiKey",
               "Gets the API key for this account. Only accessible to logged in users.",
               resolve: async context =>
               {
                   var userId = connectivity.GetCurrentUserId(context.UserContext);

                   return await context.TryAsyncResolve(
                       async c => (await connectivity.GetUserById(userId)).APIKey);
               }
            ).AuthorizeWith("UserPolicy");

            FieldAsync<LineageNodeAttributeResourceType>(
                "getLineageNodeAttributeResources",
                "Get the resources needed to edit ar create lineageNodeAttributes",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName", Description = "The bot model to run" }),
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var botModelName = context.GetArgument<string>("botModelName");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.getLineageNodeAttributeResources(userId, botModelName));
                }
            );
            FieldAsync<StringGraphType>(
                "getCollateral",
                "Get text used in responses",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the collateral" }),
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var name = context.GetArgument<string>("name");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetCollateral(userId, name));
                }
            );
            FieldAsync<ListGraphType<CollateralType>>(
                "collateral",
                "Get texts used in responses",
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetCollaterals(userId));
                }
            );
            FieldAsync<DateTimeGraphType>(
                "getLastUpdate",
                "Get the utc time of a system wide update.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "from", Description = "The source of the update" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "to", Description = "The destination of the update" }
                ),
                resolve: async context =>
                {
                    var from = context.GetArgument<string>("from");
                    var to = context.GetArgument<string>("to");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetLastUpdate(from,to));
                }
            );
            FieldAsync<ListGraphType<ConversationType>>(
              "conversations",
                  resolve: async context =>
                  {
                      return await context.TryAsyncResolve(
                                  async c => await connectivity.GetConversations());
                  }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<BotRuntimeModelType>(
                "getBotModelFromAppId",
                "Reference a bot model from an external id ",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "appId", Description = "The remote reference to a bot instance" }),
                resolve: async context =>
                {
                    var appId = context.GetArgument<string>("appId");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetBotModelFromAppId(appId));
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<ListGraphType<BotConnectionType>>(
               "botConnections",
               resolve: async context =>
               {
                   return await context.TryAsyncResolve(
                       async c => await connectivity.GetBotConnectionsAsync());
               }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<StringGraphType>(
                "getUserIdFromAppId",
                "Get the userId from an external id ",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "appId", Description = "The remote reference to a bot instance" }),
                resolve: async context =>
                {
                    var appId = context.GetArgument<string>("appId");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetUserIdFromAppId(appId));
                }
            ).AuthorizeWith("AdminPolicy");
        }

    }
}