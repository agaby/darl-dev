
using GraphQL;
using GraphQL.Caching;
using GraphQL.MicrosoftDI;
using GraphQL.SystemTextJson;
using Darl.GraphQL.Blazor.Server.Helpers;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Identity.Web;
using Microsoft.Extensions.Options;
using GraphQL.Server.Ui.Playground;
using GraphQL.Server.Ui.GraphiQL;
using Darl.Licensing;
using Darl.Lineage.Bot;
using Darl.Thinkbase;
using DarlLanguage.Processing;
using GraphQL.Server;
using Darl.GraphQL.Process.Blazor.Connectivity;
using Darl.GraphQL.Process.Blazor.Schemata;
using ThinkBase.Teams.Connectivity;
using Darl.GraphQL.Process.Blazor.Models;

namespace Darl.GraphQL.Blazor
{
    public class Program
    {

        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

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
            builder.Services.AddSingleton<ILicenseConnectivity, LocalLicenseConnectivity>();


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

            app.UseAuthorization();

            app.MapControllers();

            app.UseGraphQLGraphiQL("/graphiql", new GraphiQLOptions { GraphQLEndPoint = "/graphql" });

            app.MapRazorPages();
            app.MapFallbackToFile("index.html");

            app.Run();
        }
    }
}