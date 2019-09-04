using DarlCommon;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class LanguageFormatType : ObjectGraphType<LanguageFormat>
    {

        public LanguageFormatType()
        {
            Name = "LanguageFormat";
            Description = "Contains the texts used in a questionnaire";
            Field(c => c.DefaultLanguage).DefaultValue("En");
            Field<ListGraphType<LanguageTextType>>("languageList", resolve: context => context.Source.LanguageList);
        }
    }
}
