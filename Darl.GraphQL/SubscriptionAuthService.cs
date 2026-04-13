/// <summary>
/// </summary>

﻿using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using GraphQL;
//using GraphQL.AspNetCore3.WebSockets;
using GraphQL.Transport;
using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;

namespace Darl.GraphQL
{
    class SubscriptionAuthService : IWebSocketAuthenticationService
    {
        private readonly IGraphQLSerializer _serializer;
        private readonly IKGTranslation _trans;

        public static readonly string PRINCIPAL_KEY = "User";
        static readonly string roleClaimText = "http://schemas.microsoft.com/ws/2008/06/identity/claims/role";


        public SubscriptionAuthService(IGraphQLSerializer serializer, IKGTranslation trans)
        {
            _serializer = serializer;
            _trans = trans;
        }

        public async Task AuthenticateAsync(IWebSocketConnection connection, string sub, OperationMessage operationMessage)
        {
            // read payload of ConnectionInit message and look for an "Authorization" entry that starts with "Bearer "
            var payload = _serializer.ReadNode<Inputs>(operationMessage.Payload);
            if ((payload?.TryGetValue("Authorization", out var value) ?? false) && value is string valueString)
            {
                var user = await ParseToken(valueString);
                if (user != null)
                {
                    connection.HttpContext.User = user;
                }
            }
        }

        private async Task<ClaimsPrincipal?> ParseToken(string token)
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
    }
}
