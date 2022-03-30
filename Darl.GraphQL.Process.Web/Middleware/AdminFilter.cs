using GraphQL;
using GraphQL.Introspection;
using GraphQL.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Process.Middleware
{
    /// <summary>
    /// Used to remove administrator only facilities from the observable schema
    /// </summary>
    public class AdminFilter : ISchemaFilter
    {
        private readonly IAuthorizationService _authServ;
        private readonly IHttpContextAccessor _httpContext;
        public AdminFilter(IAuthorizationService authServ, IHttpContextAccessor httpContext)
        {
            _authServ = authServ;
            _httpContext = httpContext;
        }

        public Task<bool> AllowArgument(IFieldType field, QueryArgument argument)
        {
            return Task.FromResult(true);
        }

        public Task<bool> AllowDirective(DirectiveGraphType directive)
        {
            return Task.FromResult(true);
        }

        public Task<bool> AllowEnumValue(EnumerationGraphType parent, EnumValueDefinition enumValue)
        {
            return Task.FromResult(true);
        }

        public async Task<bool> AllowField(IGraphType parent, IFieldType field)
        {
            if (field.Metadata != null && field.Metadata.ContainsKey(AuthorizationExtensions.POLICY_KEY))
            {
                var policies = (List<string>)field.Metadata[AuthorizationExtensions.POLICY_KEY];
                if (policies.Count == 1 && policies[0] == "AdminPolicy")
                {
                    var r = await _authServ.AuthorizeAsync(_httpContext.HttpContext.User, "AdminPolicy");
                    return r.Succeeded;
                }
            }
            return true;
        }

        //hide administrator only types 
        public async Task<bool> AllowType(IGraphType type)
        {
            if (type.Metadata != null && type.Metadata.ContainsKey(AuthorizationExtensions.POLICY_KEY))
            {
                var policies = (List<string>)type.Metadata[AuthorizationExtensions.POLICY_KEY];
                if (policies.Count == 1 && policies[0] == "AdminPolicy")
                {
                    var r = await _authServ.AuthorizeAsync(_httpContext.HttpContext.User, "AdminPolicy");
                    return r.Succeeded;
                }
            }
            return true;
        }
    }
}
