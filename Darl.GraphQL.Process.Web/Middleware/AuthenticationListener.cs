using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using GraphQL.Server.Transports.Subscriptions.Abstractions;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json.Linq;
using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;

namespace Darl.GraphQL.Process.Web.Middleware
{
    public class AuthenticationListener : IOperationMessageListener
    {
        public static readonly string PRINCIPAL_KEY = "User";
        static readonly string roleClaimText = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";


        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IKGTranslation _trans;

        public AuthenticationListener(IHttpContextAccessor contextAccessor, IKGTranslation trans)
        {
            this._httpContextAccessor = contextAccessor;
            _trans = trans;
        }

        private async Task<ClaimsPrincipal?> BuildClaimsPrincipal(string? token)
        {
            if (token == null)
                return null;
            var key = token;
            if (token.Contains("Basic"))
                key = token.Substring("Basic ".Length).Trim();
            var du = await _trans.GetUserByApiKey(key);
            if (du == null)// can indicate user is barred
            {
                return null;
            }

            var objectId = du.userId;
            var roles = du.accountState switch
            {
                DarlUser.AccountState.admin => "Admin,Corp,User",
                DarlUser.AccountState.trial or DarlUser.AccountState.paying or DarlUser.AccountState.delinquent => "User",
                _ => string.Empty,
            };
            //overwrite user 
            var identity = new GenericIdentity(objectId);
            identity.AddClaim(new Claim("apikey", du.APIKey));
            foreach (var c in roles.Split(','))
                identity.AddClaim(new Claim(roleClaimText, c.ToString()));
            return new GenericPrincipal(identity, string.IsNullOrEmpty(roles) ? Array.Empty<string>() : roles.Split(','));
        }

        public async Task BeforeHandleAsync(MessageHandlingContext context)
        {
            if (context.Message.Type == MessageType.GQL_CONNECTION_INIT)
            {
                var payload = context.Message.Payload as JsonElement?;

                if (payload != null)
                {
                    if (payload.Value.TryGetProperty("authorization", out JsonElement keyElement))
                    {
                        var auth = keyElement.GetString();
                        _httpContextAccessor.HttpContext.User = await BuildClaimsPrincipal(auth);
                    }
                    else if (payload.Value.TryGetProperty("Authorization", out JsonElement keyElement2))
                    {
                        var auth = keyElement2.GetString();
                        _httpContextAccessor.HttpContext.User = await BuildClaimsPrincipal(auth);
                    }
                }
            }

            // Always insert the http context user into the message handling context properties
            // Note: any IDisposable item inside the properties bag will be disposed after this message is handled!
            //  So do not insert such items here, but use something like 'context[PRINCIPAL_KEY] = [...]'
            context.Properties[PRINCIPAL_KEY] = _httpContextAccessor.HttpContext.User;
        }

        public Task HandleAsync(MessageHandlingContext context) => Task.CompletedTask;
        public Task AfterHandleAsync(MessageHandlingContext context) => Task.CompletedTask;
    }
}
