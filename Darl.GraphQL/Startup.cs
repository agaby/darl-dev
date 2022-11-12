using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Middleware;
using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Schemata;
using Darl.GraphQL.Process.Connectivity;
using Darl.GraphQL.Process.Middleware;
using Darl.GraphQL.Process.Web.Bot;
using Darl.GraphQL.Process.Web.Middleware;
using Darl.GraphQL.Web.Models.Schemata;
using Darl.Licensing;
using Darl.Lineage;
using Darl.Lineage.Bot;
using Darl.Thinkbase;
using DarlLanguage.Processing;
using GraphQL;
using GraphQL.Caching;
using GraphQL.MicrosoftDI;
using GraphQL.Server;
using GraphQL.Server.Transports.Subscriptions.Abstractions;
using GraphQL.Server.Ui.GraphiQL;
using GraphQL.Server.Ui.Playground;
using GraphQL.Server.Ui.Voyager;
using GraphQL.SystemTextJson;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Bot.Builder;
using Microsoft.Bot.Builder.Integration.AspNet.Core;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using System;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Darl.GraphQL
{
    public class Startup
    {

        static readonly string emailClaimText = @"emails";
        static readonly string objectIdClaimText = @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";
        static readonly string firstNameClaimText = @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/givenname";
        static readonly string secondNameClaimText = @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/surname";
        static readonly string roleClaimText = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            Environment = environment;
        }

        public IConfiguration Configuration { get; }

        public IWebHostEnvironment Environment { get; }


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



            services.AddMicrosoftIdentityWebAppAuthentication(Configuration, "AzureAdB2C");

            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(60);
            });

            services.AddMemoryCache();
#if DEBUG
            services.AddDistributedMemoryCache();
#else
            services.AddDistributedRedisCache(option =>
            {
                option.Configuration = Configuration.GetSection("AppSettings")["RedisConnection"];
                option.InstanceName = "darlai";
            });
#endif
            services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromSeconds(3600);
                options.Cookie.HttpOnly = true;
            });


            //services
            services.AddSingleton<IConnectivity, CosmosDBConnectivity>();
            services.AddSingleton<IBlobConnectivity, BlobGraphConnectivity>();
            services.AddSingleton<IKGTranslation, KGTranslation>();
            services.AddSingleton<IBotProcessing, BotProcessing>();
            services.AddSingleton<IEmailProcessing, EmailProcessing>();
            services.AddSingleton<IGraphProcessing, GraphProcessing>();
            services.AddSingleton<ILicensing, ProductLicensing>();
            services.AddSingleton<ISoftMatchProcessing, SoftMatchProcessing>();
            services.AddSingleton<ILocalStore, GraphLocalStore>();
            services.AddSingleton<IGraphPrimitives, BlobGraphPrimitives>();
            services.AddSingleton<IGraphHandler, GraphHandler>();
            services.AddSingleton<IMetaStructureHandler, MetaStructureHandler>();
            services.AddSingleton<IProducts, Products>();
            services.AddSingleton<ICheckEmail, EmailChecker>();
            services.AddSingleton<Thinkbase.IDataLoader, DataLoader>();
            services.AddSingleton<BotFrameworkAuthentication, ConfigurationBotFrameworkAuthentication>();
            services.AddSingleton<IBotFrameworkHttpAdapter, AdapterWithErrorHandler>();
            services.AddSingleton<IBotStateStorage, BotStateStorage>();
            services.AddSingleton<AdminFilter>();
            services.AddTransient<IBot, DarlBot>();
            services.AddHttpContextAccessor();
            services.AddTransient<IOperationMessageListener, AuthenticationListener>();


            services.AddGraphQL(builder => builder
                .AddSystemTextJson()
                .AddHttpMiddleware<DarlSchema>()
                .AddWebSocketsHttpMiddleware<DarlSchema>()
                .AddSchema<DarlSchema>()
                .AddWebSockets()
                .AddUserContextBuilder(context => new GraphQLUserContext { User = context.User })
                .AddGraphTypes(typeof(DarlSchema).Assembly)
                .AddGraphTypes(typeof(KGraphType).Assembly)
                .AddGraphQLAuthorization(options =>
                {
                    options.AddPolicy("AdminPolicy", p => p.RequireClaim(roleClaimText, "Admin"));
                    options.AddPolicy("UserPolicy", p => p.RequireClaim(roleClaimText, "User"));
                    options.AddPolicy("CorpPolicy", p => p.RequireClaim(roleClaimText, "Corp"));
                })
            );


            services.AddControllersWithViews()
                .AddMicrosoftIdentityUI();


            services.AddRazorPages();
            services.AddApplicationInsightsTelemetry();
            services.AddHealthChecks();
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

            //Configuring appsettings section AzureAdB2C, into IOptions
            services.AddOptions();
            services.Configure<OpenIdConnectOptions>(Configuration.GetSection("AzureAdB2C"));
            LineageLibrary.LookupWord("wake up");
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

                DarlUser? du = null;
                String roles = string.Empty;
                string objectId = string.Empty;
                string clientSecret = string.Empty;
                var _rep = (IKGTranslation?)context.RequestServices.GetService(typeof(IKGTranslation));
                if (_rep != null && context.User.Identity != null && context.User.Identity.IsAuthenticated)
                {
                    //look up user
                    objectId = context.User.Claims.Where(ai => ai.Type == objectIdClaimText).Single().Value;
                    du = await _rep.GetUserById(objectId);
                    if (du == null) //new user
                    {
                        du = await AddNewUser(context, objectId, _rep);
                        if (du == null) //can't setup user
                        {
                            await next.Invoke();
                            return;
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(du.parentAccount)) //this is a sub user, log in as the parent.
                        {
                            du = await _rep.GetUserById(du.parentAccount);
                        }
                    }
                }
                else if (_rep != null)
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
                    }
                }
                if (du != null)//found it from one or other
                {
                    roles = du.accountState switch
                    {
                        DarlUser.AccountState.admin => "Admin,Corp,User",
                        DarlUser.AccountState.trial or DarlUser.AccountState.paying or DarlUser.AccountState.delinquent => "User",
                        _ => string.Empty,
                    };
                    //overwrite user 
                    var identity = new GenericIdentity(objectId);
                    identity.AddClaims(context.User.Claims);
                    identity.AddClaim(new Claim("apikey", du.APIKey));
                    foreach (var c in roles.Split(','))
                        identity.AddClaim(new Claim(roleClaimText, c.ToString()));
                    context.User = new GenericPrincipal(identity, string.IsNullOrEmpty(roles) ? Array.Empty<string>() : roles.Split(','));
                }
                await next.Invoke();
            });



            app.UseWebSockets();

            app.UseGraphQLWebSockets<DarlSchema>();
            app.UseGraphQL<DarlSchema>();

            app.UseGraphQLPlayground(new PlaygroundOptions()
            {
                BetaUpdates = true,
                RequestCredentials = RequestCredentials.Omit,
                HideTracingResponse = false,

                EditorCursorShape = EditorCursorShape.Line,
                EditorTheme = EditorTheme.Light,
                EditorFontSize = 14,
                EditorReuseHeaders = true,
                EditorFontFamily = "Consolas",

                PrettierPrintWidth = 80,
                PrettierTabWidth = 2,
                PrettierUseTabs = true,

                SchemaDisableComments = false,
                SchemaPollingEnabled = true,
                SchemaPollingEndpointFilter = "*localhost*",
                SchemaPollingInterval = 5000,
            });
            app.UseGraphQLGraphiQL(new GraphiQLOptions
            {
                GraphQLEndPoint = "/graphql"
            });
            app.UseGraphQLVoyager(new VoyagerOptions()
            {
                GraphQLEndPoint = "/graphql",
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

        private async Task<DarlUser?> AddNewUser(HttpContext context, string objectId, IKGTranslation _rep)
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
                var priceId = context.Request.Cookies["priceId"];
                return await _rep.CreateAndRegisterNewUser(new DarlUserInput { userId = objectId, InvoiceEmail = emailClaim, Issuer = provider, InvoiceName = invoiceName, InvoiceOrganization = "", productId = priceId });
            }
            catch
            {
                return null;
            }
        }
    }
}
