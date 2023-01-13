using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class DictionarySequenceType : ObjectGraphType<StringSequencePair>
    {
        public DictionarySequenceType()
        {
            Name = "Sequence";
            Description = "A named sequence.";
            Field(c => c.Name);
            Field<ListGraphType<ListGraphType<StringGraphType>>>("Sequence").Resolve(context => context.Source.Sequence);
        }
    }
}
