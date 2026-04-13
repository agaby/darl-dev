/// <summary>
/// OutputFormatUpdateType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class OutputFormatUpdateType : InputObjectGraphType<OutputFormatUpdate>
    {
        public OutputFormatUpdateType()
        {
            Name = "outputFormatUpdate";
            Description = "Format for an output used in a questionnaire";
            Field<BooleanGraphType>("hide", "If true, this output's value will not be reported in results");
            Field<DisplayTypeEnum>("displayType", "The display type for this output");
            Field<StringGraphType>("scoreBarColor", "Color of score bar if specified");
            Field<FloatGraphType>("scoreBarMaxVal", "Maximum value of score bar if specified");
            Field<FloatGraphType>("ScoreBarMinVal", "Minimum value of score bar if specified");
            Field<BooleanGraphType>("uncertainty", "If true uncertainty information is appended to results");
            Field<StringGraphType>("valueFormat", "Format for numeric values.");
            Field<StringGraphType>("path", "locator for this value in Json or XML source");
        }
    }
}
