using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Schemata;
using GraphQL;
using GraphQL.Server.Authorization.AspNetCore;
using GraphQL.Server;
using GraphQL.Server.Ui.GraphiQL;
using GraphQL.Server.Ui.Playground;
using GraphQL.Server.Ui.Voyager;
using Microsoft.ApplicationInsights;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.AzureADB2C.UI;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using Darl.Lineage.Bot.Stores;
using Darl.Lineage.Bot;

namespace Darl.GraphQL
{
    public class Startup
    {

        static readonly string emailClaimText = @"emails";
        static readonly string objectIdClaimText = @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        static readonly string firstNameClaimText = @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
        static readonly string secondNameClaimText = @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {

            Environment = environment;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Environment { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });

            services.AddAuthentication(AzureADB2CDefaults.AuthenticationScheme)
                .AddAzureADB2C(options => Configuration.Bind("AzureAdB2C", options));

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


            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));
            //services
            services.AddSingleton<IConnectivity, CosmosDBConnectivity>();
            services.AddSingleton<IFormApi, FormApi>();
            services.AddSingleton<IBotProcessing, BotProcessing>();
            services.AddSingleton<IRuleFormInterface, RuleFormInterface>();
            services.AddSingleton<ITrigger, BotTrigger>();
            services.AddSingleton<IFormProcessing, FormProcessing>();
            services.AddSingleton<IEmailProcessing, EmailProcessing>();

            //types
            services.AddTransient<BotFormatType>();
            services.AddTransient<BotInputFormatType>();
            services.AddTransient<BotModelType>();
            services.AddTransient<BotOutputFormatType>();
            services.AddTransient<DictionarySequenceType>();
            services.AddTransient<DisplayTypeEnum>();
            services.AddTransient<FormFormatType>();
            services.AddTransient<InputFormatType>();
            services.AddTransient<InputTypeEnum>();
            services.AddTransient<LanguageFormatType>();
            services.AddTransient<LanguageTextType>();
            services.AddTransient<LineageAnnotationNodeType>();
            services.AddTransient<LineageElementType>();
            services.AddTransient<LineageMatchNodePairType>();
            services.AddTransient<LineageMatchNodeType>();
            services.AddTransient<LineageMatchTreeType>();
            services.AddTransient<LineageModelType>();
            services.AddTransient<LineageTypeEnum>();
            services.AddTransient<MLModelType>();
            services.AddTransient<MLSpecType>();
            services.AddTransient<OutputFormatType>();
            services.AddTransient<OutputTypeEnum>();
            services.AddTransient<PostTypeEnum>();
            services.AddTransient<RuleFormType>();
            services.AddTransient<RuleSetType>();
            services.AddTransient<SourceTypeEnum>();
            services.AddTransient<StringDoublePairType>();
            services.AddTransient<StringStringPairType>();
            services.AddTransient<TriggerViewType>();
            services.AddTransient<VariantTextType>();
            services.AddTransient<SetDefinitionType>();
            services.AddTransient<ServiceConnectivityType>();
            services.AddTransient<AzureCredentialsType>();
            services.AddTransient<SellerCenterCredentialsType>();
            services.AddTransient<ZendeskCredentialsType>();
            services.AddTransient<SendGridCredentialsType>();
            services.AddTransient<TwilioCredentialsType>();
            services.AddTransient<BotConnectionType>();
            services.AddTransient<UserUsageType>();
            services.AddTransient<ContactType>();
            services.AddTransient<DefaultType>();
            services.AddTransient<ContactInputType>();
            services.AddTransient<ContactUpdateType>();
            services.AddTransient<InputFormatUpdateType>();
            services.AddTransient<OutputFormatUpdateType>();
            services.AddTransient<BotOutputFormatUpdateType>();
            services.AddTransient<LineageNodeDefinitionType>();
            services.AddTransient<LineageRecordType>();
            services.AddTransient<LineageNodeAttributeUpdateType>();
            services.AddTransient<LineageNodeAttributeType>();
            services.AddTransient<DarlVarType>();
            services.AddTransient<DarlVarDataTypeEnum>();
            services.AddTransient<StringStringPairInputType>();
            services.AddTransient<DarlVarInputType>();
            services.AddTransient<MLResultType>();
            services.AddTransient<AuthorizationType>();
            services.AddTransient<LineageAssociationType>();
            services.AddTransient<LineageElementUnionType>();
            services.AddTransient<AuthorizationUpdateType>();
            services.AddTransient<DarlUserType>();
            services.AddTransient<AccountStateEnum>();
            services.AddTransient<DarlUserInputType>();
            services.AddTransient<DarlUserUpdateType>();
            services.AddTransient<MLSpecUpdateType>();
            services.AddTransient<SetGraphType>();
            services.AddTransient<PercentGraphType>();
            services.AddTransient<QuestionSetType>();
            services.AddTransient<QuestionDataType>();
            services.AddTransient<QuestionTypeEnum>();
            services.AddTransient<ResponseDataType>();
            services.AddTransient<ResponseTypeEnum>();
            services.AddTransient<QuestionSetInputType>();
            services.AddTransient<QuestionInputType>();
            services.AddTransient<DarlLintErrorType>();
            services.AddTransient<StringDarlVarPairType>();
            services.AddTransient<InteractResponseType>();
            services.AddTransient<MatchedAnnotationType>();
            services.AddTransient<LineageNodeAttributeResourceType>();
            services.AddTransient<CollateralType>();
            services.AddTransient<UpdateType>();
            services.AddTransient<ConversationType>();
            services.AddTransient<ConversationInputType>();
            services.AddTransient<BotRuntimeModelType>();
            services.AddTransient<DocumentType>();
            services.AddTransient<TriggerViewInputType>();
            services.AddTransient<DQTypeEnum>();
            services.AddTransient<ResourceTypeEnum>();
            services.AddTransient<PurchaseType>();


            //root
            services.AddTransient<DarlSchema>();
            services.AddTransient<DarlMutation>();
            services.AddTransient<DarlQuery>();


            services.AddSingleton<IDependencyResolver>(
                c => new FuncDependencyResolver(type => c.GetRequiredService(type)));
            services.AddGraphQL(options => {
                options.EnableMetrics = true;
                options.ExposeExceptions = Environment.IsDevelopment();
            })
            .AddGraphQLAuthorization(options =>
            {
                options.AddPolicy("AdminPolicy", policy =>
                   policy.RequireRole("Admin"));
                options.AddPolicy("UserPolicy", policy =>
                    policy.RequireRole("User"));                          
            })
            .AddGraphTypes()
            .AddDataLoader()
            .AddUserContextBuilder(ctx => new GraphQLUserContext
            {
                User = ctx.User
            });

            services.AddRazorPages();

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
                            await next.Invoke();
                            return;
                        }
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
                        objectId = du.userId;
                    }

                }
                if (du != null)//found it from one or other
                {
                    switch (du.accountState)
                    {
                        case DarlUser.AccountState.admin:
                            roles = "Admin,User";
                            break;
                        case DarlUser.AccountState.trial:
                        case DarlUser.AccountState.paying:
                        case DarlUser.AccountState.delinquent:
                            roles = "User";
                            break;
                        default:
                            roles = string.Empty;
                            break;
                    }
                    //overwrite user 
                    var identity = new GenericIdentity(objectId);
                    identity.AddClaims(context.User.Claims);
                    context.User = new GenericPrincipal(identity, string.IsNullOrEmpty(roles) ? new string[0] : roles.Split(','));
                }
                await next.Invoke();
            });


            app.UseGraphQL<DarlSchema>("/graphql");
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
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

        }

        private async Task<DarlUser> AddNewUser(HttpContext context, string objectId, IConnectivity _rep)
        {
            var tc = new TelemetryClient();
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
                tc.TrackEvent("New registration", new Dictionary<string, string> { { "UserId", objectId }, { "provider", provider }, { "email", emailClaim } });
                return await _rep.CreateAndProvisionNewUser(new DarlUserInput {userId = objectId, InvoiceEmail = emailClaim, Issuer = provider, InvoiceName = invoiceName, InvoiceOrganization = "" });
            }
            catch(Exception ex)
            {
                tc.TrackException(ex, new Dictionary<string, string> { { "UserId", objectId } });
                return null;
            }
        }
    }
}
