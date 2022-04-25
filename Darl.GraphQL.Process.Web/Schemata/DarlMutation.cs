using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Schemata;
using Darl.Thinkbase;
using GraphQL;
using GraphQL.Types;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;

namespace Darl.GraphQL.Web.Models.Schemata
{
    public class DarlMutation : ObjectGraphType<object>
    {
        public DarlMutation(IConnectivity connectivity, IEmailProcessing email, IGraphProcessing graph, IConfiguration _config, ISoftMatchProcessing cmp, IKGTranslation trans)
        {
            Name = "Mutation";
            Description = "Make changes to the contents of your account.";
            this.AuthorizeWith("UserPolicy");
            // Default
            //  Create
            FieldAsync<DefaultType>("createDefault", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }, new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "value" }),
                resolve: async context =>
            {
                var name = context.GetArgument<string>("name");
                var value = context.GetArgument<string>("value");
                return await trans.CreateDefault(name, value);
            }).AuthorizeWith("AdminPolicy");
            FieldAsync<DefaultType>("updateDefault", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }, new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "value" }),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var value = context.GetArgument<string>("value");
                    return await trans.UpdateDefault(name, value);
                }).AuthorizeWith("AdminPolicy");
            //  Delete
            FieldAsync<DefaultType>("deleteDefault", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name" }),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    return await trans.DeleteDefault(name);
                }).AuthorizeWith("AdminPolicy");

            FieldAsync<StringGraphType>(
               "resetApiKey", "Regenerate your API key",
               resolve: async context =>
               {
                   var userId = trans.GetCurrentUserId(context.UserContext);

                   return await trans.UpdateUserAPIKey(userId);
               }
            );
            FieldAsync<CollateralType>(
                "updateCollateral",
                "update text used in responses",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the collateral" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "value", Description = "The value of the collateral" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var value = context.GetArgument<string>("value");
                    return await trans.UpdateCollateral(name, value);
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<CollateralType>(
                "deleteCollateral",
                "Delete text used in responses",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the collateral" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    return await trans.DeleteCollateral(name);
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<ContactType>(
                "deleteContact",
                "Delete a contact",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email", Description = "The email of the contact" }
                ),
                resolve: async context =>
                {
                    var email = context.GetArgument<string>("email");
                    return await trans.DeleteContactAsync(email);
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<DateTimeGraphType>(
                "setLastUpdate",
                "Set the utc time of a system wide update.",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "from", Description = "The source of the update" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "to", Description = "The destination of the update" }
                ),
                resolve: async context =>
                {
                    var from = context.GetArgument<string>("from");
                    var to = context.GetArgument<string>("to");
                    return await trans.SetLastUpdate(from, to);
                }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<IntGraphType>(
                "mailshot",
                "send a mailshot",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "collateral", Description = "Collateral to use for the body" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "subject", Description = "Email subject" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "sendfrom", Description = "Source email" },
                    new QueryArgument<BooleanGraphType> { Name = "test", DefaultValue = false, Description = "if true no emails are sent" }
                    ),
                resolve: async context =>
                {
                    var collateral = context.GetArgument<String>("collateral");
                    var subject = context.GetArgument<String>("subject");
                    var sendfrom = context.GetArgument<String>("sendfrom");
                    var test = context.GetArgument<bool>("test");
                    return await email.Mailshot(collateral, subject, sendfrom, test);
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<StringGraphType>(
                "email",
                "send an email",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "body", Description = "Body of the email" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "subject", Description = "Email subject" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "sendfrom", Description = "Source email" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "emailAddress", Description = "email of recipient" }
                    ),
                resolve: async context =>
                {
                    var body = context.GetArgument<String>("body");
                    var subject = context.GetArgument<String>("subject");
                    var sendfrom = context.GetArgument<String>("sendfrom");
                    var emailAddress = context.GetArgument<String>("emailAddress");
                    return await email.SendEmail(body, subject, sendfrom, emailAddress);
                }
            ).AuthorizeWith("AdminPolicy");
            FieldAsync<StringGraphType>(
                "createNewsItem",
                "create a news item",
                arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "title", Description = "title of the news item" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "content", Description = "Content of the news item in markdown" }
                    ),
                resolve: async context =>
                {
                    var title = context.GetArgument<String>("title");
                    var content = context.GetArgument<String>("content");
                    return await trans.CreateNewsItem(title,content);
                }
            ).AuthorizeWith("AdminPolicy");

            FieldAsync<GraphObjectType>("createGraphObject", "Add a new graph object", arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph containing the object" },
                    new QueryArgument<NonNullGraphType<GraphObjectInputType>> { Name = "graphObject", Description = "The object to add" },
                    new QueryArgument<OntologyActionEnum> { Name = "ontology", Description = "builds, checks against or ignores ontology" }
               ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var graphObject = context.GetArgument<GraphObjectInput>("graphObject");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    var ontology = context.GetArgument<OntologyAction>("ontology");
                    return await graph.CreateGraphObject(CompositeName(userId, graphName), graphObject, ontology);
                }
            );
            FieldAsync<GraphConnectionType>("createGraphConnection", "Add a new graph connection", arguments: new QueryArguments(
                     new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph to modify" },
                   new QueryArgument<NonNullGraphType<GraphConnectionInputType>> { Name = "graphConnection", Description = "The connection to add" },
                    new QueryArgument<OntologyActionEnum> { Name = "ontology", Description = "builds, checks against or ignores ontology" }
               ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var graphConnection = context.GetArgument<GraphConnectionInput>("graphConnection");
                    var ontology = context.GetArgument<OntologyAction>("ontology");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.CreateGraphConnection(CompositeName(userId, graphName), graphConnection, ontology);
                }
            );
            FieldAsync<GraphObjectType>("deleteGraphObject", "Delete a graphObject", arguments: new QueryArguments(
                     new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph to modify" },
                     new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The id of the object to delete" }
                ),
                 resolve: async context =>
                 {
                     var graphName = context.GetArgument<string>("graphName");
                     var id = context.GetArgument<string>("id");
                     var userId = trans.GetCurrentUserId(context.UserContext);
                     return await graph.DeleteGraphObject(CompositeName(userId, graphName), id);
                 }
             );
            FieldAsync<GraphConnectionType>("deleteGraphConnection", "Delete a graph connection", arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph to modify" },
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "id", Description = "The id of the connection to delete" }
               ),
                resolve: async context =>
                {
                    var graphName = context.GetArgument<string>("graphName");
                    var id = context.GetArgument<string>("id");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.DeleteGraphConnection(CompositeName(userId, graphName), id);
                }
            );
            FieldAsync<GraphObjectType>("updateGraphObject", "Update a graph object", arguments: new QueryArguments(
                    new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "graphName", Description = "Name of the graph to modify" },
                    new QueryArgument<NonNullGraphType<GraphObjectUpdateType>> { Name = "graphObject", Description = "The object to update" },
                    new QueryArgument<OntologyActionEnum> { Name = "ontology", Description = "builds, checks against or ignores ontology" }
                ),
                 resolve: async context =>
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

            FieldAsync<StringGraphType>("createKey", "Create a licensing key", arguments: new QueryArguments(
                        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "company", Description = "The company granted the license" },
                        new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email", Description = "The email address of the company" },
                        new QueryArgument<NonNullGraphType<DateTimeGraphType>> { Name = "endDate", Description = "The date the license ends" }
                   ),
                    resolve: async context =>
                    {
                        var company = context.GetArgument<string>("company");
                        var userId = trans.GetCurrentUserId(context.UserContext);
                        var email = context.GetArgument<string>("email");
                        var endDate = context.GetArgument<DateTime>("endDate");

                        return await trans.CreateKey(userId, company, email, endDate);
                    }
                ).AuthorizeWith("AdminPolicy");
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

            FieldAsync<StringGraphType>("inviteUser", arguments: new QueryArguments(new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "email" }), resolve: async context =>
            {
                var newUserEmail = context.GetArgument<string>("email");
                var userId = trans.GetCurrentUserId(context.UserContext);
                return await email.InviteUser(userId, newUserEmail);
            }).AuthorizeWith("CorpPolicy");

            FieldAsync<StringGraphType>("copyRenamKG", "copy and rename a Knowledge graph", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "name", Description = "The name of the Knowledge graph to copy" },
                 new QueryArgument<NonNullGraphType<StringGraphType>> { Name = "newName", Description = "The new name of the copied Knowledge Graph" }
                ),
                resolve: async context =>
                {
                    var name = context.GetArgument<string>("name");
                    var newName = context.GetArgument<string>("newName");
                    var userId = trans.GetCurrentUserId(context.UserContext);
                    return await graph.CopyRenameKG(userId, name, newName);
                }
            );

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
            FieldAsync<StringGraphType>("deleteVirtualObjectAttribute", "update or add an attribute of a virtual GraphObject", arguments: new QueryArguments(
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
            FieldAsync<KnowledgeStateType>("createKnowledgeState", "Creates or updates a knowledge state", arguments: new QueryArguments(
                 new QueryArgument<NonNullGraphType<KnowledgeStateInputType>> { Name = "ks", Description = "The new knowledge state" },
                 new QueryArgument<BooleanGraphType> { Name = "asSystem", Description = "Write to system account", DefaultValue = false }
                 //hide if not admin
                ),
                resolve: async context =>
                {
                    KnowledgeStateInput ks = (KnowledgeStateInput)context.GetArgument(typeof(KnowledgeStateInput), "ks");
                    var asSystem = (bool?)context.GetArgument(typeof(bool?), "asSystem");
                    if (asSystem ?? false)
                    {
                        var userId = _config["AppSettings:boaiuserid"];
                        return await graph.CreateKnowledgeState(userId, ks);
                    }
                    else
                    {
                        var userId = trans.GetCurrentUserId(context.UserContext);
                        return await graph.CreateKnowledgeState(userId, ks);
                    }

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
