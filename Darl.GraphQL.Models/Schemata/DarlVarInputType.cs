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
            Field<NonNullGraphType<BooleanGraphType>>("approximate");
            Field<NonNullGraphType<ListGraphType<StringGraphType>>>("categories");
            Field<NonNullGraphType< DarlVarDataTypeEnum >>("dataType");
            Field<NonNullGraphType<StringGraphType>>("name");
            Field<NonNullGraphType<ListGraphType<ListGraphType<StringGraphType>>>>("sequence");
            Field<NonNullGraphType<ListGraphType<DateGraphType>>>("times");
            Field<NonNullGraphType<BooleanGraphType>>("unknown");
            Field<NonNullGraphType<StringGraphType>>("value");
            Field<NonNullGraphType<ListGraphType<FloatGraphType>>>("values");
            Field <NonNullGraphType<FloatGraphType>>("weight");
        }
    }
}
