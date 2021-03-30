using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class KGraphUpdateType : InputObjectGraphType<KGraphUpdate>
    {
        public KGraphUpdateType()
        {
            Name = "KGraphUpdate";
            Description = "Update the metadata of the KGraph";
            Field(c => c.Description).Description("A description of the knowledge graph");
            Field(c => c.InitialText).Description("Default initial text for the conversation");
            Field<DateDisplayEnum>("dateDisplay", "Determines if the display form is recent or historic", resolve: context => context.Source.dateDisplay);
            Field<InferenceTimeEnum>("inferenceTime", "Determines if inferences are performed with a current or fixed time.", resolve: context => context.Source.dateDisplay);
            Field<DarlTimeInputType>("fixedTime", "The time of the inference process if in fixed time mode", resolve: context => context.Source.fixedTime);
        }
    }
}
