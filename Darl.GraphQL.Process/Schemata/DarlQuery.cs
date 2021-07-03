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
                                  async c => await trans.GetContacts());
                  }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<ListGraphType<ContactType>>(
              "recentContacts",
                  resolve: async context =>
                  {
                      return await context.TryAsyncResolve(
                                  async c => await trans.GetRecentContacts());
                  }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<ListGraphType<DarlUserType>>(
              "recentUsers",
                  resolve: async context =>
                  {
                      return await context.TryAsyncResolve(
                                  async c => await trans.GetRecentUsers());
                  }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<ListGraphType<DarlUserType>>(
              "users",
                  resolve: async context =>
                  {
                      return await context.TryAsyncResolve(
                                  async c => await trans.GetUsers());
                  }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<ListGraphType<ContactType>>("contactsByLastName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lastName" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await trans.GetContactsByLastName(c.GetArgument<String>("lastName"))); })
                .AuthorizeWith("AdminPolicy");

            FieldAsync<ContactType>("contactByEmail",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await trans.GetContactByEmail(c.GetArgument<String>("email"))); })
                .AuthorizeWith("AdminPolicy");

            FieldAsync<ListGraphType<DefaultType>>("defaults",
                resolve: async context => { return await context.TryAsyncResolve(async c => await trans.GetDefaults()); })
                .AuthorizeWith("AdminPolicy");

            FieldAsync<StringGraphType>("defaultValue",
            arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
            resolve: async context => { return await context.TryAsyncResolve(async c => await trans.GetDefaultValue(c.GetArgument<String>("name"))); })
                .AuthorizeWith("AdminPolicy");

            FieldAsync<KGraphType>(
                "kGraphByName",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await connectivity.GetKGModel(userId, c.GetArgument<String>("name"))
                    );
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

            FieldAsync<ListGraphType<DarlUserType>>("usersByEmail",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await trans.GetUsersByEmail(c.GetArgument<String>("email"))); })
                .AuthorizeWith("AdminPolicy");
            FieldAsync<DarlUserType>("userById",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "userId" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await trans.GetUserById(c.GetArgument<String>("userId"))); })
                .AuthorizeWith("AdminPolicy");
            FieldAsync<DarlUserType>("userByStripeId",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "stripeCustomerId" }),
                resolve: async context => { return await context.TryAsyncResolve(async c => await trans.GetUserByStripeId(c.GetArgument<String>("stripeCustomerId"))); })
                .AuthorizeWith("AdminPolicy");

            FieldAsync<StringGraphType>(
               "getApiKey",
               "Gets the API key for this account. Only accessible to logged in users.",
               resolve: async context =>
               {
                   var userId = trans.GetCurrentUserId(context.UserContext);

                   return await context.TryAsyncResolve(
                       async c => (await trans.GetUserById(userId)).APIKey);
               }
            ).AuthorizeWith("UserPolicy");
            FieldAsync<AccountStateEnum>(
               "getAccountState",
               "Gets the account status for this account. Only accessible to logged in users.",
               resolve: async context =>
               {
                   var userId = trans.GetCurrentUserId(context.UserContext);

                   return await context.TryAsyncResolve(
                       async c => (await trans.GetUserById(userId)).accountState);
               }
            ).AuthorizeWith("UserPolicy");

            FieldAsync<StringGraphType>(
                "getCollateral",
                "Get text used in responses",
                arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the collateral" }),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    return await context.TryAsyncResolve(
                        async c => await trans.GetCollateral(name));
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<ListGraphType<CollateralType>>(
                "collateral",
                "Get texts used in responses",
                resolve: async context =>
                {
                    return await context.TryAsyncResolve(
                        async c => await trans.GetCollaterals());
                }
            ).AuthorizeWith("AdminPolicy");

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
                        async c => await trans.GetLastUpdate(from,to));
                }
            );
            FieldAsync<ListGraphType<UpdateType>>(
                "updates",
                "Get details about most recent updates",
                resolve: async context =>
                {
                    return await context.TryAsyncResolve(
                        async c => await trans.Updates());
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
                        async c => await trans.CheckEmail(email, ipaddress));
                }
            ).AuthorizeWith("AdminPolicy");


            FieldAsync<IntGraphType>(
                "contactCount",
                "Get the count of contacts",
                resolve: async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await trans.GetContactsCount(userId));
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<IntGraphType>(
                "contactCount30Days",
                "Get the count of contacts added in the last 30 days",
                resolve: async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await trans.GetContactsMonthCount(userId));
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<IntGraphType>(
                "contactCountDay",
                "Get the count of contacts added in the last 24 hours",
                resolve: async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await trans.GetContactsDayCount(userId));
                }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<IntGraphType>(
                "userCount",
                "Get the count of users",
                resolve: async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(
                        async c => await trans.GetUserCount(userId));
                }
            ).AuthorizeWith("AdminPolicy");


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
                    return await context.TryAsyncResolve(async c => await graph.GetGraphObjects(CompositeName(userId, graphName), name, lineage));
                }
            ).AuthorizeWith("UserPolicy");
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
                    return await context.TryAsyncResolve(async c => await graph.GetGraphObjectsByLineage(CompositeName(userId, graphName), lineage));
                }
            ).AuthorizeWith("UserPolicy"); 
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
                     var userId = trans.GetCurrentUserId(context.UserContext);
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
                     var userId = trans.GetCurrentUserId(context.UserContext);
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetConnectionById(CompositeName(userId, graphName), id));
                }
            );

            FieldAsync<KnowledgeStateType>(
                "getKnowledgeState",
                "Get a knowledge state by its Id",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "Id", Description = "The knowledge state id" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The name of the associated Knowledge Graph."},
                    new QueryArgument<BooleanGraphType> { Name = "external", Description = "ids are ExternalIds", DefaultValue  = false }
                ),
                resolve: async context =>
                {
                    var Id = context.GetArgument<string>("Id");
                    var name = context.GetArgument<string>("graphName");
                    var external = context.GetArgument<bool>("external");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetKnowledgeState(userId, Id, name, external));
                }
            );

            FieldAsync<KnowledgeStateType>(
                "getKnowledgeStateByExternalId",
                "Get a knowledge state by its external Id",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "subjectId", Description = "The external id" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The name of the associated Knowledge Graph." },
                    new QueryArgument<BooleanGraphType>{ Name = "externalIds", Description = "true returns externalIds for GraphObjects, rather than internal", DefaultValue = false }
                ),
                resolve: async context =>
                {
                    var subjectId = context.GetArgument<string>("subjectId");
                    var name = context.GetArgument<string>("graphName");
                    var externalIds = context.GetArgument<bool>("externalIds");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetKnowledgeStateByExternalId(userId, subjectId,name, externalIds));
                }
            ).AuthorizeWith("UserPolicy");

            FieldAsync<ListGraphType<KnowledgeStateType>>(
                 "getKnowledgeStates",
                 "Get all the knowledge states in this account", arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "The name of the associated Knowledge Graph." }
                 ),
                 resolve: async context =>
                 {

                     var userId = trans.GetCurrentUserId(context.UserContext);
                     var name = context.GetArgument<string>("graphName");
                     return await context.TryAsyncResolve(async c => await connectivity.GetKnowledgeStates(userId,name));
                 }
             ).AuthorizeWith("UserPolicy");


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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    var texts = context.GetArgument<List<string>>("texts");
                    return await context.TryAsyncResolve(async c => await cmp.InferFromSoftMatchModel(userId, treeName, texts));
                }
            ).AuthorizeWith("UserPolicy");
            FieldAsync<ListGraphType<StringGraphType>>(
                "softMatchModels",
                "Get the names of the SoftMatch models in your account",                          
                resolve: async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await cmp.ListSoftMatchModels(userId));
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetRealDisplayGraph(CompositeName(userId, graphName), lineageFilter));
                }
            );

            FieldAsync<VRDisplayModelType>(
                "getRealVRKGDisplay",
                "Get a display version of the KG for VR",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                    new QueryArgument<StringGraphType> { Name = "lineageFilter", Description = "optional lineage filter", DefaultValue = "" }
                ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var lineageFilter = context.GetArgument<string>("lineageFilter");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetRealVRDisplayGraph(CompositeName(userId, graphName), lineageFilter));
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
                    return await context.TryAsyncResolve(async c => await graph.GetGraphObjectToString(CompositeName(userId, graphName), id));
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await context.TryAsyncResolve(async c => await graph.GetRecognitionDisplayGraph(CompositeName(userId, graphName)));
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
                    return await context.TryAsyncResolve(async c => await graph.GetGraphAttribute(userId, graphName,id,lineage,ksId));
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
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
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "objectId", Description = "Id of the object containing the new ruleset."},
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "Lineage of the attribute" }
                ),
            resolve: async context =>
            {
                var lineage = context.GetArgument<string>("lineage");
                var objectId = context.GetArgument<string>("objectId");
                var graphName = context.GetArgument<string>("graphName");
                var userId = trans.GetCurrentUserId(context.UserContext);
                return await context.TryAsyncResolve(async c => await trans.GetSuggestedRuleSet(userId, graphName, objectId, lineage));
            });
            FieldAsync<StringGraphType>("registerForMarketing", "Receive marketing communications from DARL ",
            arguments: new QueryArguments(
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "name of the contact" },
                new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email", Description = "email of the contact" }
             ),
            resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var email = context.GetArgument<string>("email");
                return await context.TryAsyncResolve(async c => await trans.RegisterForMarketing(name,email));
            });
        }

        private string CompositeName(string userId, string graphName)
        {
            return $"{userId}_{graphName}";
        }
    }
}