/// </summary>

﻿using Darl.Lineage;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageRecordType : ObjectGraphType<LineageRecord>
    {
        public LineageRecordType()
        {
            Name = "lineageRecord";
            Description = "A concept and it's associated typeWord";
            Field(c => c.description);
            Field(c => c.lineage);
            Field<LineageTypeEnum>("lineageType", resolve: c => c.Source.type);
            Field(c => c.typeWord);
            Field<ListGraphType<LineageAssociationType>>("follows", resolve: c => c.Source.follows);
            Field<ListGraphType<LineageAssociationType>>("precedes", resolve: c => c.Source.precedes);
        }
    }
}
