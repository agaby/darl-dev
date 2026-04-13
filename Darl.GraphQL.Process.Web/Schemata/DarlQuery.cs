/// <summary>
/// </summary>

﻿using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models.Noda;
using Darl.GraphQL.Models.Schemata;
using Darl.GraphQL.Models.Schemata.Noda;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase;
using DarlCommon;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Darl.GraphQL.Web.Models.Schemata
{
    public class DarlQuery : ObjectGraphType<object>
    {
        public DarlQuery(IConnectivity connectivity, IBotProcessing bot, IGraphProcessing graph, ISoftMatchProcessing cmp, IKGTranslation trans, IConfiguration config)
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

            FieldAsync<ListGraphType<ContactType>>(
              "contacts",
               "The set of contacts.",
                  resolve: async context =>
                  {
                      return await trans.GetContacts();
                  }
            ).AuthorizeWithPolicy("AdminPolicy");
            FieldAsync<ListGraphType<ContactType>>(
              "recentContacts",
                  resolve: async context =>
                  {
                      return await trans.GetRecentContacts();
                  }
            ).AuthorizeWithPolicy("AdminPolicy");
            FieldAsync<ListGraphType<DarlUserType>>(
              "recentUsers",
                  resolve: async context =>
                  {
                      return await trans.GetRecentUsers();
                  }
            ).AuthorizeWithPolicy("AdminPolicy");
            FieldAsync<ListGraphType<DarlUserType>>(
              "users",
                  resolve: async context =>
                  {
                      return await trans.GetUsers();
                  }
            ).AuthorizeWithPolicy("AdminPolicy");

            FieldAsync<ListGraphType<ContactType>>("contactsByLastName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lastName" }),
                resolve: async context => { return await trans.GetContactsByLastName(context.GetArgument<String>("lastName")); })
                .AuthorizeWithPolicy("AdminPolicy");

            FieldAsync<ContactType>("contactByEmail",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" }),
                resolve: async context => { return await trans.GetContactByEmail(context.GetArgument<String>("email")); })
                .AuthorizeWithPolicy("AdminPolicy");

            FieldAsync<ListGraphType<DefaultType>>("defaults",
                resolve: async context => { return await trans.GetDefaults(); })
                .AuthorizeWithPolicy("AdminPolicy");

            FieldAsync<StringGraphType>("defaultValue",
            arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
            resolve: async context => { return await trans.GetDefaultValue(context.GetArgument<String>("name")); })
                .AuthorizeWithPolicy("AdminPolicy");

            FieldAsync<KGraphType>(
                "kGraphByName",
                arguments:
                new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" },
                    new QueryArgument<BooleanGraphType> { Name = "asSystem", Description = "Write to system account", DefaultValue = false }),
                resolve: async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    var name = context.GetArgument<String>("name");
                    var asSystem = context.GetArgument<bool>("asSystem");
                    if (asSystem)
                    {
                        var user = trans.GetUserById(userId).Result;
                        if (user == null || user.accountState != GraphQL.Models.Models.DarlUser.AccountState.admin)
                        {
                            throw new ExecutionError($"asSystem == true only permitted to Administrators.");
                        }
                        return await connectivity.GetKGModel(config["AppSettings:boaiuserid"], name);
                    }
                    return await connectivity.GetKGModel(userId, name);
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

            FieldAsync<ListGraphType<DarlUserType>>("usersByEmail",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" }),
                resolve: async context => { return await trans.GetUsersByEmail(context.GetArgument<String>("email")); })
                .AuthorizeWithPolicy("AdminPolicy");
            FieldAsync<DarlUserType>("userById",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "userId" }),
                resolve: async context => { return await trans.GetUserById(context.GetArgument<String>("userId")); })
                .AuthorizeWithPolicy("AdminPolicy");
            FieldAsync<DarlUserType>("userByStripeId",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "stripeCustomerId" }),
                resolve: async context => { return await trans.GetUserByStripeId(context.GetArgument<String>("stripeCustomerId")); })
                .AuthorizeWithPolicy("AdminPolicy");

            FieldAsync<StringGraphType>(
               "getApiKey",
               "Gets the API key for this account. Only accessible to logged in users.",
               resolve: async context =>
               {
                   var userId = trans.GetCurrentUserId(context.UserContext);

                   return (await trans.GetUserById(userId)).APIKey;
               }
            ).AuthorizeWithPolicy("UserPolicy");
            FieldAsync<AccountStateEnum>(
               "getAccountState",
               "Gets the account status for this account. Only accessible to logged in users.",
               resolve: async context =>
               {
                   var userId = trans.GetCurrentUserId(context.UserContext);

                   return (await trans.GetUserById(userId)).accountState;
               }
            ).AuthorizeWithPolicy("UserPolicy");

            FieldAsync<StringGraphType>(
                "getCollateral",
                "Get text used in responses",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the collateral" }),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    return await trans.GetCollateral(name);
                }
            ).AuthorizeWithPolicy("AdminPolicy");
            FieldAsync<ListGraphType<CollateralType>>(
                "collateral",
                "Get texts used in responses",
                resolve: async context =>
                {
                    return await trans.GetCollaterals();
                }
            ).AuthorizeWithPolicy("AdminPolicy");

            FieldAsync<ListGraphType<PushSubType>>(
                "pushSubs",
                "Get push subscriptions",
                resolve: async context =>
                {
                    return await trans.GetPushSubs();
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
                    return await trans.GetLastUpdate(from, to);
                }
            );
            FieldAsync<ListGraphType<UpdateType>>(
                "updates",
                "Get details about most recent updates",
                resolve: async context =>
                {
                    return await trans.Updates();
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
                    return await trans.CheckEmail(email, ipaddress);
                }
            ).AuthorizeWithPolicy("AdminPolicy");


            FieldAsync<IntGraphType>(
                "contactCount",
                "Get the count of contacts",
                resolve: async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await trans.GetContactsCount(userId);
                }
            ).AuthorizeWithPolicy("AdminPolicy");
            FieldAsync<IntGraphType>(
                "contactCount30Days",
                "Get the count of contacts added in the last 30 days",
                resolve: async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await trans.GetContactsMonthCount(userId);
                }
            ).AuthorizeWithPolicy("AdminPolicy");
            FieldAsync<IntGraphType>(
                "contactCountDay",
                "Get the count of contacts added in the last 24 hours",
                resolve: async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await trans.GetContactsDayCount(userId);
                }
            ).AuthorizeWithPolicy("AdminPolicy");

            FieldAsync<IntGraphType>(
                "userCount",
                "Get the count of users",
                resolve: async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await trans.GetUserCount(userId);
                }
            ).AuthorizeWithPolicy("AdminPolicy");


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
            ).AuthorizeWithPolicy("UserPolicy");
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
            ).AuthorizeWithPolicy("UserPolicy");
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
            ).AuthorizeWithPolicy("UserPolicy");

            FieldAsync<ListGraphType<KnowledgeStateType>>(
                 "getKnowledgeStates",
                 "Get all the knowledge states in this graph", arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The name of the associated Knowledge Graph." }
                 ),
                 resolve: async context =>
                 {

                     var userId = trans.GetCurrentUserId(context.UserContext);
                     var name = context.GetArgument<string>("graphName");
                     return await connectivity.GetKnowledgeStates(userId, name);
                 }
             ).AuthorizeWithPolicy("UserPolicy");
            FieldAsync<ListGraphType<KnowledgeStateType>>(
                 "getKnowledgeStatesByType",
                 "Get all the knowledge states in this graph descended from a particular graph object.", arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The name of the associated Knowledge Graph." },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "typeObjectId", Description = "The id of the object these are descended from." },
                    new QueryArgument<BooleanGraphType> { Name = "asSystem", Description = "Write to system account", DefaultValue = false }
                 ),
                 resolve: async context =>
                 {

                     var userId = trans.GetCurrentUserId(context.UserContext);
                     var name = context.GetArgument<string>("graphName");
                     var typeId = context.GetArgument<string>("typeObjectId");
                     var asSystem = context.GetArgument<bool>("asSystem");
                     if (asSystem)
                     {
                         var user = trans.GetUserById(userId).Result;
                         if (user == null || user.accountState != GraphQL.Models.Models.DarlUser.AccountState.admin)
                         {
                             throw new ExecutionError($"asSystem == true only permitted to Administrators.");
                         }
                         return await connectivity.GetKnowledgeStatesByType(config["AppSettings:boaiuserid"], typeId, name);
                     }
                     return await connectivity.GetKnowledgeStatesByType(userId, typeId, name);
                 }
             ).AuthorizeWithPolicy("UserPolicy");
            FieldAsync<ListGraphType<KnowledgeStateType>>(
                 "getKnowledgeStatesByTypeAndAttribute",
                 "Get all the knowledge states in this graph descended from a particular graph object containing an attribute with a particular value.", arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The name of the associated Knowledge Graph." },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "typeObjectId", Description = "The id of the object these are descended from." },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "attLineage", Description = "The lineage of the attribute that must be contained." },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "attValue", Description = "The value required to be present" },
                    new QueryArgument<BooleanGraphType> { Name = "asSystem", Description = "Write to system account", DefaultValue = false }
                 ),
                 resolve: async context =>
                 {

                     var userId = trans.GetCurrentUserId(context.UserContext);
                     var name = context.GetArgument<string>("graphName");
                     var typeId = context.GetArgument<string>("typeObjectId");
                     var attLineage = context.GetArgument<string>("attLineage");
                     var attValue = context.GetArgument<string>("attValue");
                     var asSystem = context.GetArgument<bool>("asSystem");
                     if (asSystem)
                     {
                         var user = trans.GetUserById(userId).Result;
                         if (user == null || user.accountState != GraphQL.Models.Models.DarlUser.AccountState.admin)
                         {
                             throw new ExecutionError($"asSystem == true only permitted to Administrators.");
                         }
                         return await connectivity.GetKnowledgeStatesByTypeAndAttribute(config["AppSettings:boaiuserid"], typeId, name, attLineage, attValue);
                     }
                     return await connectivity.GetKnowledgeStatesByTypeAndAttribute(userId, typeId, name, attLineage, attValue);
                 }
             ).AuthorizeWithPolicy("UserPolicy");
            FieldAsync<BooleanGraphType>(
                "checkKey",
                "Check a license key is valid",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "key", Description = "the license key to check" }
                ),
                resolve: async context =>
                {
                    var key = context.GetArgument<string>("key");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await trans.CheckKey(userId, key);
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
            ).AuthorizeWithPolicy("UserPolicy");
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

            FieldAsync<VRDisplayModelType>(
                "getRealVRKGDisplay",
                "Get a display version of the KG for VR",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                    new QueryArgument<StringGraphType> { Name = "lineageFilter", Description = "optional lineage filter", DefaultValue = "" },
                    new QueryArgument<StringGraphType> { Name = "subjectId", Description = "the optional subject Id of the KS used" }
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
            FieldAsync<StringGraphType>("registerForMarketing", "Receive marketing communications from DARL ",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "name of the contact" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email", Description = "email of the contact" },
                  new QueryArgument<StringGraphType> { Name = "ipAddress", Description = "The user's IP address" },
                 new QueryArgument<StringGraphType> { Name = "longitude", Description = "The user's longitude" },
                 new QueryArgument<StringGraphType> { Name = "latitude", Description = "The user's latitude" }
            ),
            resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var email = context.GetArgument<string>("email");
                var ipAddress = context.GetArgument<string>("ipAddress");
                var longitude = context.GetArgument<string>("longitude");
                var latitude = context.GetArgument<string>("latitude");
                return await trans.RegisterForMarketing(name, email, ipAddress, longitude, latitude);
            });
            FieldAsync<StringGraphType>("registerPushSubscription", "Register a new push subscription", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "pushEndpoint", Description = "The endpoint for the browser" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "pushP256DH", Description = "The push key code" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "pushAuth", Description = "The push key Auth" },
                 new QueryArgument<StringGraphType> { Name = "ipAddress", Description = "The user's IP address" },
                 new QueryArgument<StringGraphType> { Name = "longitude", Description = "The user's longitude" },
                 new QueryArgument<StringGraphType> { Name = "latitude", Description = "The user's latitude" }
                ),
                resolve: async context =>
                {
                    var pushEndpoint = context.GetArgument<string>("pushEndpoint");
                    var pushP256DH = context.GetArgument<string>("pushP256DH");
                    var pushAuth = context.GetArgument<string>("pushAuth");
                    var ipAddress = context.GetArgument<string>("ipAddress");
                    var longitude = context.GetArgument<string>("longitude");
                    var latitude = context.GetArgument<string>("latitude");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await trans.CreatePushSubscription(userId, pushEndpoint, pushP256DH, pushAuth, ipAddress, longitude, latitude);
                }
            );
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

            FieldAsync<StringGraphType>("nodaView", "Obtain the data to display a graph within Noda",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The Knowledge graph to display" },
                new QueryArgument<FloatGraphType> { Name = "boundingDiagonal", Description = "The diagonal size in display units of the laid-out network", DefaultValue = 3.0 },
                new QueryArgument<NodaPositionInputType> { Name = "offset", Description = "The offset of the centre of the network relative to the window" }
                ),
            resolve: async context =>
            {
                var graphName = context.GetArgument<string>("graphName");
                var boundingDiagonal = context.GetArgument<double>("boundingDiagonal");
                var offset = context.GetArgument<NodaPosition>("offset");
                var userId = trans.GetCurrentUserId(context.UserContext);
                return await trans.NodaView(userId, graphName, offset, boundingDiagonal);
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
            }).AuthorizeWithPolicy("UserPolicy");
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

            FieldAsync<BooleanGraphType>("tempKGExists", "See if a temp KG exists",
                arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The Knowledge graph to check for" }
                ),
            resolve: async context =>
            {
                var graphName = context.GetArgument<string>("graphName");
                var userId = trans.GetCurrentUserId(context.UserContext);
                return await trans.TempKGExists(userId, graphName);
            });
            FieldAsync<ListGraphType<ByteGraphType>>("kGContents", "Get a KG's contents binary encoded",
                arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The Knowledge graph to download" }
                ),
            resolve: async context =>
            {
                var graphName = context.GetArgument<string>("graphName");
                var userId = trans.GetCurrentUserId(context.UserContext);
                return (await trans.KGContents(userId, graphName)).ToList();
            });

            FieldAsync<KnowledgeStateType>(
                "getInteractKnowledgeState",
                "Get a knowledge state created during an interaction by its conversationId",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "Id", Description = "The conversation id" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The Knowledge graph involved" },
                    new QueryArgument<BooleanGraphType> { Name = "external", Description = "ids are ExternalIds", DefaultValue = false }
                ),
                resolve: async context =>
                {
                    var Id = context.GetArgument<string>("Id");
                    var external = context.GetArgument<bool>("external");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    var graphName = context.GetArgument<string>("graphName");
                    return await bot.GetInteractKnowledgeState(Id, userId, graphName, external);
                }
            );
        }

        private string CompositeName(string userId, string graphName)
        {
            return $"{userId}_{graphName}";
        }
    }
}