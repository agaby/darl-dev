using Darl.GraphQL.Models.Middleware;
using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using GraphQL;

namespace Darl.GraphQL.Models.Schemata
{
    public class DefaultType : ObjectGraphType<Default>
    {
        public DefaultType()
        {
            Name = "Default";
            this.AuthorizeWith("AdminPolicy");

            Description = "Name value pairs used to configure the system";
            Field(c => c.Name);
            Field(c => c.Value);
        }
    }
}
