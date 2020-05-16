using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class KGTrainingValueType : InputObjectGraphType<KGTrainingValue>
    {
        public KGTrainingValueType()
        {
            Name = "kgTrainingValue";
            Description = "A single value to match.";
            Field<ListGraphType<StringGraphType>>("values", "the strings to softmatch");
            Field<ListGraphType<StringGraphType>>("valueLineages", "the lineages determining the objects to match");
        }
    }
}