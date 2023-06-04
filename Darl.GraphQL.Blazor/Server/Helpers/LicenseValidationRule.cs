using GraphQL.Validation;
using GraphQLParser.AST;
using Microsoft.Extensions.Caching.Memory;

namespace Darl.GraphQL.Blazor.Server.Helpers
{
    public class LicenseValidationRule : IValidationRule
    {
        static readonly string objectIdClaimText = @"http://schemas.microsoft.com/identity/claims/objectidentifier";
        static readonly string tenantIdClaimText = @"http://schemas.microsoft.com/identity/claims/tenantid";

        private readonly IConfiguration _config;
        private readonly IMemoryCache _mem;
        private readonly int cacheDurationMinutes = 30;
        private readonly bool allowAnonymous = false;

        public LicenseValidationRule(IMemoryCache memoryCache, IConfiguration config)
        {
            _mem = memoryCache;
            _config = config;
            allowAnonymous = _config.GetValue<bool?>("allowAnonymous") ?? false;
        }

        public async ValueTask<INodeVisitor> ValidateAsync(ValidationContext context)
        {
            return  new MatchingNodeVisitor<GraphQLOperationDefinition>(async (op, context) =>  
            {
                if (!allowAnonymous)
                {
                    if (!context!.User!.Identity!.IsAuthenticated)
                    {
                        context.ReportError(new ValidationError("Not authenticated. Please log in. "));
                    }
                    else
                    {

                    }
                }
            });
        }
    }
}
