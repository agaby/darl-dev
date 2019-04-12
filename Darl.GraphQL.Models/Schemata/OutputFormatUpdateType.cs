using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class OutputFormatUpdateType : InputObjectGraphType<OutputFormatUpdate>
    {
        public OutputFormatUpdateType()
        {
            Name = "outputFormatUpdate";
            Description = "Format for an output used in a questionnaire";
            Field<NonNullGraphType<BooleanGraphType>>("hide");
            Field<NonNullGraphType<DisplayTypeEnum>>("displayType");
            Field<NonNullGraphType<StringGraphType>>("scoreBarColor");
            Field<NonNullGraphType<FloatGraphType>>("scoreBarMaxVal");
            Field<NonNullGraphType<FloatGraphType>>("ScoreBarMinVal");
            Field<NonNullGraphType<BooleanGraphType>>("uncertainty");
            Field<NonNullGraphType<StringGraphType>>("valueFormat");
            Field<NonNullGraphType<StringGraphType>>("path");
        }
    }
}
