using DarlCommon;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public  class InputFormatType : ObjectGraphType<InputFormat>
    {

        public InputFormatType()
        {
            Name = "InputFormat";
            Description = "Format for an input used in a questionnaire";
            Field(c => c.Name).Description("The name of the input defined in the rule set");
            Field<ListGraphType<StringGraphType>>("categories", "All the categories found in the rule set for this input", resolve: context => context.Source.Categories);
            Field(c => c.EnforceCrisp).DefaultValue(false).Description("If true, then only a singleton value for a numeric input, or a single category should be selectable in the UI, default false");
            Field(c => c.Increment).DefaultValue(0.0).Description("optional increment for numeric spinners where supported");
            Field<InputTypeEnum>("inputType", "The data type of the input", resolve: context => context.Source.InType); 
            Field(c => c.MaxLength).DefaultValue(0).Description("The maximum length if textual, (0 means no limit)");
            Field(c => c.NumericMax,true).DefaultValue(0.0).Description("The maximum value for a numeric input (can be null)");
            Field(c => c.NumericMin,true).DefaultValue(0.0).Description("The minimum value for a numeric input (can be null)");
            Field(c => c.path,true).DefaultValue(string.Empty).Description("If the source data is json, this is a jsonpath expression to locate the data, if XML, Xpath, or a lineage to match variables by conceptual type");
            Field(c => c.Regex, true).DefaultValue(string.Empty).Description("A regular expression generating a validation error for a textual input if not met"); 
            Field(c => c.ShowSets).DefaultValue(false).Description("If true, a numeric input is displayed like a categorical one, using set names as categories");
        }
    }
}