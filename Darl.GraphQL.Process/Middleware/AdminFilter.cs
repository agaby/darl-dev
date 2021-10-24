using Darl.GraphQL.Models.Middleware;
using GraphQL.Introspection;
using GraphQL.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Process.Middleware
{
    /// <summary>
    /// Used to remove administrator only facilities from the observable schema
    /// </summary>
    public class AdminFilter : ISchemaFilter
    {
        private readonly IAuthChecker _authChecker;
        public AdminFilter(IAuthChecker authChecker)
        {
            _authChecker = authChecker;
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
            if (field.Metadata != null && field.Metadata.ContainsKey(AuthorizationMetadataExtensions.PolicyKey))
            {
                var policies = (List<string>)field.Metadata[AuthorizationMetadataExtensions.PolicyKey];
                if (policies.Count == 1 && policies[0] == "AdminPolicy")
                {
                    return await _authChecker.AuthorizedAdmin();
                }
            }
            return true;
        }

        //hide administrator only types 
        public async Task<bool> AllowType(IGraphType type)
        {
            if (type.Metadata != null && type.Metadata.ContainsKey(AuthorizationMetadataExtensions.PolicyKey))
            {
                var policies = (List<string>)type.Metadata[AuthorizationMetadataExtensions.PolicyKey];
                if (policies.Count == 1 && policies[0] == "AdminPolicy")
                {
                    return await _authChecker.AuthorizedAdmin();
                }
            }
            return true;
        }
    }
}
