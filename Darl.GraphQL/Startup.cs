using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Middleware;
using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Schemata;
using Darl.GraphQL.Process.Middleware;
using Darl.GraphQL.Ui.GraphiQL;
using Darl.GraphQL.Ui.Playground;
using Darl.GraphQL.Ui.Voyager;
using Darl.Lineage.Bot;
using Darl.Lineage.Bot.Stores;
using Darl.Thinkbase;
using DarlLanguage.Processing;
using GraphQL;
using GraphQL.Http;
using GraphQL.Validation;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Security.Principal;
using System.Threading.Tasks;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;

namespace Darl.GraphQL
{
    public class Startup
    {

        static readonly string emailClaimText = @"emails";
        static readonly string objectIdClaimText = @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        static readonly string firstNameClaimText = @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
        static readonly string secondNameClaimText = @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";

        private readonly ILogger _logger;

        public Startup(IConfiguration configuration, ILogger<Startup> logger)
        {
            _logger = logger;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
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

            services.AddMicrosoftWebAppAuthentication(Configuration, "AzureAdB2C");

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(60);
            });

            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = Configuration.GetSection("AppSettings")["RedisConnection"];
                option.InstanceName = "darlai";
            });

            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(3600);
                options.Cookie.HttpOnly = true;
            });


            //services
            services.AddSingleton<IDocumentExecuter, DocumentExecuter>();
            services.AddSingleton<IDocumentWriter, DocumentWriter>();
            services.AddSingleton<IConnectivity, CosmosDBConnectivity>();
            services.AddSingleton<IFormApi, FormApi>();
            services.AddSingleton<IBotProcessing, BotProcessing>();
            services.AddSingleton<IRuleFormInterface, RuleFormInterface>();
            services.AddSingleton<ITrigger, BotTrigger>();
            services.AddSingleton<IFormProcessing, FormProcessing>();
            services.AddSingleton<IEmailProcessing, EmailProcessing>();
            services.AddSingleton<ISimProcessing, SimProcessing>();
            services.AddSingleton<IAuthChecker, AuthChecker>();
            services.AddSingleton<IGraphProcessing, GraphProcessing>();
            services.AddSingleton<ILicensing, ProductLicensing>();
            services.AddSingleton<IBlobConnectivity, BlobConnectivity>();
            services.AddSingleton<ISoftMatchProcessing, SoftMatchProcessing>();
            services.AddSingleton<ILocalStore, GraphLocalStore>();
            services.AddSingleton<IGraphPrimitives, BlobGraphPrimitives>();
            services.AddSingleton<IGraphHandler, GraphHandler>();

            //types
            services.AddSingleton<BotFormatType>();
            services.AddSingleton<BotInputFormatType>();
            services.AddSingleton<BotModelType>();
            services.AddSingleton<BotOutputFormatType>();
            services.AddSingleton<DictionarySequenceType>();
            services.AddSingleton<DisplayTypeEnum>();
            services.AddSingleton<FormFormatType>();
            services.AddSingleton<InputFormatType>();
            services.AddSingleton<InputTypeEnum>();
            services.AddSingleton<LanguageFormatType>();
            services.AddSingleton<LanguageTextType>();
            services.AddSingleton<LineageAnnotationNodeType>();
            services.AddSingleton<LineageElementType>();
            services.AddSingleton<LineageMatchNodePairType>();
            services.AddSingleton<LineageMatchNodeType>();
            services.AddSingleton<LineageMatchTreeType>();
            services.AddSingleton<LineageModelType>();
            services.AddSingleton<LineageTypeEnum>();
            services.AddSingleton<MLModelType>();
            services.AddSingleton<MLSpecType>();
            services.AddSingleton<OutputFormatType>();
            services.AddSingleton<OutputTypeEnum>();
            services.AddSingleton<PostTypeEnum>();
            services.AddSingleton<RuleFormType>();
            services.AddSingleton<RuleSetType>();
            services.AddSingleton<SourceTypeEnum>();
            services.AddSingleton<StringDoublePairType>();
            services.AddSingleton<StringStringPairType>();
            services.AddSingleton<TriggerViewType>();
            services.AddSingleton<VariantTextType>();
            services.AddSingleton<SetDefinitionType>();
            services.AddSingleton<ServiceConnectivityType>();
            services.AddSingleton<AzureCredentialsType>();
            services.AddSingleton<SellerCenterCredentialsType>();
            services.AddSingleton<ZendeskCredentialsType>();
            services.AddSingleton<SendGridCredentialsType>();
            services.AddSingleton<TwilioCredentialsType>();
            services.AddSingleton<UserUsageType>();
            services.AddSingleton<ContactType>();
            services.AddSingleton<DefaultType>();
            services.AddSingleton<ContactInputType>();
            services.AddSingleton<ContactUpdateType>();
            services.AddSingleton<InputFormatUpdateType>();
            services.AddSingleton<OutputFormatUpdateType>();
            services.AddSingleton<BotOutputFormatUpdateType>();
            services.AddSingleton<LineageNodeDefinitionType>();
            services.AddSingleton<LineageRecordType>();
            services.AddSingleton<LineageNodeAttributeUpdateType>();
            services.AddSingleton<LineageNodeAttributeType>();
            services.AddSingleton<DarlVarType>();
            services.AddSingleton<DarlVarDataTypeEnum>();
            services.AddSingleton<StringStringPairInputType>();
            services.AddSingleton<DarlVarInputType>();
            services.AddSingleton<MLResultType>();
            services.AddSingleton<AuthorizationType>();
            services.AddSingleton<LineageAssociationType>();
            services.AddSingleton<LineageElementUnionType>();
            services.AddSingleton<AuthorizationUpdateType>();
            services.AddSingleton<DarlUserType>();
            services.AddSingleton<AccountStateEnum>();
            services.AddSingleton<DarlUserInputType>();
            services.AddSingleton<DarlUserUpdateType>();
            services.AddSingleton<MLSpecUpdateType>();
            services.AddSingleton<SetGraphType>();
            services.AddSingleton<PercentGraphType>();
            services.AddSingleton<QuestionSetType>();
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
            services.AddSingleton<CollateralType>();
            services.AddSingleton<UpdateType>();
            services.AddSingleton<ConversationType>();
            services.AddSingleton<ConversationInputType>();
            services.AddSingleton<BotRuntimeModelType>();
            services.AddSingleton<DocumentType>();
            services.AddSingleton<TriggerViewInputType>();
            services.AddSingleton<DQTypeEnum>();
            services.AddSingleton<ResourceTypeEnum>();
            services.AddSingleton<PurchaseType>();
            services.AddSingleton<DaslSetType>();
            services.AddSingleton<DaslStateType>();
            services.AddSingleton<SampleTypeEnum>();
            services.AddSingleton<AdminFilter>();
            services.AddSingleton<DaslSetInputType>();
            services.AddSingleton<DaslStateInputType>();
            services.AddSingleton<DarlSubscription>();
            services.AddSingleton<StringDoublePairInputType>();
            services.AddSingleton<ModelDetailsType>();
            services.AddSingleton<ModelDetailsInputType>();
            services.AddSingleton<ModelTypeEnum>();
            services.AddSingleton<GraphQLCredentialsType>();
            services.AddSingleton<InteractionModelType>();
            services.AddSingleton<LanguageModelType>();
            services.AddSingleton<IntentType>();
            services.AddSingleton<SlotType>();
            services.AddSingleton<AlexaTypeType>();
            services.AddSingleton<StoreStateType>();
            services.AddSingleton<BotTestViewType>();
            services.AddSingleton<GraphObjectType>();
            services.AddSingleton<GraphConnectionType>();
            services.AddSingleton<GraphObjectInputType>();
            services.AddSingleton<GraphConnectionInputType>();
            services.AddSingleton<GraphObjectUpdateType>();
            services.AddSingleton<GraphConnectionUpdateType>();
            services.AddSingleton<SubscriptionTypeEnum>();
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



            //root
            services.AddSingleton<DarlMutation>();
            services.AddSingleton<DarlQuery>();
            services.AddSingleton<DarlSchema>();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton<IValidationRule, AuthorizationValidationRule>()
                .AddAuthorization(options =>
                {
                    options.AddPolicy("AdminPolicy", policy =>
                       policy.RequireRole("Admin"));
                    options.AddPolicy("UserPolicy", policy =>
                        policy.RequireRole("User"));
                    options.AddPolicy("CorpPolicy", policy =>
                       policy.RequireRole("Corp"));
                });

            services.AddSingleton<IUserContextBuilder>(new UserContextBuilder<GraphQLUserContext>(ctx => new GraphQLUserContext{ User = ctx.User}));

            services.TryAddSingleton<IDocumentExecuter, DocumentExecuter>();
            services.TryAddTransient(typeof(IGraphQLExecuter<>), typeof(DefaultGraphQLExecuter<>));
//            services.AddSingleton(p => Options.Create(options(p)));

            services.TryAddSingleton<IDocumentWriter>(x =>
            {
                return new DocumentWriter(Formatting.None, new JsonSerializerSettings());
            });

            services.AddControllersWithViews()
                .AddMicrosoftIdentityUI();


            services.AddRazorPages();
            services.AddApplicationInsightsTelemetry();
            services.AddHealthChecks();
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

            //Configuring appsettings section AzureAdB2C, into IOptions
            services.AddOptions();
            services.Configure<OpenIdConnectOptions>(Configuration.GetSection("AzureAdB2C"));

        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseDefaultFiles();
            app.UseStaticFiles();
            app.UseRouting();

            app.UseAuthentication();

            app.Use(async (context, next) =>
            {

                DarlUser du = null;
                String roles = string.Empty;
                string objectId = string.Empty;
                var _rep = (IConnectivity)context.RequestServices.GetService(typeof(IConnectivity));
                if (context.User.Identity.IsAuthenticated)
                {
                    //look up user
                    objectId = context.User.Claims.Where(ai => ai.Type == objectIdClaimText).Single().Value;
                    du = await _rep.GetUserById(objectId);
                    if(du == null) //new user
                    {
                        du = await AddNewUser(context, objectId, _rep);
                        if(du == null) //can't setup user
                        {
                            _logger.LogError($"Can't setup user {context.User.Identity.Name}");
                            await next.Invoke();
                            return;
                        }
                    }
                    else
                    {
                        _logger.LogInformation($"logged in user {du.InvoiceEmail}");
                    }
                }
                else
                {
                    //look for header
                    var authHeader = context.Request.Headers["Authorization"].ToString();
                    if (authHeader != null && authHeader.StartsWith("Basic", StringComparison.OrdinalIgnoreCase))
                    {
                        var token = authHeader.Substring("Basic ".Length).Trim();
                        du = await _rep.GetUserByApiKey(token);
                        if (du == null)// can indicate user is barred
                        {
                            await next.Invoke();
                            return;
                        }
                        objectId = du.userId;
                        _logger.LogInformation($"User {du.InvoiceEmail} logged in via API key");
                    }
                }
                if (du != null)//found it from one or other
                {
                    switch (du.accountState)
                    {
                        case DarlUser.AccountState.admin:
                            roles = "Admin,Corp,User";
                            break;
                        case DarlUser.AccountState.trial:
                        case DarlUser.AccountState.paying:
                        case DarlUser.AccountState.delinquent:
                            switch(du.subscriptionType ?? DarlUser.SubscriptionType.individual)
                            {
                                case DarlUser.SubscriptionType.inhouse:
                                case DarlUser.SubscriptionType.embedded:
                                case DarlUser.SubscriptionType.corporate:
                                    roles = "Corp,User";
                                    break;
                                case DarlUser.SubscriptionType.individual:
                                    roles = "User";
                                    break;
                            }
                            break;
                        default:
                            roles = string.Empty;
                            break;
                    }
                    //overwrite user 
                    var identity = new GenericIdentity(objectId);
                    identity.AddClaims(context.User.Claims);
                    context.User = new GenericPrincipal(identity, string.IsNullOrEmpty(roles) ? Array.Empty<string>() : roles.Split(','));
                }
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
            app.UseGraphQLVoyager(new GraphQLVoyagerOptions()
            {
                GraphQLEndPoint = "/graphql",
                Path = "/ui/voyager"
            });

            app.UseEndpoints(endpoints =>
            {

                endpoints.MapHealthChecks("/health");
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

        }

        private async Task<DarlUser> AddNewUser(HttpContext context, string objectId, IConnectivity _rep)
        {
            try
            { 
                //extract claims that may be present
                var firstNameClaim = context.User.Claims.Where(ai => ai.Type == firstNameClaimText).FirstOrDefault();
                var firstName = firstNameClaim == null ? string.Empty : firstNameClaim.Value;
                var secondNameClaim = context.User.Claims.Where(ai => ai.Type == secondNameClaimText).FirstOrDefault();
                var secondName = secondNameClaim == null ? string.Empty : secondNameClaim.Value;
                var emailList = context.User.Claims.Where(ai => ai.Type == emailClaimText).Single().Value;
                if (string.IsNullOrEmpty(emailList))
                {
                    return null;
                }
                var emailClaim = emailList.Split(',')[0];
                //build names as best we can
                var invoiceName = "";
                if (string.IsNullOrEmpty(firstName) && string.IsNullOrEmpty(secondName))
                {
                    invoiceName = emailClaim;
                }
                else
                {
                    invoiceName = $"{firstName} {secondName}";
                }
                var provider = "aadb2c";
                return await _rep.CreateAndProvisionNewUser(new DarlUserInput {userId = objectId, InvoiceEmail = emailClaim, Issuer = provider, InvoiceName = invoiceName, InvoiceOrganization = "" });
            }
            catch
            {
                return null;
            }
        }
    }
}
