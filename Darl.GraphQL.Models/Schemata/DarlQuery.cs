using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Services;
using GraphQL.Types;
using System;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlQuery : ObjectGraphType<object>
    {

        public DarlQuery(IBotModelService botmodels, IMLModelService mlmodels, IRuleSetService rulesets, IConnectivity connectivity)
        {
            Name = "Query";
            FieldAsync<ListGraphType<RuleSetType>>(
                "rulesets",
                resolve: async context => {
                    return await context.TryAsyncResolve(
                        async c => await rulesets.GetRuleSetsAsync());
                }
            );

            FieldAsync<ListGraphType<MLModelType>>(
               "mlmodels",
               resolve: async context => {
                   return await context.TryAsyncResolve(
                       async c => await mlmodels.GetMLModelsAsync());
               }
            );

            FieldAsync<ListGraphType<BotModelType>>(
              "botmodels",
              resolve: async context => {
                  return await context.TryAsyncResolve(
                      async c => await botmodels.GetBotModelsAsync());
              }
            );

            FieldAsync<ServiceConnectivityType>(
              "connectivity",
                  resolve: async context => {
                  return await context.TryAsyncResolve(
                  async c => await connectivity.GetServiceConnectivity());
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

            FieldAsync<ListGraphType<ContactType>>("contactsByEmail",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetContactsByLastName(c.GetArgument<String>("email"))); });

            FieldAsync<ListGraphType<DefaultType>>("defaults",
                resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetDefaults()); });

            FieldAsync<ListGraphType<StringGraphType>>("defaultValue",
            arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
            resolve: async context => { return await context.TryAsyncResolve(async c => await connectivity.GetDefaultValue(c.GetArgument<String>("name"))); });


            FieldAsync<RuleSetType>(
                "rulesetByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context => {
                    return await context.TryAsyncResolve(
                        async c => await rulesets.GetRuleSet(c.GetArgument<String>("name"))
                    );
                }
            );

            FieldAsync<RuleSetType>(
                "mlmodelByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context => {
                    return await context.TryAsyncResolve(
                        async c => await mlmodels.GetMLModel(c.GetArgument<String>("name"))
                    );
                }
            );

            FieldAsync<RuleSetType>(
                "botmodelByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context => {
                    return await context.TryAsyncResolve(
                        async c => await botmodels.GetBotModel(c.GetArgument<String>("name"))
                    );
                }
            );
        }
    }
}