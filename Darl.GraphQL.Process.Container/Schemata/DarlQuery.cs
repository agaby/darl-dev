/// </summary>

﻿using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Schemata;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase;
using DarlCommon;
using GraphQL;
using GraphQL.Types;
using System;
using System.Collections.Generic;

namespace Darl.GraphQL.Container.Models.Schemata
{
    public class DarlQuery : ObjectGraphType<object>
    {
        public DarlQuery(IConnectivity connectivity, IBotProcessing bot, IGraphProcessing graph, ISoftMatchProcessing cmp, IKGTranslation trans)
        {
            Name = "Query";
            Description = "View the contents of your account.";

            FieldAsync<ListGraphType<KGraphType>>(
              "kgraphs",
              "The set of Knowledge Graphs for this account.",
              resolve: async context =>
              {
                  var userId = trans.GetCurrentUserId(context.UserContext);
                  return await connectivity.GetKGraphsAsync(userId);
              }
            );

            FieldAsync<KGraphType>(
                "kGraphByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await connectivity.GetKGModel(userId, context.GetArgument<String>("name"));
                }
            );
            FieldAsync<ListGraphType<LineageRecordType>>("getLineagesForWord",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "word", Description = "The word to look up" },
                    new QueryArgument<StringGraphType> { Name = "isoLanguage", DefaultValue = "en", Description = "language for lookup (Only en currently supported)" }
                    ),
                resolve: async context =>
                {
                    var isoLanguage = context.GetArgument<string>("isoLanguage");
                    var word = context.GetArgument<string>("word");
                    return await trans.GetLineagesForWord(word, isoLanguage);
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
                    return await trans.GetTypeWordForLineage(lineage, isoLanguage);
                });

            Field<ListGraphType<StringGraphType>>(
                "tokenize",
                "Tokenize a string using the standard en tokenizer",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "text", Description = "Text to tokenize" }),
                resolve: context =>
               {
                   var text = context.GetArgument<string>("text");
                   return LineageLibrary.SimpleTokenizer(text);
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.GetGraphObjects(CompositeName(userId, graphName), name, lineage);
                }
            );
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.GetGraphObjectsByLineage(CompositeName(userId, graphName), lineage);
                }
            );
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.GetGraphObjectById(CompositeName(userId, graphName), id);
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
                     var userId = trans.GetCurrentUserId(context.UserContext);
                     return await graph.GetVirtualObjectByLineage(CompositeName(userId, graphName), lineage);
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
                     var userId = trans.GetCurrentUserId(context.UserContext);
                     return await graph.GetRecognitionObjectById(CompositeName(userId, graphName), id);
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.GetGraphObjectByExternalId(CompositeName(userId, graphName), id);
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.GetConnectionByIds(CompositeName(userId, graphName), startId, endId, lineage);
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.GetConnectionById(CompositeName(userId, graphName), id);
                }
            );

            FieldAsync<KnowledgeStateType>(
                "getKnowledgeState",
                "Get a knowledge state by its Id",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "Id", Description = "The knowledge state id" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The name of the associated Knowledge Graph." },
                    new QueryArgument<BooleanGraphType> { Name = "external", Description = "ids are ExternalIds", DefaultValue = false }
                ),
                resolve: async context =>
                {
                    var Id = context.GetArgument<string>("Id");
                    var name = context.GetArgument<string>("graphName");
                    var external = context.GetArgument<bool>("external");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.GetKnowledgeState(userId, Id, name, external);
                }
            );

            FieldAsync<KnowledgeStateType>(
                "getKnowledgeStateByExternalId",
                "Get a knowledge state by its external Id",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "subjectId", Description = "The external id" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The name of the associated Knowledge Graph." },
                    new QueryArgument<BooleanGraphType> { Name = "externalIds", Description = "true returns externalIds for GraphObjects, rather than internal", DefaultValue = false }
                ),
                resolve: async context =>
                {
                    var subjectId = context.GetArgument<string>("subjectId");
                    var name = context.GetArgument<string>("graphName");
                    var externalIds = context.GetArgument<bool>("externalIds");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.GetKnowledgeStateByExternalId(userId, subjectId, name, externalIds);
                }
            );

            FieldAsync<ListGraphType<KnowledgeStateType>>(
                 "getKnowledgeStates",
                 "Get all the knowledge states in this account", arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The name of the associated Knowledge Graph." }
                 ),
                 resolve: async context =>
                 {

                     var userId = trans.GetCurrentUserId(context.UserContext);
                     var name = context.GetArgument<string>("graphName");
                     return await connectivity.GetKnowledgeStates(userId, name);
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    var texts = context.GetArgument<List<string>>("texts");
                    return await cmp.InferFromSoftMatchModel(userId, treeName, texts);
                }
            );
            FieldAsync<ListGraphType<StringGraphType>>(
                "softMatchModels",
                "Get the names of the SoftMatch models in your account",
                resolve: async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await cmp.ListSoftMatchModels(userId);
                }
            );

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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await bot.InteractKGAsync(userId, kgModelName, conversationId, conversationData);
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.GetRealDisplayGraph(CompositeName(userId, graphName), lineageFilter);
                }
            );

            FieldAsync<DisplayModelType>(
                "getRealKGDisplayWithState",
                "Get a display version of the KG with states set",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "subjectId", Description = "The subject Id of the Knowledge State", DefaultValue = "" }
                ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var subjectId = context.GetArgument<string>("subjectId");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.GetRealDisplayGraphWithState(userId, graphName, subjectId);
                }
            );

            FieldAsync<VRDisplayModelType>(
                "getRealVRKGDisplay",
                "Get a display version of the KG for VR",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                    new QueryArgument<StringGraphType> { Name = "lineageFilter", Description = "optional lineage filter", DefaultValue = "" },
                    new QueryArgument<StringGraphType> { Name = "subjectId", Description = "optional identifier for a Knowledge State to use." }
                ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var lineageFilter = context.GetArgument<string>("lineageFilter");
                    var subjectId = context.GetArgument<string>("subjectId");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.GetRealVRDisplayGraph(userId, graphName, lineageFilter, subjectId);
                }
            );
            FieldAsync<StringGraphType>(
                "getGraphObjectToString",
                "Get a textual overview of an object",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id of the object" }
                ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var id = context.GetArgument<string>("id");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.GetGraphObjectToString(CompositeName(userId, graphName), id);
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.GetVirtualDisplayGraph(CompositeName(userId, graphName));
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.GetRecognitionDisplayGraph(CompositeName(userId, graphName));
                }
            );
            FieldAsync<GraphAttributeType>(
                "getGraphAttribute",
                "Drill down to an individual attribute within a KG",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "id or externalId of the associated object" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "lineage of the attribute to find" },
                    new QueryArgument<StringGraphType> { Name = "ksId", Description = "Knowledge State id if required" }
                ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var id = context.GetArgument<string>("id");
                    var lineage = context.GetArgument<string>("lineage");
                    var ksId = context.GetArgument<string>("ksId");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.GetGraphAttribute(userId, graphName, id, lineage, ksId);
                }
            );

            //                Lint Ruleset
            FieldAsync<ListGraphType<DarlLintErrorType>>("lintDarlMeta", "Read code in DARL.Meta and return any syntax errors",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "darl" }),
                resolve: async context =>
                {
                    var darl = context.GetArgument<string>("darl");
                    return await trans.LintDarlMeta(darl);
                });

            FieldAsync<ListGraphType<LineageRecordType>>("getLineagesInKG", "Get existing lineages used for this element type in this KG. ",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the elements" },
                    new QueryArgument<NonNullGraphType<GraphTypeEnum>> { Name = "graphType", Description = "The type of element to find lineages for" }
                    ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    var graphType = context.GetArgument<GraphElementType>("graphType");
                    return await graph.GetLineagesInKG(CompositeName(userId, graphName), graphType);
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
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "objectId", Description = "Id of the object containing the new ruleset." },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "Lineage of the attribute" }
                ),
            resolve: async context =>
            {
                var lineage = context.GetArgument<string>("lineage");
                var objectId = context.GetArgument<string>("objectId");
                var graphName = context.GetArgument<string>("graphName");
                var userId = trans.GetCurrentUserId(context.UserContext);
                return await trans.GetSuggestedRuleSet(userId, graphName, objectId, lineage);
            });

            FieldAsync<ListGraphType<KnowledgeStateType>>("discover", "Discover possibilities in a graph",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The Knowledge graph used" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "subjectId", Description = "the subject Id of the start point" }
                ),
            resolve: async context =>
            {
                var graphName = context.GetArgument<string>("graphName");
                var subjectId = context.GetArgument<string>("subjectId");
                var userId = trans.GetCurrentUserId(context.UserContext);
                return await bot.Discover(userId, graphName, subjectId);
            });

            FieldAsync<StringGraphType>("exportNoda", "Export a graph in Noda format",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The Knowledge graph to export" }
                ),
            resolve: async context =>
            {
                var graphName = context.GetArgument<string>("graphName");
                var userId = trans.GetCurrentUserId(context.UserContext);
                return await trans.ExportNoda(userId, graphName);
            });

            FieldAsync<StringGraphType>("getExportGraphUrl", "get a link to Export a graph in native format",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The Knowledge graph to export" }
                ),
            resolve: async context =>
            {
                var graphName = context.GetArgument<string>("graphName");
                var userId = trans.GetCurrentUserId(context.UserContext);
                return await graph.CreateTimedAccessUrl(userId, graphName);
            });
            FieldAsync<ListGraphType<GraphAttributeType>>("conceptCloud", "Get the data for the concept cloud",
                arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The Knowledge graph to view" },
                new QueryArgument<StringGraphType> { Name = "address", Description = "the address to return. Empty/null = top" }
                ),
            resolve: async context =>
            {
                var graphName = context.GetArgument<string>("graphName");
                var userId = trans.GetCurrentUserId(context.UserContext);
                var address = context.GetArgument<string>("address");
                return await trans.GetConceptCloudData(userId, graphName, address);
            });
        }

        private string CompositeName(string userId, string graphName)
        {
            return $"{userId}_{graphName}";
        }
    }
}