using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Services;
using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class BotFormatType : ObjectGraphType<BotFormat>
    {
        public BotFormatType()
        {
            Field<ListGraphType<BotInputFormatType>>("inputformatList", resolve: context => context.Source.InputFormatList);//
            Field<ListGraphType<StringDoublePairType>>("constants", resolve: context => GetSDPairsFromDictionary(context.Source.Constants));
            Field<ListGraphType<BotOutputFormatType>>("outputformatList", resolve: context => context.Source.OutputFormatList);//
            Field<ListGraphType<DictionarySequenceType>>("sequences", resolve: context => GetSSQPairsFromDictionary(context.Source.Sequences));//
            Field<ListGraphType<StringGraphType>>("stores", resolve: context => context.Source.Stores);//
            Field<ListGraphType<StringStringPairType>>("strings", resolve: context => GetSSPairsFromDictionary(context.Source.Strings));//
        }

        public static List<StringDoublePair> GetSDPairsFromDictionary(Dictionary<string, double> dict)
        {
            var list = new List<StringDoublePair>();
            foreach (var k in dict.Keys)
            {
                list.Add(new StringDoublePair { name = k, value = dict[k] });
            }
            return list;
        }

        public static List<StringSequencePair> GetSSQPairsFromDictionary(Dictionary<string, List<List<string>>> dict)
        {
            var list = new List<StringSequencePair>();
            foreach (var k in dict.Keys)
            {
                list.Add(new StringSequencePair(k, dict[k]));
            }
            return list;
        }

        public List<StringStringPair> GetSSPairsFromDictionary(Dictionary<string, string> dict)
        {
            var list = new List<StringStringPair>();
            foreach (var k in dict.Keys)
            {
                list.Add(new StringStringPair(k, dict[k]));
            }
            return list;
        }
    }
}
