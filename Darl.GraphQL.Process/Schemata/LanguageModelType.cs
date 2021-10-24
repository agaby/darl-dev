using Darl.GraphQL.Process.Models.Alexa;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class LanguageModelType : ObjectGraphType<LanguageModel>
    {
        public LanguageModelType()
        {
            Name = "languageModel";
            Description = "Alexa Language model for skill definition";
            Field(c => c.invocationName);
            Field<ListGraphType<IntentType>>("intents", resolve: c => c.Source.intents);
            Field<ListGraphType<AlexaTypeType>>("types", resolve: c => c.Source.types);
        }
    }
}
