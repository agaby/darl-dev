using Darl.GraphQL.Models.Services;
using GraphQL.Types;
using System;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlQuery : ObjectGraphType<object>
    {

        public DarlQuery(IBotModelService botmodels, IMLModelService mlmodels, IRuleSetService rulesets)
        {
            Name = "Query";
            FieldAsync<ListGraphType<RuleSetType>>(
                "rulesets",
                resolve: async context => {
                    return await context.TryAsyncResolve(
                        async c => await rulesets.GetRuleSetsAsync());
                }
            );

            FieldAsync<ListGraphType<RuleSetType>>(
               "mlmodels",
               resolve: async context => {
                   return await context.TryAsyncResolve(
                       async c => await mlmodels.GetMlModelsAsync());
               }
            );

            FieldAsync<ListGraphType<RuleSetType>>(
              "botmodels",
              resolve: async context => {
                  return await context.TryAsyncResolve(
                      async c => await botmodels.GetBotModelsAsync());
              }
            );

            FieldAsync<RuleSetType>(
                "rulesetByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context => {
                    return await context.TryAsyncResolve(
                        async c => await rulesets.GetRuleSetAsync(c.GetArgument<String>("name"))
                    );
                }
            );

            FieldAsync<RuleSetType>(
                "mlmodelsByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context => {
                    return await context.TryAsyncResolve(
                        async c => await mlmodels.GetMlModelAsync(c.GetArgument<String>("name"))
                    );
                }
            );

            FieldAsync<RuleSetType>(
                "botmodelsByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context => {
                    return await context.TryAsyncResolve(
                        async c => await botmodels.GetBotModelAsync(c.GetArgument<String>("name"))
                    );
                }
            );
        }
    }
}