using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class OutputFormatUpdateType : InputObjectGraphType<OutputFormatUpdate>
    {
        public OutputFormatUpdateType()
        {
            Name = "outputFormatUpdate";
            Description = "Format for an output used in a questionnaire";
            Field<BooleanGraphType>("hide").Description("If true).Description(this output's value will not be reported in results");
            Field<DisplayTypeEnum>("displayType").Description("The display type for this output");
            Field<StringGraphType>("scoreBarColor").Description("Color of score bar if specified");
            Field<FloatGraphType>("scoreBarMaxVal").Description("Maximum value of score bar if specified");
            Field<FloatGraphType>("ScoreBarMinVal").Description("Minimum value of score bar if specified");
            Field<BooleanGraphType>("uncertainty").Description("If true uncertainty information is appended to results");
            Field<StringGraphType>("valueFormat").Description("Format for numeric values.");
            Field<StringGraphType>("path").Description("locator for this value in Json or XML source");
        }
    }
}
