using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class KGTrainingValueType : InputObjectGraphType<KGTrainingValue>
    {
        public KGTrainingValueType()
        {
            Name = "kgTrainingValue";
            Description = "A single value to match.";
            Field<ListGraphType<StringGraphType>>("values").Description("the strings to softmatch");
            Field<ListGraphType<StringGraphType>>("valueLineages").Description("the lineages determining the objects to match");
        }
    }
}