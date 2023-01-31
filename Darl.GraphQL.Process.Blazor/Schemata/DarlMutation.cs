using Darl.GraphQL.Process.Blazor.Connectivity;
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
            this.AuthorizeWithPolicy("UserPolicy");

            Field<GraphObjectType>("createGraphObject")
                .Description("Add a new graph object")
                .Argument <NonNullGraphType<StringGraphType>>("graphName", "Name of the graph containing the object")
                .Argument<NonNullGraphType<GraphObjectInputType>>("graphObject", "The object to add")
                .Argument<OntologyActionEnum>("ontology", "builds, checks against or ignores ontology")
                .ResolveAsync(async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var graphObject = context.GetArgument<GraphObjectInput>("graphObject");
                    var userId = trans.GetCurrentUserId(context.UserContext);
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
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
                     var userId = trans.GetCurrentUserId(context.UserContext);
                     var ontology = context.GetArgument<OntologyAction>("ontology");
                     return await graph.UpdateGraphObject(CompositeName(userId, graphName), graphObject, ontology);
                 }
             );

            FieldAsync<GraphConnectionType>("updateGraphConnection",
                    "Update a graph connection", arguments: new QueryArguments(
                        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph to modify" },
                        new QueryArgument<NonNullGraphType<GraphConnectionUpdateType>> { Name = "graphConnection", Description = "The connection to update" },
                        new QueryArgument<OntologyActionEnum> { Name = "ontology", Description = "builds, checks against or ignores ontology" }
                   ),
                    resolve: async context =>
                    {
                        var graphName = context.GetArgument<string>("graphName");
                        var graphConnection = context.GetArgument<GraphConnectionUpdate>("graphConnection");
                        var userId = trans.GetCurrentUserId(context.UserContext);
                        var ontology = context.GetArgument<OntologyAction>("ontology");
                        return await graph.UpdateGraphConnection(CompositeName(userId, graphName), graphConnection, ontology);
                    }
                );

            FieldAsync<StringGraphType>("createSoftMatchModel", "Create a SoftMatch model from text/index pairs", arguments: new QueryArguments(
                     new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "modelName", Description = "The unique name of the stored model for later reuse " },
                     new QueryArgument<NonNullGraphType<ListGraphType<StringStringPairInputType>>> { Name = "data", Description = "The text/index data to add to the SoftMatch model" },
                     new QueryArgument<BooleanGraphType> { Name = "rebuild", Description = "if false (default) add to existing model, otherwise create a new model.", DefaultValue = false }
                 ),
                 resolve: async context =>
                 {
                     var treeName = context.GetArgument<string>("modelName");
                     var userId = trans.GetCurrentUserId(context.UserContext);
                     var data = context.GetArgument<List<StringStringPair>>("data");

                     return await cmp.CreateSoftMatchModel(userId, treeName, data);
                 }
             );
            FieldAsync<StringGraphType>("deleteSoftMatchModel", "delete a SoftMatch model", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the SoftMatch model to delete" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await cmp.DeleteSoftMatchModel(userId, name);
                }
            );

            FieldAsync<BooleanGraphType>("createKGraph", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The unique name of the stored model for later reuse " }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var userId = trans.GetCurrentUserId(context.UserContext);
                return await trans.CreateNewGraph(userId, name);
            });

            FieldAsync<BooleanGraphType>("deleteKG", "Delete a Knowledge graph", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph to delete" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.DeleteGraph(userId, name);
                }
            );

            FieldAsync<StringGraphType>("saveKGraph", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }), resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var userId = trans.GetCurrentUserId(context.UserContext);
                await graph.Store(CompositeName(userId, name));
                return "";
            });

            FieldAsync<GraphObjectType>("updateRecognitionObject", "update a GraphObject in the recognition trees", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<GraphObjectUpdateType>> { Name = "object", Description = "The object to update" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var obj = context.GetArgument<GraphObjectUpdate>("object");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.UpdateRecognitionObject(CompositeName(userId, name), obj);
                }
            );

            FieldAsync<GraphObjectType>("createRecognitionObject", "create a GraphObject in the recognition trees", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is to be in" },
                 new QueryArgument<NonNullGraphType<GraphObjectInputType>> { Name = "object", Description = "The object to create" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var obj = context.GetArgument<GraphObjectInput>("object");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.CreateRecognitionObject(CompositeName(userId, name), obj);
                }
            );

            FieldAsync<GraphConnectionType>("createRecognitionConnection", "create a GraphConnection in the recognition trees", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is to be in" },
                 new QueryArgument<NonNullGraphType<GraphConnectionInputType>> { Name = "connection", Description = "The connection to create" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var conn = context.GetArgument<GraphConnectionInput>("connection");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.CreateRecognitionConnection(CompositeName(userId, name), conn);
                }
            );

            FieldAsync<StringGraphType>("deleteRecognitionObject", "Delete a GraphObject in the recognition trees", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The id of the object to delete" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var id = context.GetArgument<string>("id");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.DeleteRecognitionObject(CompositeName(userId, name), id);
                }
            );

            FieldAsync<StringGraphType>("updateRecognitionObjectAttribute", "update or add an attribute of a GraphObject in the recognition trees", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The id of the parent object" },
                 new QueryArgument<NonNullGraphType<GraphAttributeInputType>> { Name = "att", Description = "The attribute to update" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var id = context.GetArgument<string>("id");
                    var att = context.GetArgument<GraphAttributeInput>("att");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.UpdateRecognitionObjectAttribute(CompositeName(userId, name), id, att);
                }
            );

            FieldAsync<StringGraphType>("updateVirtualObjectAttribute", "update or add an attribute of a virtual GraphObject", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "The lineage of the parent object" },
                 new QueryArgument<NonNullGraphType<GraphAttributeInputType>> { Name = "att", Description = "The attribute to update" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var lineage = context.GetArgument<string>("lineage");
                    var att = context.GetArgument<GraphAttributeInput>("att");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.UpdateVirtualObjectAttribute(CompositeName(userId, name), lineage, att);
                }
            );
            FieldAsync<StringGraphType>("deleteVirtualObjectAttribute", "Delete an attribute of a virtual GraphObject", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "The lineage of the parent object" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "attLineage", Description = "The lineage of the attribute to delete" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var lineage = context.GetArgument<string>("lineage");
                    var attLineage = context.GetArgument<string>("attLineage");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.DeleteVirtualObjectAttribute(CompositeName(userId, name), lineage, attLineage);
                }
            );
            FieldAsync<StringGraphType>("deleteRecognitionObjectAttribute", "delete an attribute of a recognition GraphObject", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The id of the parent object" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "attLineage", Description = "The lineage of the attribute to delete" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var id = context.GetArgument<string>("id");
                    var attLineage = context.GetArgument<string>("attLineage");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.DeleteRecognitionObjectAttribute(CompositeName(userId, name), id, attLineage);
                }
            );
            FieldAsync<StringGraphType>("deleteGraphObjectAttribute", "delete an attribute of a real GraphObject", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The id of the parent object" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "attLineage", Description = "The lineage of the attribute to delete" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var id = context.GetArgument<string>("id");
                    var attLineage = context.GetArgument<string>("attLineage");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.DeleteGraphObjectAttribute(CompositeName(userId, name), id, attLineage);
                }
            );
            FieldAsync<GraphAttributeType>("updateGraphObjectAttribute", "update or add an attribute of a real GraphObject", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The id of the parent object" },
                 new QueryArgument<NonNullGraphType<GraphAttributeInputType>> { Name = "att", Description = "The attribute to update" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var id = context.GetArgument<string>("id");
                    var att = context.GetArgument<GraphAttributeInput>("att");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.UpdateGraphObjectAttribute(CompositeName(userId, name), id, att);
                }
            );
            FieldAsync<GraphObjectType>("CreateRecognitionRoot", "Create a new root in the recognition trees", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph the object is in" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "lineage", Description = "The lineage of the root" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var lineage = context.GetArgument<string>("lineage");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.CreateRecognitionRoot(CompositeName(userId, name), lineage);
                }
            );
            FieldAsync<ModelMetaDataType>("UpdateKGraphMetadata", "Update the meta-data of a knowledge graph", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph to update" },
                 new QueryArgument<NonNullGraphType<ModelMetaDataUpdateType>> { Name = "update", Description = "The update" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var update = context.GetArgument<ModelMetaData>("update");
                    var userId = trans.GetCurrentUserId(context.UserContext);
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
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.CreateKnowledgeState(userId, ks);
                }
            );
            FieldAsync<ListGraphType<KnowledgeStateType>>("createKnowledgeStateList", "Creates or updates a list of knowledge states in order. Maximum count is 50", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<ListGraphType<KnowledgeStateInputType>>> { Name = "ksl", Description = "The new knowledge states" }
                ),
                resolve: async context =>
                {
                    var ksl = context.GetArgument<List<KnowledgeStateInput>>("ks");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.CreateKnowledgeStateList(userId, ksl);
                }
            );
            FieldAsync<KnowledgeStateType>("deleteKnowledgeState", "deletes a knowledge state", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the graph it belongs to " },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "subjectId", Description = "The subjectId of the KS" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var subjectId = context.GetArgument<string>("subjectId");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await connectivity.DeleteKnowledgeState(userId, subjectId, name);
                }
            );
            FieldAsync<ULongGraphType>("deleteAllKnowledgeStates", "deletes all knowledge states for a graph", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the graph they belong to " }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await connectivity.DeleteAllKnowledgeStates(userId, name);
                }
            );
            FieldAsync<StringGraphType>("loadExternalData", "Turn Json or XML data into Knowledge States", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph to create KStates for" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "data", Description = "The XML or Json source" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "patternPath", Description = "The XPath or JPath pattern locator" },
                 new QueryArgument<NonNullGraphType<ListGraphType<DataMapType>>> { Name = "dataMaps", Description = "List of maps for individual data items" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var data = context.GetArgument<string>("data");
                    var patternPath = context.GetArgument<string>("patternPath");
                    var dataMaps = context.GetArgument<List<Thinkbase.DataMap>>("dataMaps");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.LoadExternalData(userId, name, data, patternPath, dataMaps);
                }
            );
        }

        private string CompositeName(string userId, string graphName)
        {
            return $"{userId}_{graphName}";
        }
    }
}
