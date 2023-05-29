using Darl.GraphQL.Process.Blazor.Connectivity;
using Darl.GraphQL.Process.Blazor.Models;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase;
using DarlCommon;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class DarlQuery : ObjectGraphType<object>
    {
        public DarlQuery(IConnectivity connectivity, IBotProcessing bot, IGraphProcessing graph, ISoftMatchProcessing cmp, IKGTranslation trans, IConfiguration config)
        {
            Name = "Query";
            Description ="View the contents of your account.";

            Field<ListGraphType<KGraphListElementType>>("kgraphs").Description("The set of Knowledge Graph names for this account.")
                .ResolveAsync( async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    var tenantId = trans.GetCurrentTenantId(context.UserContext as GraphQLUserContext);
                    //combine team and individual graphs
                    return await trans.GetKGraphs(userId,tenantId);
                }
            );

            Field<KGraphType>("kGraphByName").Description("Get a single KGraph")
               .Argument<NonNullGraphType<StringGraphType>>("name","Name of the KGraph")
               .ResolveAsync(async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    var name = context.GetArgument<string>("name");
                    return await graph.GetModel(userId, name);
                }
            );

            Field<ListGraphType<LineageRecordType>>("getLineagesForWord")
                .Description("Get the lineages associated with a word")
                .Argument<NonNullGraphType<StringGraphType>> ( "word", "The word to look up" )
                .ResolveAsync(async context =>
                {
                    var word = context.GetArgument<string>("word");
                    return await trans.GetLineagesForWord(word, "en");
                });

            Field<StringGraphType>("getTypeWordForLineage")
                .Argument<NonNullGraphType<StringGraphType>>( "lineage", "The lineage to look up" )
                .ResolveAsync(async context =>
                {
                    var lineage = context.GetArgument<string>("lineage");
                    return await trans.GetTypeWordForLineage(lineage, "en");
                });



            Field<ListGraphType<StringGraphType>>("tokenize")
                .Description("Tokenize a string using the standard en tokenizer")
                .Argument<NonNullGraphType<StringGraphType>>("text","Text to tokenize")
                .Resolve(context =>
                {
                   var text = context.GetArgument<string>("text");
                   return LineageLibrary.SimpleTokenizer(text);
                });

            Field<ListGraphType<GraphObjectType>>("getGraphObjects")
                .Description("Get graph objects based on name and lineage")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the object")
                .Argument<NonNullGraphType<StringGraphType>>("name", "Name of the object")
                .Argument<NonNullGraphType<StringGraphType>>("lineage", "The parent lineage")
                .ResolveAsync(async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var name = context.GetArgument<string>("name");
                    var lineage = context.GetArgument<string>("lineage");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.GetGraphObjects(CompositeName(userId, graphName), name, lineage);
                });


            Field<ListGraphType<GraphObjectType>>("getGraphObjectsByLineage")
                .Description("Get graph objects based on lineage")
                .Argument<NonNullGraphType<StringGraphType>>("graphName","Name of the graph containing the object")
                .Argument<NonNullGraphType<StringGraphType>>("lineage","The parent lineage" )
                .ResolveAsync(async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var name = context.GetArgument<string>("name");
                    var lineage = context.GetArgument<string>("lineage");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.GetGraphObjectsByLineage(CompositeName(userId, graphName), lineage);
                });

            Field<GraphObjectType>("getGraphObjectById")
                .Description("Get a graph object based on id")
                .Argument<NonNullGraphType<StringGraphType>>("graphName","Name of the graph containing the object" )
                .Argument<NonNullGraphType<StringGraphType>>("id","id of the object" )
                .ResolveAsync( async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var id = context.GetArgument<string>("id");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.GetGraphObjectById(CompositeName(userId, graphName), id);
                });

            Field<GraphObjectType>("getVirtualObjectByLineage")
                 .Description("Get a virtual graph object based on lineage")
                 .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the object")
                 .Argument<NonNullGraphType<StringGraphType>>("lineage","Lineage of the object")
                 .ResolveAsync( async context =>
                 {
                     var graphName = context.GetArgument<string>("graphName");
                     var lineage = context.GetArgument<string>("lineage");
                     var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                     return await graph.GetVirtualObjectByLineage(CompositeName(userId, graphName), lineage);
                 });

            Field<GraphObjectType>("getRecognitionObjectById")
                 .Description("Get a recognition graph object based on id")
                 .Argument<NonNullGraphType<StringGraphType>>("graphName","Name of the graph containing the object")
                 .Argument<NonNullGraphType<StringGraphType>>("id","Id of the object")
                 .ResolveAsync( async context =>
                 {
                     var graphName = context.GetArgument<string>("graphName");
                     var id = context.GetArgument<string>("id");
                     var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                     return await graph.GetRecognitionObjectById(CompositeName(userId, graphName), id);
                 });

            Field<GraphObjectType>("getGraphObjectByExternalId")
                .Description("Get a graph object based on an external id")
                .Argument<NonNullGraphType<StringGraphType>>("graphName","Name of the graph containing the object")
                .Argument<NonNullGraphType<StringGraphType>>("externalId","external id of the object")
                .ResolveAsync( async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var id = context.GetArgument<string>("externalId");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.GetGraphObjectByExternalId(CompositeName(userId, graphName), id);
                });

            Field<GraphConnectionType>("getGraphConnection")
                .Description("Get a graph connection based on start and end ids and lineage")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the object")
                .Argument<NonNullGraphType<StringGraphType>>("startId", "id of the start object")
                .Argument<NonNullGraphType<StringGraphType>>("endId", "id of the end object")
                .Argument<NonNullGraphType<StringGraphType>>("lineage", "lineage of the connection")
                .ResolveAsync( async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var startId = context.GetArgument<string>("startId");
                    var endId = context.GetArgument<string>("endId");
                    var lineage = context.GetArgument<string>("lineage");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.GetConnectionByIds(CompositeName(userId, graphName), startId, endId, lineage);
                });

            Field<GraphConnectionType>("getGraphConnectionById")
                .Description("Get a graph connection based on its Id")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the object")
                .Argument<NonNullGraphType<StringGraphType>>("id", "id of the connection")
                .ResolveAsync( async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var id = context.GetArgument<string>("id");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.GetConnectionById(CompositeName(userId, graphName), id);
                });

            Field<KnowledgeStateType>("getKnowledgeState")
                .Description("Get a knowledge state by its Id")
                .Argument<NonNullGraphType<StringGraphType>>("Id", "The knowledge state id")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "The name of the associated Knowledge Graph.")
                .Argument<BooleanGraphType>("external", "ids are ExternalIds")
                .ResolveAsync( async context =>
                {
                    var Id = context.GetArgument<string>("Id");
                    var name = context.GetArgument<string>("graphName");
                    var external = context.GetArgument<bool>("external");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.GetKnowledgeState(userId, Id, name, external);
                });

            Field<KnowledgeStateType>("getKnowledgeStateByExternalId")
                .Description("Get a knowledge state by its external Id")
                .Argument<NonNullGraphType<StringGraphType>>("subjectId", "The external id")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "The name of the associated Knowledge Graph.")
                .Argument<BooleanGraphType>("externalIds", "true returns externalIds for GraphObjects, rather than internal")
                .ResolveAsync(async context =>
                {
                    var subjectId = context.GetArgument<string>("subjectId");
                    var name = context.GetArgument<string>("graphName");
                    var externalIds = context.GetArgument<bool>("externalIds");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.GetKnowledgeStateByExternalId(userId, subjectId, name, externalIds);
                });

            Field<ListGraphType<KnowledgeStateType>>("getKnowledgeStates")
                 .Description("Get all the knowledge states in this graph")
                 .Argument<NonNullGraphType<StringGraphType>>("graphName", "The name of the associated Knowledge Graph.")
                 .ResolveAsync(async context =>
                 {

                     var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                     var name = context.GetArgument<string>("graphName");
                     return await connectivity.GetKnowledgeStates(userId, name);
                 });

            Field<ListGraphType<KnowledgeStateType>>("getKnowledgeStatesByType")
                 .Description("Get all the knowledge states in this graph descended from a particular graph object.")
                 .Argument<NonNullGraphType<StringGraphType>>("graphName", "The name of the associated Knowledge Graph.")
                 .Argument<NonNullGraphType<StringGraphType>>("typeObjectId", "The id of the object these are descended from.")
                 .ResolveAsync( async context =>
                 {

                     var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                     var name = context.GetArgument<string>("graphName");
                     var typeId = context.GetArgument<string>("typeObjectId");
                     return await connectivity.GetKnowledgeStatesByType(userId, typeId, name);
                 });

            Field<ListGraphType<KnowledgeStateType>>("getKnowledgeStatesByTypeAndAttribute")
                 .Description("Get all the knowledge states in this graph descended from a particular graph object containing an attribute with a particular value.")
                 .Argument<NonNullGraphType<StringGraphType>>("graphName","The name of the associated Knowledge Graph.")
                 .Argument<NonNullGraphType<StringGraphType>>("typeObjectId", "The id of the object these are descended from.")
                 .Argument<NonNullGraphType<StringGraphType>>("attLineage", "The lineage of the attribute that must be contained.")
                 .Argument<NonNullGraphType<StringGraphType>>("attValue", "The value required to be present")
                 .Argument<BooleanGraphType>("asSystem",  "Write to system account")
                 .ResolveAsync( async context =>
                 {
                     var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                     var name = context.GetArgument<string>("graphName");
                     var typeId = context.GetArgument<string>("typeObjectId");
                     var attLineage = context.GetArgument<string>("attLineage");
                     var attValue = context.GetArgument<string>("attValue");
                     return await connectivity.GetKnowledgeStatesByTypeAndAttribute(userId, typeId, name, attLineage, attValue);
                 });

            Field<ListGraphType<MatchResultType>>("InferFromSoftMatchModel")
                .Description("Find the nearest matches in a given SoftMatch model to the given set of texts")
                .Argument<NonNullGraphType<StringGraphType>>("modelName", "The concept match model name")
                .Argument<NonNullGraphType<ListGraphType<StringGraphType>>>("texts", "The texts to match. Maximum 50 at a time.")
                .ResolveAsync( async context =>
                {
                    var treeName = context.GetArgument<string>("modelName");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    var texts = context.GetArgument<List<string>>("texts");
                    return await cmp.InferFromSoftMatchModel(userId, treeName, texts);
                });

            Field<ListGraphType<StringGraphType>>("softMatchModels")
                .Description("Get the names of the SoftMatch models in your account")
                .ResolveAsync(async context =>
                {
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await cmp.ListSoftMatchModels(userId);
                });

            Field<ListGraphType<InteractResponseType>>("interactKnowledgeGraph")
                .Description("Perform a chatbot interaction making use of a knowledge graph")
                .Argument<NonNullGraphType<StringGraphType>>("kgModelName", "The knowledge graph to run")
                .Argument<NonNullGraphType<StringGraphType>>("conversationId", "The unique conversation identifier")
                .Argument<NonNullGraphType<DarlVarInputType>>("conversationData", "The input from the other converser.")
                .ResolveAsync( async context =>
                {
                    var kgModelName = context.GetArgument<string>("kgModelName");
                    var conversationId = context.GetArgument<string>("conversationId");
                    var conversationData = context.GetArgument<DarlVar>("conversationData");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await bot.InteractKGAsync(userId, kgModelName, conversationId, conversationData);
                });

            Field<DisplayModelType>("getRealKGDisplay")
                .Description("Get a display version of the KG")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the object")
                .Argument<StringGraphType>("lineageFilter", "optional lineage filter")
                .ResolveAsync( async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var lineageFilter = context.GetArgument<string>("lineageFilter");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.GetRealDisplayGraph(CompositeName(userId, graphName), lineageFilter);
                });

            Field<VRDisplayModelType>("getRealVRKGDisplay")
                .Description("Get a display version of the KG for VR")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the object")
                .Argument<StringGraphType>("lineageFilter", "optional lineage filter")
                .Argument<StringGraphType>("subjectId", "the optional subject Id of the KS used")
                .ResolveAsync(async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var lineageFilter = context.GetArgument<string>("lineageFilter");
                    var subjectId = context.GetArgument<string>("subjectId");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.GetRealVRDisplayGraph(userId, graphName, lineageFilter, subjectId);
                });

            Field<StringGraphType>("getGraphObjectToString")
                .Description("Get a textual overview of an object")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the object")
                .Argument<NonNullGraphType<StringGraphType>>("id", "id of the object")
                .ResolveAsync( async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var id = context.GetArgument<string>("id");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.GetGraphObjectToString(CompositeName(userId, graphName), id);
                });

            Field<DisplayModelType>("getVirtualKGDisplay")
                .Description("Get a display version of the virtual part of the KG")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the object")
                .ResolveAsync( async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.GetVirtualDisplayGraph(CompositeName(userId, graphName));
                }
            );
            Field<DisplayModelType>("getRecognitionKGDisplay")
                .Description("Get a display version of a recognition tree from the KG")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the object")
                .ResolveAsync( async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.GetRecognitionDisplayGraph(CompositeName(userId, graphName));
                }
            );

            Field<GraphAttributeType>("getGraphAttribute")
                .Description("Drill down to an individual attribute within a KG")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the object")
                .Argument<NonNullGraphType<StringGraphType>>("id", "id or externalId of the associated object")
                .Argument<NonNullGraphType<StringGraphType>>("lineage", "lineage of the attribute to find")
                .Argument<StringGraphType>("ksId", "Knowledge State id if required")
                .ResolveAsync( async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var id = context.GetArgument<string>("id");
                    var lineage = context.GetArgument<string>("lineage");
                    var ksId = context.GetArgument<string>("ksId");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.GetGraphAttribute(userId, graphName, id, lineage, ksId);
                }
            );

            //Lint Ruleset
            Field<ListGraphType<DarlLintErrorType>>("lintDarlMeta")
                .Description("Read code in DARL.Meta and return any syntax errors")
                .Argument<NonNullGraphType<StringGraphType>>("darl")
                .ResolveAsync( async context =>
                {
                    var darl = context.GetArgument<string>("darl");
                    return await trans.LintDarlMeta(darl);
                });

            Field<ListGraphType<LineageRecordType>>("getLineagesInKG")
                .Description("Get existing lineages used for this element type in this KG. ")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the elements" )
                .Argument<NonNullGraphType<GraphTypeEnum>>("graphType", "The type of element to find lineages for" )
                .ResolveAsync( async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    var graphType = context.GetArgument<GraphElementType>("graphType");
                    return await graph.GetLineagesInKG(CompositeName(userId, graphName), graphType);
                });
            Field<BooleanGraphType>("isValidLineage")
                .Description("Check if this is a valid lineage. ")
                .Argument<NonNullGraphType<StringGraphType>>("lineage", "the word to check")
                .Resolve( context =>
                {
                    var lineage = context.GetArgument<string>("lineage");
                    return LineageLibrary.CheckLineage(lineage);
                });

            Field<StringGraphType>("getSuggestedRuleset")
                .Description("Get a suggested initial ruleset for an attribute. ")
                .Argument<NonNullGraphType<StringGraphType>>("objectId", "Id of the object containing the new ruleset.")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the object")
                .Argument<NonNullGraphType<StringGraphType>>("lineage", "Lineage of the attribute" )
                .ResolveAsync( async context =>
                {
                    var lineage = context.GetArgument<string>("lineage");
                    var objectId = context.GetArgument<string>("objectId");
                    var graphName = context.GetArgument<string>("graphName");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await trans.GetSuggestedRuleSet(userId, graphName, objectId, lineage);
                });

            Field<ListGraphType<KnowledgeStateType>>("discover")
                .Description("Discover possibilities in a graph")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "The Knowledge graph used")
                .Argument<NonNullGraphType<StringGraphType>>("subjectId", "the subject Id of the start point")
                .ResolveAsync( async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var subjectId = context.GetArgument<string>("subjectId");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await bot.Discover(userId, graphName, subjectId);
                });


            Field<KnowledgeStateType>("getInteractKnowledgeState")
                .Description("Get a knowledge state created during an interaction by its conversationId")
                .Argument<NonNullGraphType<StringGraphType>>("Id", "The conversation id")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "The Knowledge graph involved")
                .Argument<BooleanGraphType>("external", "ids are ExternalIds")
                .ResolveAsync( async context =>
                {
                    var Id = context.GetArgument<string>("Id");
                    var external = context.GetArgument<bool>("external");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    var graphName = context.GetArgument<string>("graphName");
                    return await bot.GetInteractKnowledgeState(Id, userId, graphName, external);
                }
            );
        }

        private string CompositeName(string userId, string graphName)
        {
            return $"{userId}/{graphName}";
        }
    }
}