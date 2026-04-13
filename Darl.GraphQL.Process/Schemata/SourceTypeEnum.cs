/// <summary>
/// SourceTypeEnum.cs - Core module for the Darl.dev project.
/// </summary>

﻿using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class SourceTypeEnum : EnumerationGraphType
    {
        public SourceTypeEnum()
        {
            Name = "sourceTypes";
            Add("RESULTS", 0, "Get the value from a result in the ruleset output");
            Add("FIXEDVALUE", 1, "The values is determined at design time");
        }
    }
}
