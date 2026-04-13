/// <summary>
/// InputTypeEnum.cs - Core module for the Darl.dev project.
/// </summary>

﻿using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class InputTypeEnum : EnumerationGraphType
    {

        public InputTypeEnum()
        {
            Name = "inputTypes";
            Description = "The data types of inputs";
            Add("NUMERIC", 0, "input is numeric");
            Add("CATEGORICAL", 1, "input is categorical");
            Add("TEXTUAL", 2, "input is textual");
            Add("TEMPORAL", 3, "input is temporal");

        }
    }
}
