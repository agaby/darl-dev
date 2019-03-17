using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class StringDoublePairType : ObjectGraphType<StringDoublePair>
    {
        public StringDoublePairType()
        {
            Field(c => c.name);
            Field(c => c.value);
        }
    }
}
