using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class DictionarySequenceType : ObjectGraphType<StringSequencePair>
    {
        public DictionarySequenceType()
        {
            Field(c => c.Name);
            Field<ListGraphType<ListGraphType<StringGraphType>>>("Sequence", resolve: context => context.Source.Sequence);
        }
    }
}
