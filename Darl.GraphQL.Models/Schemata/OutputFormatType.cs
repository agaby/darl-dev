using Darl.GraphQL.Models.Models;
using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class OutputFormatType : ObjectGraphType<OutputFormat>
    {
        public OutputFormatType()
        {
            Field<DisplayTypeEnum>("displayType", resolve: context => context.Source.displayType);
            Field(c => c.Hide).DefaultValue(false);
            Field(c => c.Name);
            Field<OutputTypeEnum>("OutputType", resolve: context => context.Source.OutputType);
            Field(c => c.path).DefaultValue(string.Empty);
            Field(c => c.ScoreBarColor).DefaultValue(string.Empty);
            Field(c => c.ScoreBarMaxVal).DefaultValue(0.0);
            Field(c => c.ScoreBarMinVal).DefaultValue(0.0);
            Field(c => c.Uncertainty).DefaultValue(false);
            Field(c => c.ValueFormat).DefaultValue(string.Empty);
        }
    }
}
