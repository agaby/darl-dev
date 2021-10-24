using System.Collections.Generic;

namespace Darl.GraphQL.Models.Models
{
    public class StringSequencePair
    {
        public StringSequencePair(string name, List<List<string>> sequence)
        {
            Name = name;
            Sequence = sequence;
        }

        public string Name { get; }
        public List<List<string>> Sequence { get; }
    }
}
