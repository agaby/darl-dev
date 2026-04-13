/// <summary>
/// </summary>

﻿using Darl.GraphQL.Process.Models.Alexa;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class InteractionModelType : ObjectGraphType<InteractionModel>
    {
        public InteractionModelType()
        {
            Name = "interactionModel";
            Description = "Alexa InteractionModel for specifying skills";
            Field<LanguageModelType>("languageModel", resolve: c => c.Source.languageModel);
        }
    }
}
