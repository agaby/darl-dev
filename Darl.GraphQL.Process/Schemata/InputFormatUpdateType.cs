/// <summary>
/// InputFormatUpdateType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class InputFormatUpdateType : InputObjectGraphType<InputFormatUpdate>
    {

        public InputFormatUpdateType()
        {
            Name = "inputFormatUpdate";
            Description = "Format for an input used in a questionnaire";
            Field<FloatGraphType>("increment", "the size of the increment used in an editor for numeric inputs");
            Field<IntGraphType>("maxLength", "Maximum length for textual inputs");
            Field<FloatGraphType>("numericMax", "Maximum value for numeric inputs");
            Field<FloatGraphType>("numericMin", "Minimum value for numeric inputs");
            Field<StringGraphType>("regex", "validating regex for textual inputs");
            Field<BooleanGraphType>("showSets", "If true for a numeric inputs the set names are shown as if a categorical input");
            Field<BooleanGraphType>("enforceCrisp", "Gets or sets a value indicating whether to allow the user to give a fuzzy value, default false");
            Field<StringGraphType>("path", "If the source data is json, this is a jsonpath expression to locate the data, if XML, Xpath, or a lineage to match variables by conceptual type.");
        }
    }
}
