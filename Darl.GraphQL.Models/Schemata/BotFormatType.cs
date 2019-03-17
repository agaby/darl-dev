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
        public BotFormatType(IDictionaryStringDoublePairService sdpairs, IDictionaryStringSequencePairService sspairs, IDictionaryStringStringPairService stspairs )
        {
            Field<ListGraphType<BotInputFormatType>>("inputformatList", resolve: context => context.Source.InputFormatList);//
            Field<ListGraphType<StringDoublePairType>>("constants", resolve: context => sdpairs.GetPairsFromDictionary(context.Source.Constants));
            Field<ListGraphType<BotOutputFormatType>>("outputformatList", resolve: context => context.Source.OutputFormatList);//
            Field<ListGraphType<DictionarySequenceType>>("sequences", resolve: context => sspairs.GetPairsFromDictionary(context.Source.Sequences));//
            Field<ListGraphType<StringGraphType>>("stores", resolve: context => context.Source.Stores);//
            Field<ListGraphType<StringStringPairType>>("strings", resolve: context => stspairs.GetPairsFromDictionary(context.Source.Strings));//
        }
    }
}
