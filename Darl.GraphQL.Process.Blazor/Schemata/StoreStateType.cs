using Darl.Lineage.Bot;
using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class StoreStateType : ObjectGraphType<StoreState>
    {
        public StoreStateType()
        {
            Name = "storeState";
            Description = "represents the state of an internal store in bot testing";
            Field(c => c.name).Description("The name of the store");
            Field<ListGraphType<StringStringPairType>>("states").Resolve(context => GetSSPairsFromDictionary(context.Source.states));//
        }

        public static List<StringStringPair> GetSSPairsFromDictionary(Dictionary<string, string> dict)
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
