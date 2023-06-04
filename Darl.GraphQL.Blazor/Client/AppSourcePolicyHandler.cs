using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph.Beta;

namespace Darl.GraphQL.Blazor.Client
{
    public class AppSourcePolicyHandler : AuthorizationHandler<AppSourceRequirement>
    {

        private GraphServiceClient _client;
        private IConfiguration _config;

        public AppSourcePolicyHandler(GraphServiceClient client, IConfiguration config)
        {
            _client  = client;
            _config = config;
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context, AppSourceRequirement requirement)
        {
            if (context.User.Identity != null && context.User.Identity.IsAuthenticated)
            {
                var thisUser = await _client.Me.GetAsync();
                if (thisUser != null)
                {
                    var resp = await _client.Users[thisUser.Id].UsageRights.GetAsync();
                    var usageRight = resp.Value!.FirstOrDefault(a => a.ServiceIdentifier == requirement.ServiceIdentifier);
                    if (usageRight != null && (usageRight.State == Microsoft.Graph.Beta.Models.UsageRightState.Active || usageRight.State == Microsoft.Graph.Beta.Models.UsageRightState.Warning))
                    {
                        context.Succeed(requirement);
                    }
                }
            }
            return;
        }
    }

    public class AppSourceRequirement : IAuthorizationRequirement
    {
        public string ServiceIdentifier { get;}
        public AppSourceRequirement(string serviceIdentifier)
        {
            ServiceIdentifier = serviceIdentifier;
        }
    }
}
