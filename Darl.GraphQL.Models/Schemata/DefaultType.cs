using Darl.Connectivity.Models;
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
            Field<StringGraphType>("name", resolve:  c => c.Source.RowKey);
            Field(c => c.Value);
        }
    }
}
