using Darl.GraphQL.Process.Models.Alexa;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class IntentType : ObjectGraphType<Intent>
    {
        public IntentType()
        {
            Name = "intent";
            Description = "An Alexa Intent used in Skill definitions";
            Field(c => c.name);
            Field<ListGraphType<StringGraphType>>("samples", resolve: c => c.Source.samples);
            Field<ListGraphType<SlotType>>("slots", resolve: c => c.Source.slots);
        }
    }
}
