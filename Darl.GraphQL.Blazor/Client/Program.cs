using Darl.GraphQL.Blazor.Client;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Fast.Components.FluentUI;
using Microsoft.Fast.Components.FluentUI.Infrastructure;
using ThinkBase.ComponentLibrary.Interfaces;
using ThinkBase.GraphQLLibrary;

namespace Darl.GraphQL.Blazor.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");
            builder.RootComponents.Add<HeadOutlet>("head::after");

            builder.Services.AddHttpClient("Darl.GraphQL.Blazor.ServerAPI", client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress))
                .AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("Darl.GraphQL.Blazor.ServerAPI"));

            builder.Services.AddMsalAuthentication(options =>
            {
                builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
                options.ProviderOptions.DefaultAccessTokenScopes.Add(builder.Configuration.GetSection("ServerApi")["Scopes"]);
            });
            builder.Services.AddScoped<IClientConnectivity, LocalConnectivity>();
            builder.Services.AddThinkBaseGraphQL(builder.HostEnvironment.BaseAddress + "graphql");
            builder.Services.AddFluentUIComponents();
            builder.Services.AddOptions();
            var baseUrl = builder.Configuration.GetSection("MicrosoftGraph")["BaseUrl"];
            var scopes = builder.Configuration.GetSection("MicrosoftGraph:Scopes")
                .Get<List<string>>();
            builder.Services.AddScoped<IAuthorizationHandler, AppSourcePolicyHandler>();
            builder.Services.AddGraphClient(baseUrl, scopes);
            builder.Services.AddAuthorizationCore(options =>
            {
                options.AddPolicy("appSource", policy => policy.Requirements.Add(new AppSourceRequirement(builder.Configuration["SaaSServiceIdentifier"] ?? "")));
            });

            await builder.Build().RunAsync();
        }
    }
}