using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class DefaultType : ObjectGraphType<Default>
    {
        public DefaultType()
        {
            Name = "Default";
            Description = "Name value pairs used to configure the system";
            Field<StringGraphType>("name", resolve:  c => c.Source.Name);
            Field(c => c.Value);
        }
    }
}
