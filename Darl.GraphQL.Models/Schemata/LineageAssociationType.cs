using Darl.Lineage;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageAssociationType : ObjectGraphType<LineageAssociation>
    {
        public LineageAssociationType()
        {
            Name = "lineageAssociation";
            Field(c => c.weight);
            Field<LineageRecordType>("start", resolve: c => c.Source.start);
            Field<LineageRecordType>("end", resolve: c => c.Source.end);
        }
    }
}
