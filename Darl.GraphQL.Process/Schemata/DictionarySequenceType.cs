/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DictionarySequenceType : ObjectGraphType<StringSequencePair>
    {
        public DictionarySequenceType()
        {
            Name = "Sequence";
            Description = "A named sequence.";
            Field(c => c.Name);
            Field<ListGraphType<ListGraphType<StringGraphType>>>("Sequence", resolve: context => context.Source.Sequence);
        }
    }
}
