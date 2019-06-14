using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlVarType : ObjectGraphType<DarlVar>
    {
        public DarlVarType()
        {
            Name = "darlVar";
            Description = "A variable of any type supported by DARL with associated uncertainty";
            Field(c => c.approximate);
            Field<ListGraphType<StringGraphType>>("categories", resolve: context => context.Source.categories);
            Field<DarlVarDataTypeEnum>("dataType", resolve: context => context.Source.dataType);
            Field(c => c.name);
            Field<ListGraphType<ListGraphType<StringGraphType>>>("sequence", resolve: context => context.Source.sequence);
            Field(c => c.times);
            Field(c => c.unknown);
            Field(c => c.Value);
            Field<ListGraphType<FloatGraphType>>("values", resolve: context => context.Source.values);
            Field(c => c.weight);
        }
    }
}
