using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class MLSpecUpdateType : InputObjectGraphType<MLSpecUpdate>
    {

        public MLSpecUpdateType()
        {
            Name = "MLSpecUpdate";
            Field<StringGraphType>("darl");
            Field<StringGraphType>("version");
            Field<StringGraphType>("author");
            Field<StringGraphType>("copyright");
            Field<StringGraphType>("license");
            Field<StringGraphType>("description");
            Field<StringGraphType>("trainData");
            Field<StringGraphType>("dataSchema");
            Field<SetGraphType>("sets");
            Field<PercentGraphType>("percentTest");
            Field<StringGraphType>("destinationRulesetName");
        }
    }
}
