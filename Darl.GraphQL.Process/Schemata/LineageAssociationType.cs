/// <summary>
/// LineageAssociationType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Lineage;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageAssociationType : ObjectGraphType<LineageAssociation>
    {
        public LineageAssociationType()
        {
            Name = "lineageAssociation";
            Description = "The weighted association between two lineages";
            Field(c => c.weight);
            Field<LineageRecordType>("start", resolve: c => c.Source.start);
            Field<LineageRecordType>("end", resolve: c => c.Source.end);
        }
    }
}
