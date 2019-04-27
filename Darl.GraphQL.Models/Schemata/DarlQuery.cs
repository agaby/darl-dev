using Darl.GraphQL.Models.Connectivity;
using GraphQL.Types;
using System;

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
                resolve: async context => {
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetRuleSetsAsync());
                }
            );

            FieldAsync<ListGraphType<MLModelType>>(
               "mlmodels",
               resolve: async context => {
                   return await context.TryAsyncResolve(
                       async c => await connectivity.GetMlModelsAsync());
               }
            );

            FieldAsync<ListGraphType<BotModelType>>(
              "botmodels",
              resolve: async context => {
                  return await context.TryAsyncResolve(
                      async c => await connectivity.GetBotModelsAsync());
              }
            );

            FieldAsync<ListGraphType<ContactType>>(
              "contacts",
                  resolve: async context => {
                      return await context.TryAsyncResolve(
                                  async c => await connectivity.GetContacts());
                        }
                    );

            FieldAsync<ListGraphType<ContactType>>("contactsByLastName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lastName" }),
                resolve: async context => {return await context.TryAsyncResolve(async c => await connectivity.GetContactsByLastName(c.GetArgument<String>("lastName")));});

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
                resolve: async context => {
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetRuleSet(c.GetArgument<String>("name"))
                    );
                }
            );

            FieldAsync<MLModelType>(
                "mlmodelByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context => {
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetMlModel(c.GetArgument<String>("name"))
                    );
                }
            );

            FieldAsync<BotModelType>(
                "botmodelByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context => {
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetBotModel(c.GetArgument<String>("name"))
                    );
                }
            );

            FieldAsync<ListGraphType<BotConnectionType>>(
                "botConnectionsByModel",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context => {
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
                resolve: async context => {
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetBotUsage(c.GetArgument<String>("botModelName"), c.GetArgument<String>("appId"))
                    );
                }
            );

            FieldAsync< ListGraphType<LineageNodeDefinitionType>>("getChildrenLineageNodes",
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
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "isoLanguage" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "word" }
                    ),
                resolve: async context =>
                {
                    var isoLanguage = context.GetArgument<string>("isoLanguage");
                    var word = context.GetArgument<string>("word");
                    return await context.TryAsyncResolve(
                         async c => await connectivity.GetLineagesForWord(isoLanguage, word));
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
                         async c => await connectivity.GetAttribute(botModelName,phrase));
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
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetUserById(c.GetArgument<String>("id"))); });

        }
    }
}