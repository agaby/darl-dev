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
            Field(c => c.Hide);
            Field(c => c.Name);
            Field<OutputTypeEnum>("OutputType", resolve: context => context.Source.OutputType);
            Field(c => c.path);
            Field(c => c.ScoreBarColor);
            Field(c => c.ScoreBarMaxVal);
            Field(c => c.ScoreBarMinVal);
            Field(c => c.Uncertainty);
            Field(c => c.ValueFormat);
        }
    }
}
