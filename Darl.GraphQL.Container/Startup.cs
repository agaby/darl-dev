using Darl.GraphQL.Container.Models.Schemata;
using Darl.GraphQL.Container.Ui.GraphiQL;
using Darl.GraphQL.Container.Ui.Playground;
using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Middleware;
using Darl.GraphQL.Models.Schemata;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase;
using Darl.Thinkbase.Meta;
using DarlLanguage.Processing;
using GraphQL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using System;
using System.Security.Principal;

namespace Darl.GraphQL.Container
{
    public class Startup
    {

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddDistributedMemoryCache();

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(3600);
                options.Cookie.HttpOnly = true;
            });

            services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
            services.AddSingleton<IBotProcessing, BotProcessing>();
            services.AddSingleton<IGraphProcessing, GraphProcessing>();
            services.AddSingleton<ILicensing, ProductLicensing>();
            services.AddSingleton<ISoftMatchProcessing, SoftMatchProcessing>();
            services.AddSingleton<ILocalStore, GraphLocalStore>();
            services.AddSingleton<IGraphPrimitives, BlobGraphPrimitives>();
            services.AddSingleton<IGraphHandler, GraphHandler>();
            services.AddSingleton<IMetaStructureHandler, MetaStructureHandler>();
            services.AddSingleton<IDarlMetaRunTime, DarlMetaRunTime>();

            //types
            services.AddSingleton<DictionarySequenceType>();
            services.AddSingleton<DisplayTypeEnum>();
            services.AddSingleton<FormFormatType>();
            services.AddSingleton<InputFormatType>();
            services.AddSingleton<InputTypeEnum>();
            services.AddSingleton<LineageAnnotationNodeType>();
            services.AddSingleton<LineageElementType>();
            services.AddSingleton<LineageMatchNodePairType>();
            services.AddSingleton<LineageMatchNodeType>();
            services.AddSingleton<LineageMatchTreeType>();
            services.AddSingleton<LineageTypeEnum>();
            services.AddSingleton<MLSpecType>();
            services.AddSingleton<OutputFormatType>();
            services.AddSingleton<OutputTypeEnum>();
            services.AddSingleton<PostTypeEnum>();
            services.AddSingleton<SourceTypeEnum>();
            services.AddSingleton<StringDoublePairType>();
            services.AddSingleton<StringStringPairType>();
            services.AddSingleton<SetDefinitionType>();
            services.AddSingleton<InputFormatUpdateType>();
            services.AddSingleton<OutputFormatUpdateType>();
            services.AddSingleton<LineageNodeDefinitionType>();
            services.AddSingleton<LineageRecordType>();
            services.AddSingleton<LineageNodeAttributeUpdateType>();
            services.AddSingleton<LineageNodeAttributeType>();
            services.AddSingleton<DarlVarType>();
            services.AddSingleton<DarlVarDataTypeEnum>();
            services.AddSingleton<StringStringPairInputType>();
            services.AddSingleton<DarlVarInputType>();
            services.AddSingleton<MLResultType>();
            services.AddSingleton<LineageAssociationType>();
            services.AddSingleton<LineageElementUnionType>();
            services.AddSingleton<MLSpecUpdateType>();
            services.AddSingleton<SetGraphType>();
            services.AddSingleton<PercentGraphType>();
            services.AddSingleton<QuestionDataType>();
            services.AddSingleton<QuestionTypeEnum>();
            services.AddSingleton<ResponseDataType>();
            services.AddSingleton<ResponseTypeEnum>();
            services.AddSingleton<QuestionSetInputType>();
            services.AddSingleton<QuestionInputType>();
            services.AddSingleton<DarlLintErrorType>();
            services.AddSingleton<StringDarlVarPairType>();
            services.AddSingleton<InteractResponseType>();
            services.AddSingleton<MatchedAnnotationType>();
            services.AddSingleton<LineageNodeAttributeResourceType>();
            services.AddSingleton<DQTypeEnum>();
            services.AddSingleton<ResourceTypeEnum>();
            services.AddSingleton<DaslSetType>();
            services.AddSingleton<DaslStateType>();
            services.AddSingleton<SampleTypeEnum>();
            services.AddSingleton<DaslSetInputType>();
            services.AddSingleton<DaslStateInputType>();
            services.AddSingleton<DarlSubscription>();
            services.AddSingleton<StringDoublePairInputType>();
            services.AddSingleton<ModelTypeEnum>();
            services.AddSingleton<GraphQLCredentialsType>();
            services.AddSingleton<StoreStateType>();
            services.AddSingleton<GraphObjectType>();
            services.AddSingleton<GraphConnectionType>();
            services.AddSingleton<GraphObjectInputType>();
            services.AddSingleton<GraphConnectionInputType>();
            services.AddSingleton<GraphObjectUpdateType>();
            services.AddSingleton<GraphConnectionUpdateType>();
            services.AddSingleton<DarlLicenseType>();
            services.AddSingleton<MatchResultType>();
            services.AddSingleton<InferenceRecordType>();
            services.AddSingleton<OntologyActionEnum>();
            services.AddSingleton<KGTrainingSpecType>();
            services.AddSingleton<KGTrainingValueType>();
            services.AddSingleton<GraphAttributeType>();
            services.AddSingleton<GraphAttributeInputType>();
            services.AddSingleton<GraphConnectionInputType>();
            services.AddSingleton<GraphConnectionType>();
            services.AddSingleton<GraphConnectionUpdateType>();
            services.AddSingleton<GraphObjectInputType>();
            services.AddSingleton<GraphObjectType>();
            services.AddSingleton<GraphObjectUpdateType>();
            services.AddSingleton<GraphAttributeDataTypeEnum>();
            services.AddSingleton<KnowledgeStateType>();
            services.AddSingleton<StringListGraphAttributePairType>();
            services.AddSingleton<KGraphType>();
            services.AddSingleton<GraphModelType>();
            services.AddSingleton<StringGraphObjectPairType>();
            services.AddSingleton<StringGraphConnectionPairType>();
            services.AddSingleton<DisplayConnectionInnerType>();
            services.AddSingleton<DisplayConnectionOuterType>();
            services.AddSingleton<DisplayModelType>();
            services.AddSingleton<DisplayObjectInnerType>();
            services.AddSingleton<DisplayObjectOuterType>();
            services.AddSingleton<GraphTypeEnum>();
            services.AddSingleton<DarlTimeType>();
            services.AddSingleton<DarlTimeInputType>();
            services.AddSingleton<DarlSeasonEnum>();
            services.AddSingleton<InferenceTimeEnum>();
            services.AddSingleton<DateDisplayEnum>();
            services.AddSingleton<VRDisplayModelType>();
            services.AddSingleton<VRDisplayNodeType>();
            services.AddSingleton<VRDisplayLinkType>();
            services.AddSingleton<KnowledgeStateInputType>();
            services.AddSingleton<StringListGraphAttributeInputPairInputType>();
            services.AddSingleton<ModelMetaDataType>();
            services.AddSingleton<ModelMetaDataUpdateType>();
 
            //root
            services.AddSingleton<DarlMutation>();
            services.AddSingleton<DarlQuery>();
            services.AddSingleton<DarlSchema>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddSingleton<IConnectivity, LocalConnectivity>();
            services.AddSingleton<IBlobConnectivity, OneFileConnectivity>();
            services.AddSingleton<IKGTranslation, KGContainer>();

            services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
            services.AddTransient(typeof(IGraphQLExecuter<>), typeof(DefaultGraphQLExecuter<>));



            services.AddRazorPages();
            services.AddHealthChecks();
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");
            services.AddOptions();
            LineageLibrary.LookupWord("wake up");

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseRouting();


            app.Use(async (context, next) =>
            {
                var identity = new GenericIdentity("893C1F9A-0419-4C91-8F34-52E779C316FD");
                identity.AddClaims(context.User.Claims);
                var roles = "Corp,User";
                context.User = new GenericPrincipal(identity, string.IsNullOrEmpty(roles) ? Array.Empty<string>() : roles.Split(','));
                await next.Invoke();
            });

            app.UseMiddleware<GraphQLHttpMiddleware<DarlSchema>>(new PathString("/graphql"));

            app.UseGraphQLPlayground(new GraphQLPlaygroundOptions()
            {
                Path = "/ui/playground",
            });

            app.UseGraphiQLServer(new GraphiQLOptions
            {
                GraphiQLPath = "/ui/graphiql",
                GraphQLEndPoint = "/graphql"
            });

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapHealthChecks("/health");
                endpoints.MapRazorPages();
            });
        }
    }
}
