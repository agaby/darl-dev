using Darl.GraphQL.Models.Models;
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
            Field(c => c.approximate,true);
            Field<ListGraphType<StringDoublePairType>>("categories", resolve: context => GetSDPairsFromDictionary(context.Source.categories));
            Field<DarlVarDataTypeEnum>("dataType", resolve: context => context.Source.dataType);
            Field(c => c.name);
            Field<ListGraphType<ListGraphType<StringGraphType>>>("sequence", resolve: context => context.Source.sequence);
            Field(c => c.times,true);
            Field(c => c.unknown,true);
            Field(c => c.Value);
            Field<ListGraphType<FloatGraphType>>("values", resolve: context => context.Source.values);
            Field(c => c.weight);
        }

        public static List<StringDoublePair> GetSDPairsFromDictionary(Dictionary<string, double> dict)
        {
            var list = new List<StringDoublePair>();
            if(dict != null)
            { 
                foreach (var k in dict.Keys)
                {
                    list.Add(new StringDoublePair { name = k, value = dict[k] });
                }
            }
            return list;
        }
    }
}
