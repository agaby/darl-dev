using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlVarInputType : InputObjectGraphType<DarlVar>
    {
        public DarlVarInputType()
        {
            Name = "darlVarUpdate";
            Field<BooleanGraphType>("approximate");
            Field<ListGraphType<StringGraphType>>("categories");
            Field<NonNullGraphType< DarlVarDataTypeEnum >>("dataType");
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<ListGraphType<ListGraphType<StringGraphType>>>("sequence");
            Field<ListGraphType<DateTimeGraphType>>("times");
            Field<BooleanGraphType>("unknown");
            Field<NonNullGraphType<StringGraphType>>("value");
            Field<ListGraphType<FloatGraphType>>("values");
            Field<FloatGraphType>("weight");
        }
    }
}
