using DarlCommon;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class OutputFormatType : ObjectGraphType<OutputFormat>
    {
        public OutputFormatType()
        {
            Name = "OutputFormat";
            Description = "Questionnare output format data";
            Field<DisplayTypeEnum>("displayType", resolve: context => context.Source.displayType);
            Field(c => c.Hide, true).DefaultValue(false);
            Field(c => c.Name);
            Field<OutputTypeEnum>("OutputType", resolve: context => context.Source.OutputType);
            Field(c => c.path, true).DefaultValue(string.Empty);
            Field(c => c.ScoreBarColor, true).DefaultValue(string.Empty);
            Field(c => c.ScoreBarMaxVal, true).DefaultValue(0.0);
            Field(c => c.ScoreBarMinVal, true).DefaultValue(0.0);
            Field(c => c.Uncertainty).DefaultValue(false);
            Field(c => c.ValueFormat, true).DefaultValue(string.Empty);
        }
    }
}
