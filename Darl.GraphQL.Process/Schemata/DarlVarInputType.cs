/// <summary>
/// DarlVarInputType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCommon;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlVarInputType : InputObjectGraphType<DarlVar>
    {
        public DarlVarInputType()
        {
            Name = "darlVarInput";
            Field(c => c.approximate, true).Description("For numeric inputs reports that alpha cuts approximations have been made.").DefaultValue(false);
            Field(c => c.unknown, true).Description("If true the value of this input is unknown.").DefaultValue(false);
            Field<ListGraphType<StringDoublePairInputType>>("categories", "possible categories of this input if categorical");
            Field<NonNullGraphType<DarlVarDataTypeEnum>>("dataType", "The data type of this input");
            Field(c => c.name).Description("the name of this input");
            Field<ListGraphType<ListGraphType<StringGraphType>>>("sequence");
            Field<ListGraphType<DarlTimeInputType>>("times");
            Field(c => c.Value).Description("the central or crisp value of this input");
            Field<ListGraphType<FloatGraphType>>("values");
            Field(c => c.weight, true).Description("The degree of truth associated with this value").DefaultValue(1.0);
        }


    }
}
