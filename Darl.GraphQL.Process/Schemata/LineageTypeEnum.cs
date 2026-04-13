/// </summary>

﻿using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class LineageTypeEnum : EnumerationGraphType
    {
        public LineageTypeEnum()
        {
            Name = "lineageTypes";
            Add("CONCEPT", 0, "A concept");
            Add("REFERENCE", 1, "A reference");
            Add("VALUE", 2, "A value");
            Add("LITERAL", 3, "A literal");
            Add("DEFAULT", 4, "A default");
            Add("COMPOSITE", 5, "A composite type");
        }
    }
}
