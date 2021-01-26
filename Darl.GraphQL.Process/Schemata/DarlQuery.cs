using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Middleware;
using Darl.GraphQL.Models.Models;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase;
using DarlCommon;
using DarlLanguage.Processing;
using GraphQL.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlQuery : ObjectGraphType<object>
    {
        public DarlQuery(IConnectivity connectivity, IBotProcessing bot, IFormProcessing form, ISimProcessing sim, IGraphProcessing graph, ISoftMatchProcessing cmp, ILocalStore graphStore)
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
               "The set of Bot models for this account.",
             resolve: async context =>
              {
                  var userId = connectivity.GetCurrentUserId(context.UserContext);
                  return await context.TryAsyncResolve(
                      async c => await connectivity.GetBotModelsAsync(userId));
              }
            );

            FieldAsync<ListGraphType<KGraphType>>(
              "kgraphs",
              "The set of Knowledge Graphs for this account.",
              resolve: async context =>
              {
                  var userId = connectivity.GetCurrentUserId(context.UserContext);
                  return await context.TryAsyncResolve(
                      async c => await connectivity.GetKGraphsAsync(userId));
              }
            );

            FieldAsync<ListGraphType<ContactType>>(
              "contacts",
               "The set of contacts.",
                  resolve: async context =>
                  {
                      return await context.TryAsyncResolve(
                                  async c => await connectivity.GetContacts());
                  }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<ListGraphType<ContactType>>(
              "recentContacts",
                  resolve: async context =>
                  {
                      return await context.TryAsyncResolve(
                                  async c => await connectivity.GetRecentContacts());
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

            FieldAsync<KGraphType>(
                "kGraphByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetKGModel(userId, c.GetArgument<String>("name"))
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
            ).AuthorizeWith("UserPolicy");

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

            FieldAsync<StringGraphType>("getTypeWordForLineage",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "The lineage to look up" },
                    new QueryArgument<StringGraphType> { Name = "isoLanguage", DefaultValue = "en", Description = "language for lookup (Only en currently supported)" }
                    ),
                resolve: async context =>
                {
                    var isoLanguage = context.GetArgument<string>("isoLanguage");
                    var lineage = context.GetArgument<string>("lineage");
                    return await context.TryAsyncResolve(
                         async c => await connectivity.GetTypeWordForLineage(lineage, isoLanguage));
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
                    var inputs = context.GetArgument<List<DarlVarInput>>("inputs");
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
            FieldAsync<QuestionSetType>("beginQuestionnaire","Begin a questionnaire by specifying the ruleset",
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
                                    async c => await form.BeginQuestionnaire(userId, ruleSetName, language ?? "en", questCount ?? 1));
                });
            FieldAsync<QuestionSetType>("beginDynamicQuestionnaire","Begin a dynamic questionnaire",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "selector", Description = "The model/ruleset to modify" },
                    new QueryArgument<DQTypeEnum> { Name = "dqType", DefaultValue = "rule_edit", Description = "The process" }
                ),
                resolve: async context =>
                {
                    var selector = context.GetArgument<string>("selector");
                    var dqType = context.GetArgument<DQType>("dqType");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);

                    return await context.TryAsyncResolve(
                                    async c => await form.BeginDynamicQuestionnaire(userId, selector, dqType));
                });

            FieldAsync<QuestionSetType>("continueQuestionnaire",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<QuestionSetInputType>> { Name = "responses" }
                ),
                resolve: async context =>
                {
                    var responses = context.GetArgument<QuestionSetInput>("responses");
                    return await context.TryAsyncResolve(
                                    async c => await form.ContinueQuestionnaire(responses));
                });
            FieldAsync<QuestionSetType>("backtrackQuestionnaire",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ieToken" }
                ),
                resolve: async context =>
                {
                    var ieToken = context.GetArgument<string>("ieToken");
                    return await context.TryAsyncResolve(
                                    async c => await form.BacktrackQuestionnaire(ieToken));
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
            FieldAsync<AccountStateEnum>(
               "getAccountState",
               "Gets the account status for this account. Only accessible to logged in users.",
               resolve: async context =>
               {
                   var userId = connectivity.GetCurrentUserId(context.UserContext);

                   return await context.TryAsyncResolve(
                       async c => (await connectivity.GetUserById(userId)).accountState);
               }
            ).AuthorizeWith("UserPolicy");
            FieldAsync<SubscriptionTypeEnum>(
               "getSubscriptionType",
               "Gets the subscription type for this account. Only accessible to logged in users.",
               resolve: async context =>
               {
                   var userId = connectivity.GetCurrentUserId(context.UserContext);

                   return await context.TryAsyncResolve(
                       async c => (await connectivity.GetSubscriptionType(userId)));
               }
            ).AuthorizeWith("UserPolicy");
            FieldAsync<LineageNodeAttributeResourceType>(
                "getLineageNodeAttributeResources",
                "Get the resources needed to edit or create lineageNodeAttributes",
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
            FieldAsync<DocumentType>(
                "getDocument",
                "Get a document used as a template",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the collateral" }),
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var name = context.GetArgument<string>("name");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetDocument(userId, name));
                }
            );
            FieldAsync<ListGraphType<DocumentType>>(
                "documents",
                "Get documents used as templates",
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetDocuments(userId));
                }
            );
            FieldAsync<ListGraphType<UpdateType>>(
                "updates",
                "Get details about most recent updates",
                resolve: async context =>
                {
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetUpdates());
                }
            );
            FieldAsync<BooleanGraphType>(
                "checkEmail",
                "Check if an email is valid",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email", Description = "The email to check" },
                    new QueryArgument<StringGraphType> { Name = "ipaddress", Description = "IP address of the applicant, if available" }),
                resolve: async context =>
                {
                    var email = context.GetArgument<string>("email");
                    var ipaddress = context.GetArgument<string>("ipaddress");
                    return await context.TryAsyncResolve(
                        async c => await connectivity.CheckEmail(email, ipaddress));
                }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<DaslSetType>(
                "simulate",
                "run a simulation",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "Name of the ruleset to use" },
                    new QueryArgument<NonNullGraphType<DaslSetInputType>> { Name = "dataSet", Description = "The sequence data set to use" },
                    new QueryArgument<NonNullGraphType<SampleTypeEnum>> { Name = "sampleType", Description = "whether the data is events or samples" }),
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var name = context.GetArgument<string>("name");
                    var dataSet = context.GetArgument<DaslSet>("dataSet");
                    var sampleType = context.GetArgument<SampleType>("sampleType");
                    return await context.TryAsyncResolve(
                        async c => await sim.Simulate(userId,name,dataSet,sampleType));
                }
            );

            FieldAsync<IntGraphType>(
                "contactCount",
                "Get the count of contacts",
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetContactsCount(userId));
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<IntGraphType>(
                "contactCount30Days",
                "Get the count of contacts added in the last 30 days",
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetContactsMonthCount(userId));
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<IntGraphType>(
                "contactCountDay",
                "Get the count of contacts added in the last 24 hours",
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetContactsDayCount(userId));
                }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<IntGraphType>(
                "userCount",
                "Get the count of users",
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetUserCount(userId));
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<IntGraphType>(
                "conversationCount",
                "Get the count of conversations",
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetConversationCount(userId));
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<InteractionModelType>(
                "alexaInteractionModel",
                "Get the json contents of an Alexa interaction model to set up an Alexa skill.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "Name of the ruleset to use" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "invocationName", Description = "The invocation name of the skill" }),
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var name = context.GetArgument<string>("name");
                    var invocationName = context.GetArgument<string>("invocationName");
                    return await context.TryAsyncResolve(
                        async c => await form.GetAlexaInteractionModel(userId,name,invocationName));
                }
            );
            Field<ListGraphType<StringGraphType>>(
                "tokenize",
                "Tokenize a string using the standard en tokenizer",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "text", Description = "Text to tokenize" }),
                resolve:  context =>
                {
                    var text = context.GetArgument<string>("text");
                    return LineageLibrary.SimpleTokenizer(text);
                }
            );
            FieldAsync<ListGraphType<BotTestViewType>>(
                "botTest",
                "test a bot through conversation with visibility of internal states",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName", Description = "Name of the bot model" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "conversationId", Description = "The conversationId" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "text", Description = "The user's text" },
                    new QueryArgument<NonNullGraphType<BooleanGraphType>> { Name = "reset", Description = "true if the conversation is reset" }
                ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var conversationId = context.GetArgument<string>("conversationId");
                    var text = context.GetArgument<string>("text");
                    var reset = context.GetArgument<bool>("reset");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await bot.InteractTestAsync(userId, botModelName, conversationId, text, reset));
                }
            );
            FieldAsync<ListGraphType<GraphObjectType>>(
                "getGraphObjects",
                "Get graph objects based on name and lineage",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "Name of the object" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "The parent lineage" }
                ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var name = context.GetArgument<string>("name");
                    var lineage = context.GetArgument<string>("lineage");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetGraphObjects(CompositeName(userId, graphName), name, lineage));
                }
            ).AuthorizeWith("CorpPolicy");
            FieldAsync<ListGraphType<GraphObjectType>>(
                "getGraphObjectsByLineage",
                "Get graph objects based on lineage",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "The parent lineage" }
                ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var name = context.GetArgument<string>("name");
                    var lineage = context.GetArgument<string>("lineage");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetGraphObjectsByLineage(CompositeName(userId, graphName), lineage));
                }
            ).AuthorizeWith("CorpPolicy"); 
            FieldAsync<GraphObjectType>(
                "getGraphObjectById",
                "Get a graph object based on id",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the object" }
                    
                ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var id = context.GetArgument<string>("id");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetGraphObjectById(CompositeName(userId, graphName), id));
                }
            );
            FieldAsync<GraphObjectType>(
                 "getVirtualObjectByLineage",
                 "Get a virtual graph object based on lineage",
                 arguments: new QueryArguments(
                     new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                     new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "Lineage of the object" }

                 ),
                 resolve: async context =>
                 {
                     var graphName = context.GetArgument<string>("graphName");
                     var lineage = context.GetArgument<string>("lineage");
                     var userId = connectivity.GetCurrentUserId(context.UserContext);
                     return await context.TryAsyncResolve(async c => await graph.GetVirtualObjectByLineage(CompositeName(userId, graphName), lineage));
                 }
             );
            FieldAsync<GraphObjectType>(
                 "getRecognitionObjectById",
                 "Get a recognition graph object based on id",
                 arguments: new QueryArguments(
                     new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                     new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "Id of the object" }

                 ),
                 resolve: async context =>
                 {
                     var graphName = context.GetArgument<string>("graphName");
                     var id = context.GetArgument<string>("id");
                     var userId = connectivity.GetCurrentUserId(context.UserContext);
                     return await context.TryAsyncResolve(async c => await graph.GetRecognitionObjectById(CompositeName(userId, graphName), id));
                 }
             );

            FieldAsync<GraphObjectType>(
                "getGraphObjectByExternalId",
                "Get a graph object based on an external id",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "externalId", Description = "external id of the object" }
                ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var id = context.GetArgument<string>("externalId");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetGraphObjectByExternalId(CompositeName(userId,graphName), id));
                }
            );
            FieldAsync<GraphConnectionType>(
                "getGraphConnection",
                "Get a graph connection based on start and end ids and lineage",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "startId", Description = "id of the start object" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "endId", Description = "id of the end object" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "lineage of the connection" }
                ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var startId = context.GetArgument<string>("startId");
                    var endId = context.GetArgument<string>("endId");
                    var lineage = context.GetArgument<string>("lineage");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetConnectionByIds(CompositeName(userId, graphName), startId,endId,lineage));
                }
            );
            FieldAsync<GraphConnectionType>(
                "getGraphConnectionById",
                "Get a graph connection based on its Id",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the connection" }
                ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var id = context.GetArgument<string>("id");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetConnectionById(CompositeName(userId, graphName), id));
                }
            );

            FieldAsync<KnowledgeStateType>(
                "getKnowledgeState",
                "Get a knowledge state by its Id",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "Id", Description = "The knowledge state id" },
                    new QueryArgument<BooleanGraphType> { Name = "external", Description = "ids are ExternalIds", DefaultValue  = false }
                ),
                resolve: async context =>
                {
                    var Id = context.GetArgument<string>("Id");
                    var external = context.GetArgument<bool>("external");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetKnowledgeState(userId, Id, external));
                }
            );

            FieldAsync<KnowledgeStateType>(
                "getKnowledgeStateByExternalId",
                "Get a knowledge state by its external Id",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "subjectId", Description = "The external id" },
                     new QueryArgument<BooleanGraphType>{ Name = "externalIds", Description = "true returns externalIds for GraphObjects, rather than internal", DefaultValue = false }
                ),
                resolve: async context =>
                {
                    var subjectId = context.GetArgument<string>("subjectId");
                    var externalIds = context.GetArgument<bool>("externalIds");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetKnowledgeStateByExternalId(userId, subjectId, externalIds));
                }
            ).AuthorizeWith("CorpPolicy");

            FieldAsync<ListGraphType<KnowledgeStateType>>(
                 "getKnowledgeStates",
                 "Get all the knowledge states in this account",
                 resolve: async context =>
                 {

                     var userId = connectivity.GetCurrentUserId(context.UserContext);
                     return await context.TryAsyncResolve(async c => await connectivity.GetKnowledgeStates(userId));
                 }
             ).AuthorizeWith("CorpPolicy");


            FieldAsync<BooleanGraphType>(
                "checkKey",
                "Check a license key is valid",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "key", Description = "the license key to check" }
                ),
                resolve: async context =>
                {
                    var key = context.GetArgument<string>("key");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await connectivity.CheckKey(userId, key));
                }
            );
            FieldAsync<ListGraphType<MatchResultType>>(
                "InferFromSoftMatchModel",
                "Find the nearest matches in a given SoftMatch model to the given set of texts",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "modelName", Description = "The concept match model name" },
                    new QueryArgument<NonNullGraphType<ListGraphType<StringGraphType>>> { Name = "texts", Description = "The texts to match. Maximum 50 at a time." }
                ),
                resolve: async context =>
                {
                    var treeName = context.GetArgument<string>("modelName");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var texts = context.GetArgument<List<string>>("texts");
                    return await context.TryAsyncResolve(async c => await cmp.InferFromSoftMatchModel(userId, treeName, texts));
                }
            ).AuthorizeWith("CorpPolicy");
            FieldAsync<ListGraphType<StringGraphType>>(
                "softMatchModels",
                "Get the names of the SoftMatch models in your account",                          
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await cmp.ListSoftMatchModels(userId));
                }
            );
            FieldAsync<DarlVarType>(
                "readGraph",
                "Test graph calls used in the graph store",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "action", Description = "The name of the action to perform" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "param1", Description = "The first parameter" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "param2", Description = "The second parameter" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "param3", Description = "The third parameter" },                   
                    new QueryArgument<StringGraphType> { Name = "param4", Description = "The fourth parameter" },                    
                    new QueryArgument<StringGraphType> { Name = "param5", Description = "The fifth parameter" }),
                resolve: async context =>
                {
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var action = context.GetArgument<string>("action");
                    var param1 = context.GetArgument<string>("param1");
                    var param2 = context.GetArgument<string>("param2");
                    var param3 = context.GetArgument<string>("param3");
                    var param4 = context.GetArgument<string>("param4");
                    var param5 = context.GetArgument<string>("param5");
                    var list = new List<string> { action, param1, param2, param3 };
                    if (!string.IsNullOrEmpty(param4))
                    {
                        list.Add(param4);
                        if (!string.IsNullOrEmpty(param5))
                        {
                            list.Add(param5);
                        }
                    }
                    return DarlVarExtensions.Convert(await graphStore.ReadAsync(list));
                }
            ).AuthorizeWith("CorpPolicy");

            FieldAsync<ListGraphType<InteractResponseType>>(
                "interactKnowledgeGraph",
                "Perform a chatbot interaction making use of a knowledge graph",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "kgModelName", Description = "The knowledge graph to run" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "conversationId", Description = "The unique conversation identifier" },
                    new QueryArgument<NonNullGraphType<DarlVarInputType>> { Name = "conversationData", Description = "The input from the other converser." }
                ),
                resolve: async context =>
                {
                    var kgModelName = context.GetArgument<string>("kgModelName");
                    var conversationId = context.GetArgument<string>("conversationId");
                    var conversationData = context.GetArgument<DarlVar>("conversationData");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await bot.InteractKGAsync(userId, kgModelName, conversationId, conversationData));
                });

            FieldAsync<DisplayModelType>(
                "getRealKGDisplay",
                "Get a display version of the KG",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                    new QueryArgument<StringGraphType> { Name = "lineageFilter", Description = "optional lineage filter", DefaultValue = "" }
                ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var lineageFilter = context.GetArgument<string>("lineageFilter");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetRealDisplayGraph(CompositeName(userId, graphName), lineageFilter));
                }
            );
            FieldAsync<DisplayModelType>(
                "getVirtualKGDisplay",
                "Get a display version of the virtual part of the KG",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" }
                ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetVirtualDisplayGraph(CompositeName(userId, graphName)));
                }
            );
            FieldAsync<DisplayModelType>(
                "getRecognitionKGDisplay",
                "Get a display version of a recognition tree from the KG",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" }
                ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var root = context.GetArgument<string>("root");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetRecognitionDisplayGraph(CompositeName(userId, graphName)));
                }
            );

            //                Lint Ruleset
            FieldAsync<ListGraphType<DarlLintErrorType>>("lintDarlMeta", "Read code in DARL.Meta and return any syntax errors",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "darl" }),
                resolve: async context =>
                {
                    var darl = context.GetArgument<string>("darl");
                    return await context.TryAsyncResolve( async c => await connectivity.LintDarlMeta(darl));
                });

            FieldAsync<ListGraphType<LineageRecordType>>("getLineagesInKG", "Get existing lineages used for this element type in this KG. ",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the elements" },
                    new QueryArgument<NonNullGraphType<GraphTypeEnum>> { Name = "graphType", Description = "The type of element to find lineages for" }
                    ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var userId = connectivity.GetCurrentUserId(context.UserContext);
                    var graphType = context.GetArgument<GraphElementType>("graphType");
                    return await context.TryAsyncResolve(async c => await graph.GetLineagesInKG(CompositeName(userId,graphName),graphType));
                });
            Field<BooleanGraphType>("isValidLineage", "Check if this is a valid lineage. ",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "the word to check" }
                    ),
                resolve: context =>
                {
                    var lineage = context.GetArgument<string>("lineage");
                    return LineageLibrary.CheckLineage(lineage);
                });
            FieldAsync<StringGraphType>("getSuggestedRuleset", "Get a suggested initial ruleset for an attribute. ",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "Lineage of the attribute" }
                ),
            resolve: async context =>
            {
                var lineage = context.GetArgument<string>("lineage");
                return await context.TryAsyncResolve(async c => await graph.GetSuggestedRuleSet(lineage));
            });
        }

        private string CompositeName(string userId, string graphName)
        {
            return $"{userId}_{graphName}";
        }
    }
}