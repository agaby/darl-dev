using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlQuery : ObjectGraphType<object>
    {
        public DarlQuery(IConnectivity connectivity)
        {
            Name = "Query";
            Description = "View the contents of your account.";
            FieldAsync<ListGraphType<RuleSetType>>(
                "rulesets",
                resolve: async context =>
                {
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetRuleSetsAsync());
                }
            );

            FieldAsync<ListGraphType<MLModelType>>(
               "mlmodels",
               resolve: async context =>
               {
                   return await context.TryAsyncResolve(
                       async c => await connectivity.GetMlModelsAsync());
               }
            );

            FieldAsync<ListGraphType<BotModelType>>(
              "botmodels",
              resolve: async context =>
              {
                  return await context.TryAsyncResolve(
                      async c => await connectivity.GetBotModelsAsync());
              }
            );

            FieldAsync<ListGraphType<ContactType>>(
              "contacts",
                  resolve: async context =>
                  {
                      return await context.TryAsyncResolve(
                                  async c => await connectivity.GetContacts());
                  }
            );

            FieldAsync<ListGraphType<ContactType>>("contactsByLastName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lastName" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetContactsByLastName(c.GetArgument<String>("lastName"))); });

            FieldAsync<ContactType>("contactByEmail",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetContactByEmail(c.GetArgument<String>("email"))); });

            FieldAsync<ListGraphType<DefaultType>>("defaults",
                resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetDefaults()); });

            FieldAsync<StringGraphType>("defaultValue",
            arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
            resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetDefaultValue(c.GetArgument<String>("name"))); });

            FieldAsync<RuleSetType>(
                "rulesetByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context =>
                {
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetRuleSet(c.GetArgument<String>("name"))
                    );
                }
            );

            FieldAsync<MLModelType>(
                "mlmodelByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context =>
                {
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetMlModel(c.GetArgument<String>("name"))
                    );
                }
            );

            FieldAsync<BotModelType>(
                "botmodelByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context =>
                {
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetBotModel(c.GetArgument<String>("name"))
                    );
                }
            );

            FieldAsync<ListGraphType<BotConnectionType>>(
                "botConnectionsByModel",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context =>
                {
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetBotConnectivity(c.GetArgument<String>("name"))
                    );
                }
            );

            FieldAsync<ListGraphType<UserUsageType>>(
                "botUsages",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "appId" }
                    ),
                resolve: async context =>
                {
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetBotUsage(c.GetArgument<String>("botModelName"), c.GetArgument<String>("appId"))
                    );
                }
            );

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
                    return await context.TryAsyncResolve(
                         async c => await connectivity.GetChildrenLineageNodes(botModelName, path, isRoot));
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

            FieldAsync<ListGraphType<LineageNodeAttributeType>>("getAttribute",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "phrase" }
                    ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var phrase = context.GetArgument<string>("phrase");
                    return await context.TryAsyncResolve(
                         async c => await connectivity.GetAttribute(botModelName, phrase));
                });

            FieldAsync<ListGraphType<LineageNodeAttributeType>>("getAttributeFromPath",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "botModelName" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "path" }
                    ),
                resolve: async context =>
                {
                    var botModelName = context.GetArgument<string>("botModelName");
                    var path = context.GetArgument<string>("path");
                    return await context.TryAsyncResolve(
                         async c => await connectivity.GetAttributeFromPath(botModelName, path));
                });
            FieldAsync<ListGraphType<DarlUserType>>("usersByEmail",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetUsersByEmail(c.GetArgument<String>("email"))); });
            FieldAsync<DarlUserType>("userById",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "userId" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetUserById(c.GetArgument<String>("userId"))); });
            //                GetExampleInputs
            FieldAsync<ListGraphType<DarlVarType>>("getExampleInputs",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName" }
                ),
                resolve: async context =>
                {
                    var ruleSetName = context.GetArgument<string>("ruleSetName");
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.GetExampleInputs(ruleSetName));
                });
            //  Whole ruleset inference
            FieldAsync<ListGraphType<DarlVarType>>("inferFromRuleSet",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "ruleSetName" },
                new QueryArgument<NonNullGraphType<ListGraphType<DarlVarInputType>>> { Name = "inputs" }
                ),
                resolve: async context =>
                {
                    var ruleSetName = context.GetArgument<string>("ruleSetName");
                    var inputs = context.GetArgument<List<DarlVar>>("inputs");
                    return await context.TryAsyncResolve(
                                    async c => await connectivity.InferFromRuleSetDarlVar(ruleSetName, inputs));
                });
            //                Lint Ruleset
            FieldAsync<ListGraphType<DarlLintErrorType>>("lintDarl",
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

                    return await context.TryAsyncResolve(
                                    async c => await connectivity.BeginQuestionnaire(ruleSetName, language ?? "en", questCount ?? 1));
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
        }
    }
}