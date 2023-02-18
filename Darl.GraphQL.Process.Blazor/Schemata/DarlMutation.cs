using Darl.GraphQL.Process.Blazor.Connectivity;
using Darl.GraphQL.Process.Blazor.Models;
using Darl.Thinkbase;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class DarlMutation : ObjectGraphType<object>
    {
        public DarlMutation(IConnectivity connectivity, IGraphProcessing graph, IConfiguration _config, ISoftMatchProcessing cmp, IKGTranslation trans)
        {
            Name = "Mutation";
            Description = "Make changes to the contents of your account.";

            Field<GraphObjectType>("createGraphObject")
                .Description("Add a new graph object")
                .Argument <NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the object")
                .Argument<NonNullGraphType<GraphObjectInputType>>("graphObject", "The object to add")
                .Argument<OntologyActionEnum>("ontology", "builds, checks against or ignores ontology")
                .ResolveAsync(async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var graphObject = context.GetArgument<GraphObjectInput>("graphObject");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    var ontology = context.GetArgument<OntologyAction>("ontology");
                    return await graph.CreateGraphObject(CompositeName(userId, graphName), graphObject, ontology);
                });

            Field<GraphConnectionType>("createGraphConnection")
                .Description("Add a new graph connection")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the object")
                .Argument<NonNullGraphType<GraphConnectionInputType>>("graphConnection", "The connection to add")
                .Argument<OntologyActionEnum>("ontology", "builds, checks against or ignores ontology")
                .ResolveAsync(async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var graphConnection = context.GetArgument<GraphConnectionInput>("graphConnection");
                    var ontology = context.GetArgument<OntologyAction>("ontology");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.CreateGraphConnection(CompositeName(userId, graphName), graphConnection, ontology);
                });

            Field<GraphObjectType>("deleteGraphObject")
                .Description("Delete a graphObject")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the object")
                .Argument<NonNullGraphType<StringGraphType>>("id", "The id of the object to delete")
                .ResolveAsync(async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var id = context.GetArgument<string>("id");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.DeleteGraphObject(CompositeName(userId, graphName), id);
                });

            Field<GraphConnectionType>("deleteGraphConnection")
                .Description("Delete a graph connection")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph to modify" )
                .Argument<NonNullGraphType<StringGraphType>>("id", "The id of the connection to delete" )
                .ResolveAsync(async context =>
                    {
                        var graphName = context.GetArgument<string>("graphName");
                        var id = context.GetArgument<string>("id");
                        var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                        return await graph.DeleteGraphConnection(CompositeName(userId, graphName), id);
                    }
                );

            Field<GraphObjectType>("updateGraphObject")
                .Description("Update a graph object")
                .Argument<NonNullGraphType<StringGraphType>>("graphName", "Name of the graph to modify" )
                .Argument<NonNullGraphType<GraphObjectUpdateType>>("graphObject","The object to update" )
                .Argument<OntologyActionEnum>("ontology","builds, checks against or ignores ontology")
                .ResolveAsync( async context =>
                 {
                     var graphName = context.GetArgument<string>("graphName");
                     var graphObject = context.GetArgument<GraphObjectUpdate>("graphObject");
                     var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                     var ontology = context.GetArgument<OntologyAction>("ontology");
                     return await graph.UpdateGraphObject(CompositeName(userId, graphName), graphObject, ontology);
                 }
             );

            Field<GraphConnectionType>("updateGraphConnection")
                .Description("Update a graph connection")
                .Argument<NonNullGraphType<StringGraphType>>("graphName","Name of the graph to modify")
                .Argument<NonNullGraphType<GraphConnectionUpdateType>>("graphConnection","The connection to update")
                .Argument<OntologyActionEnum>("ontology","builds, checks against or ignores ontology")
                .ResolveAsync(async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var graphConnection = context.GetArgument<GraphConnectionUpdate>("graphConnection");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    var ontology = context.GetArgument<OntologyAction>("ontology");
                    return await graph.UpdateGraphConnection(CompositeName(userId, graphName), graphConnection, ontology);
                });

            Field<StringGraphType>("createSoftMatchModel")
                .Description("Create a SoftMatch model from text/index pairs")
                .Argument<NonNullGraphType<StringGraphType>>("modelName","The unique name of the stored model for later reuse ")
                .Argument<NonNullGraphType<ListGraphType<StringStringPairInputType>>>("data","The text/index data to add to the SoftMatch model")
                .Argument<BooleanGraphType>("rebuild","if false (default) add to existing model, otherwise create a new model.")
                .ResolveAsync(async context =>
                 {
                     var treeName = context.GetArgument<string>("modelName");
                     var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                     var data = context.GetArgument<List<StringStringPair>>("data");
                     return await cmp.CreateSoftMatchModel(userId, treeName, data);
                 });

            Field<StringGraphType>("deleteSoftMatchModel")
                .Description("delete a SoftMatch model")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the SoftMatch model to delete")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await cmp.DeleteSoftMatchModel(userId, name);
                });

            Field<BooleanGraphType>("createKGraph")
                .Argument<NonNullGraphType<StringGraphType>>("name","The unique name of the stored model for later reuse ")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await trans.CreateNewGraph(userId, name);
                });

            Field<BooleanGraphType>("deleteKG")
                .Description("Delete a Knowledge graph")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the Knowledge graph to delete")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.DeleteGraph(userId, name);
                });

            Field<StringGraphType>("saveKGraph")
                .Argument<NonNullGraphType<StringGraphType>>("name")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    await graph.Store(CompositeName(userId, name));
                    return "";
                });

            Field<StringGraphType>("promoteKGraph")
                .Argument<NonNullGraphType<StringGraphType>>("name")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    var tenantId = trans.GetCurrentTenantId(context.UserContext as GraphQLUserContext);
                    await trans.Promote(userId, tenantId, name);
                    return "";
                });

            Field<GraphObjectType>("updateRecognitionObject")
                .Description("update a GraphObject in the recognition trees")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the Knowledge graph the object is in")
                .Argument<NonNullGraphType<GraphObjectUpdateType>>("object","The object to update")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var obj = context.GetArgument<GraphObjectUpdate>("object");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.UpdateRecognitionObject(CompositeName(userId, name), obj);
                }
            );

            Field<GraphObjectType>("createRecognitionObject")
                .Description("create a GraphObject in the recognition trees")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the Knowledge graph the object is to be in")
                .Argument<NonNullGraphType<GraphObjectInputType>>("object","The object to create")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var obj = context.GetArgument<GraphObjectInput>("object");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.CreateRecognitionObject(CompositeName(userId, name), obj);
                });

            Field<GraphConnectionType>("createRecognitionConnection")
                .Description("create a GraphConnection in the recognition trees")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the Knowledge graph the object is to be in")
                .Argument<NonNullGraphType<GraphConnectionInputType>>("connection","The connection to create")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var conn = context.GetArgument<GraphConnectionInput>("connection");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.CreateRecognitionConnection(CompositeName(userId, name), conn);
                });

            Field<StringGraphType>("deleteRecognitionObject")
                .Description("Delete a GraphObject in the recognition trees")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the Knowledge graph the object is in")
                .Argument<NonNullGraphType<StringGraphType>>("id","The id of the object to delete")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var id = context.GetArgument<string>("id");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.DeleteRecognitionObject(CompositeName(userId, name), id);
                });

            Field<StringGraphType>("updateRecognitionObjectAttribute")
                .Description("update or add an attribute of a GraphObject in the recognition trees")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the Knowledge graph the object is in" )
                .Argument<NonNullGraphType<StringGraphType>>("id","The id of the parent object" )
                .Argument<NonNullGraphType<GraphAttributeInputType>>("att","The attribute to update")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var id = context.GetArgument<string>("id");
                    var att = context.GetArgument<GraphAttributeInput>("att");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.UpdateRecognitionObjectAttribute(CompositeName(userId, name), id, att);
                });

            Field<StringGraphType>("updateVirtualObjectAttribute")
                 .Description("update or add an attribute of a virtual GraphObject")
                 .Argument<NonNullGraphType<StringGraphType>>("name","The name of the Knowledge graph the object is in")
                 .Argument<NonNullGraphType<StringGraphType>>("lineage","The lineage of the parent object")
                 .Argument<NonNullGraphType<GraphAttributeInputType>>("att","The attribute to update")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var lineage = context.GetArgument<string>("lineage");
                    var att = context.GetArgument<GraphAttributeInput>("att");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.UpdateVirtualObjectAttribute(CompositeName(userId, name), lineage, att);
                }
            );
            Field<StringGraphType>("deleteVirtualObjectAttribute")
                .Description("Delete an attribute of a virtual GraphObject")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the Knowledge graph the object is in")
                .Argument<NonNullGraphType<StringGraphType>>("lineage","The lineage of the parent object")
                .Argument<NonNullGraphType<StringGraphType>>("attLineage","The lineage of the attribute to delete")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var lineage = context.GetArgument<string>("lineage");
                    var attLineage = context.GetArgument<string>("attLineage");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.DeleteVirtualObjectAttribute(CompositeName(userId, name), lineage, attLineage);
                }
            );
            Field<StringGraphType>("deleteRecognitionObjectAttribute")
                .Description("delete an attribute of a recognition GraphObject")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the Knowledge graph the object is in")
                .Argument<NonNullGraphType<StringGraphType>>("id","The id of the parent object")
                .Argument<NonNullGraphType<StringGraphType>>("attLineage","The lineage of the attribute to delete")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var id = context.GetArgument<string>("id");
                    var attLineage = context.GetArgument<string>("attLineage");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.DeleteRecognitionObjectAttribute(CompositeName(userId, name), id, attLineage);
                }
            );
            Field<StringGraphType>("deleteGraphObjectAttribute")
                .Description("delete an attribute of a real GraphObject")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the Knowledge graph the object is in")
                .Argument<NonNullGraphType<StringGraphType>>("id","The id of the parent object")
                .Argument<NonNullGraphType<StringGraphType>>("attLineage","The lineage of the attribute to delete")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var id = context.GetArgument<string>("id");
                    var attLineage = context.GetArgument<string>("attLineage");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.DeleteGraphObjectAttribute(CompositeName(userId, name), id, attLineage);
                });
            Field<GraphAttributeType>("updateGraphObjectAttribute")
                .Description("update or add an attribute of a real GraphObject")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the Knowledge graph the object is in")
                .Argument<NonNullGraphType<StringGraphType>>("id","The id of the parent object" )
                .Argument<NonNullGraphType<GraphAttributeInputType>>("att","The attribute to update" )
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var id = context.GetArgument<string>("id");
                    var att = context.GetArgument<GraphAttributeInput>("att");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.UpdateGraphObjectAttribute(CompositeName(userId, name), id, att);
                });

            Field<GraphObjectType>("CreateRecognitionRoot")
                .Description("Create a new root in the recognition trees")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the Knowledge graph the object is in" )
                .Argument<NonNullGraphType<StringGraphType>>("lineage","The lineage of the root")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var lineage = context.GetArgument<string>("lineage");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.CreateRecognitionRoot(CompositeName(userId, name), lineage);
                });

            Field<ModelMetaDataType>("UpdateKGraphMetadata")
                .Description("Update the meta-data of a knowledge graph")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the Knowledge graph to update")
                .Argument<NonNullGraphType<ModelMetaDataUpdateType>>("update","The update")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var update = context.GetArgument<ModelMetaData>("update");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.UpdateKGraph(userId, name, update);
                }
            );
            Field<KnowledgeStateType>("createKnowledgeState")
                .Description("Creates or updates a knowledge state")
                .Argument<NonNullGraphType<KnowledgeStateInputType>>("ks", "The new knowledge state" )
                .ResolveAsync(async context =>
                {
                    KnowledgeStateInput ks = (KnowledgeStateInput)context.GetArgument(typeof(KnowledgeStateInput), "ks");
                    var asSystem = (bool?)context.GetArgument(typeof(bool?), "asSystem");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.CreateKnowledgeState(userId, ks);
                }
            );

            Field<ListGraphType<KnowledgeStateType>>("createKnowledgeStateList")
                .Description("Creates or updates a list of knowledge states in order. Maximum count is 50")
                .Argument<NonNullGraphType<ListGraphType<KnowledgeStateInputType>>>("ksl","The new knowledge states")
                .ResolveAsync(async context =>
                {
                    var ksl = context.GetArgument<List<KnowledgeStateInput>>("ks");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.CreateKnowledgeStateList(userId, ksl);
                });

            Field<KnowledgeStateType>("deleteKnowledgeState")
                .Description("deletes a knowledge state")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the graph it belongs to ")
                .Argument<NonNullGraphType<StringGraphType>>("subjectId","The subjectId of the KS")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var subjectId = context.GetArgument<string>("subjectId");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await connectivity.DeleteKnowledgeState(userId, subjectId, name);
                });

            Field<ULongGraphType>("deleteAllKnowledgeStates")
                .Description("deletes all knowledge states for a graph")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the graph they belong to ")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await connectivity.DeleteAllKnowledgeStates(userId, name);
                });

            Field<StringGraphType>("loadExternalData")
                .Description("Turn Json or XML data into Knowledge States")
                .Argument<NonNullGraphType<StringGraphType>>("name","The name of the Knowledge graph to create KStates for" )
                .Argument<NonNullGraphType<StringGraphType>>("data","The XML or Json source" )
                .Argument<NonNullGraphType<StringGraphType>>("patternPath","The XPath or JPath pattern locator" )
                .Argument<NonNullGraphType<ListGraphType<DataMapType>>>("dataMaps","List of maps for individual data items")
                .ResolveAsync(async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var data = context.GetArgument<string>("data");
                    var patternPath = context.GetArgument<string>("patternPath");
                    var dataMaps = context.GetArgument<List<Thinkbase.DataMap>>("dataMaps");
                    var userId = trans.GetCurrentUserId(context.UserContext as GraphQLUserContext);
                    return await graph.LoadExternalData(userId, name, data, patternPath, dataMaps);
                });
        }

        private string CompositeName(string userId, string graphName)
        {
            return $"{userId}_{graphName}";
        }
    }
}
