using GraphQL.Types;
using GemBox.Document;
using GraphQL.Validation;
using GraphQLParser;
using GraphQLParser.AST;
using System.Security.Claims;
using System.Xml.Linq;
using ThinkBase.Teams.Connectivity;
using Microsoft.Extensions.Caching.Memory;
using MongoDB.Bson;

namespace Darl.GraphQL.Blazor.Server.Helpers
{
    public class LicenseValidationRule : IValidationRule
    {
        static readonly string objectIdClaimText = @"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier";

        private readonly ILicenseConnectivity _license;
        private readonly IConfiguration _config;
        private readonly IMemoryCache _mem;
        private readonly int cacheDurationMinutes = 30;

        public LicenseValidationRule(ILicenseConnectivity license, IMemoryCache memoryCache, IConfiguration config)
        {
            _license= license;
            _mem = memoryCache;
            _config = config;
        }

        public async ValueTask<INodeVisitor> ValidateAsync(ValidationContext context)
        {
            return  new MatchingNodeVisitor<GraphQLOperationDefinition>(async (op, context) =>  
            {
                var userContext = (context.UserContext["ident"]) as ClaimsIdentity;
                if(!userContext!.IsAuthenticated)
                {
                    context.ReportError(new ValidationError("Not authenticated. Please log in. "));
                }
                else
                {
                    var objectId = userContext!.Claims.Where(ai => ai.Type == objectIdClaimText).Single().Value;
                    var tenantId = userContext!.Claims.Where(ai => ai.Type == objectIdClaimText).Single().Value;
                    var permitted = _mem.Get<bool?>(objectId);
                    var error = false;
                    if (permitted != null && permitted == false)
                    {
                        error = true;
                    }
                    else if(permitted == null)
                    {
                        //cache avoids repeated calls here.
                        permitted = await _license.IsLicensed(objectId, tenantId, _config["planId"]!);
                        _mem.Set(objectId, permitted, TimeSpan.FromMinutes(cacheDurationMinutes));
                    }
                    if(error || !(permitted ?? false) )
                    {
                         context.ReportError(new ValidationError("Not Licensed. Please ask your administrator for a license to use this service. "));
                    }                
                }
            });
        }
    }
}
