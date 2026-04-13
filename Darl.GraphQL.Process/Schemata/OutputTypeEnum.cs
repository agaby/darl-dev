/// </summary>

﻿using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class OutputTypeEnum : EnumerationGraphType
    {
        public OutputTypeEnum()
        {
            Name = "outputTypes";
            Add("NUMERIC", 0, "Output is numeric");
            Add("CATEGORICAL", 1, "Output is categorical");
            Add("TEXTUAL", 2, "Output is textual");
            Add("TEMPORAL", 3, "Output is temporal");
        }
    }
}
