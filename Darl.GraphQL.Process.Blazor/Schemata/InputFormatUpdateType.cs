using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class InputFormatUpdateType : InputObjectGraphType<InputFormatUpdate>
    {

        public InputFormatUpdateType()
        {
            Name = "inputFormatUpdate";
            Description = "Format for an input used in a questionnaire";
            Field<FloatGraphType>("increment").Description("the size of the increment used in an editor for numeric inputs");
            Field<IntGraphType>("maxLength").Description("Maximum length for textual inputs");
            Field<FloatGraphType>("numericMax").Description("Maximum value for numeric inputs");
            Field<FloatGraphType>("numericMin").Description("Minimum value for numeric inputs");
            Field<StringGraphType>("regex").Description("validating regex for textual inputs");
            Field<BooleanGraphType>("showSets").Description("If true for a numeric inputs the set names are shown as if a categorical input");
            Field<BooleanGraphType>("enforceCrisp").Description("Gets or sets a value indicating whether to allow the user to give a fuzzy value).Description(default false");
            Field<StringGraphType>("path").Description("If the source data is json).Description(this is a jsonpath expression to locate the data).Description(if XML).Description(Xpath).Description(or a lineage to match variables by conceptual type.");
        }
    }
}
