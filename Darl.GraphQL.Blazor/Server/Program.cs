
using GraphQL;
using GraphQL.SystemTextJson;
using Darl.GraphQL.Blazor.Server.Helpers;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Options;
using GraphQL.Server.Ui.GraphiQL;
using Darl.Licensing;
using Darl.Lineage.Bot;
using Darl.Thinkbase;
using DarlLanguage.Processing;
using Darl.GraphQL.Process.Blazor.Connectivity;
using Darl.GraphQL.Process.Blazor.Schemata;
using Darl.GraphQL.Process.Blazor.Models;
using MongoDB.Bson;
using System.Data;
using System.Security.Claims;
using System.Security.Principal;

namespace Darl.GraphQL.Blazor
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthentication();

            builder.Services.AddGraphQL(b => b
                .AddSchema<DarlSchema>()
                .UseApolloTracing(true)
                .AddErrorInfoProvider((opts, serviceProvider) =>
                {
                    var settings = serviceProvider.GetRequiredService<IOptions<GraphQLSettings>>();
                    opts.ExposeExceptionDetails = settings.Value.ExposeExceptions;
                })
                .AddGraphTypes(typeof(DarlSchema).Assembly)
                .AddGraphTypes(typeof(KGraphType).Assembly)
                .AddSystemTextJson()
                .AddValidationRule<LicenseValidationRule>()
                .AddUserContextBuilder(httpContext => new GraphQLUserContext(httpContext.User))
                .UseMemoryCache(options =>
                {
                    // maximum total cached query length of 1,000,000 bytes (assume 10x memory usage
                    // for 10MB maximum memory use by the cache - parsed AST and other stuff)
                    options.SizeLimit = 1000000;
                    // no expiration of cached queries (cached queries are only ejected when the cache is full)
                    options.SlidingExpiration = null;
                })
                ); 

            builder.Services.Configure<GraphQLSettings>(builder.Configuration.GetSection("GraphQLSettings"));
            builder.Services.AddLogging(builder => builder.AddConsole());
            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllersWithViews().AddJsonOptions(opts =>
            {
                opts.JsonSerializerOptions.Converters.Add(new InputsJsonConverter());
            });

            builder.Services.AddRazorPages();
            builder.Services.AddSingleton<IConnectivity, CosmosDBConnectivity>();
            builder.Services.AddSingleton<IBlobConnectivity, BlobGraphConnectivity>();
            builder.Services.AddSingleton<IKGTranslation, KGTranslation>();
            builder.Services.AddSingleton<IBotProcessing, BotProcessing>();
            builder.Services.AddSingleton<IGraphProcessing, GraphProcessing>();
            builder.Services.AddSingleton<ISoftMatchProcessing, SoftMatchProcessing>();
            builder.Services.AddSingleton<ILocalStore, GraphLocalStore>();
            builder.Services.AddSingleton<IGraphPrimitives, BlobGraphPrimitives>();
            builder.Services.AddSingleton<IGraphHandler, GraphHandler>();
            builder.Services.AddSingleton<IMetaStructureHandler, MetaStructureHandler>();
            builder.Services.AddSingleton<IBotStateStorage, BotStateStorage>();
            builder.Services.AddSingleton<ILicensing, ProductLicensing>();
            builder.Services.AddSingleton<Thinkbase.IDataLoader, DataLoader>();


            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseWebAssemblyDebugging();
            }
            else
            {
                app.UseExceptionHandler("/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseWebSockets();

            app.UseHttpsRedirection();

            app.UseBlazorFrameworkFiles();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();

            app.Use(async (context, next) =>
            {
            var authHeader = context.Request.Headers["Authorization"].ToString();
                if (authHeader != null && authHeader.StartsWith("Basic", StringComparison.OrdinalIgnoreCase))
                {
                    var token = authHeader.Substring("Basic ".Length).Trim(); //token is <UserId>/<tenantId>.
                    var userId = token.Substring(0,36);
                    var tenantId = token.Substring(36);
                    if (!string.IsNullOrEmpty(token))
                    {
                        var identity = new GenericIdentity(userId);
                        identity.AddClaims(context.User.Claims);
                        identity.AddClaim(new Claim(KGTranslation.objectIdClaimText, userId));
                        identity.AddClaim(new Claim(KGTranslation.tenantIdClaimText, tenantId));
                        context.User = new GenericPrincipal(identity, null);
                    }
                }
                await next.Invoke();
            });
                app.MapControllers();

            app.UseGraphQLGraphiQL("/graphiql", new GraphiQLOptions { GraphQLEndPoint = "/graphql" });

            app.MapRazorPages();
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}