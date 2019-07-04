using DarlCommon;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class RuleFormType : ObjectGraphType<RuleForm>
    {
        public RuleFormType()
        {
            Name = "RuleForm";
            Description = "Questionnaire detail";
            Field(c => c.author,true).DefaultValue(string.Empty).Description("Author of the ruleset");
            Field(c => c.copyright,true).DefaultValue(string.Empty).Description("Copyright holder of the ruleset");
            Field(c => c.currency, true).DefaultValue(string.Empty).Description("Currency for charges for use of the ruleset");
            Field(c => c.darl).DefaultValue(string.Empty).Description("The Darl code");
            Field(c => c.description, true).DefaultValue(string.Empty).Description("Description of the ruleset");
            Field<LanguageFormatType>("language", "The text used with the ruleset", resolve: context => context.Source.language);
            Field(c => c.license, true).DefaultValue(string.Empty).Description("License of the ruleset");
            Field(c => c.name).Description("License of the ruleset");
            Field(c => c.price).DefaultValue(0.0).Description("Charge for use of the ruleset");
            Field(c => c.testData, true).Description("Test Data for  the ruleset");
            Field(c => c.version, true).DefaultValue(string.Empty).Description("Version of the ruleset");
            Field<TriggerViewType>("trigger", "Activities to occur on execution of the ruleset", resolve: context => context.Source.trigger);
            Field<FormFormatType>("format", "Describes how I/O is presented",resolve: context => context.Source.format);
            Field<ListGraphType<DarlVarType>>("preload", "Data provided at each run of the ruleset", resolve: context => context.Source.preload);

        }
    }
}
