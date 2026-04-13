/// <summary>
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class KGTrainingSpecType : InputObjectGraphType<KGTrainingSpec>
    {
        public KGTrainingSpecType()
        {
            Name = "kgTrainingSpec";
            Description = "A specification and data set for training associations in a knowledge graph from a data source. For each array element associations are inferred between the index object and the values.";
            Field<ListGraphType<KGTrainingValueType>>("values", "the list of associated values to match");
        }
    }
}
