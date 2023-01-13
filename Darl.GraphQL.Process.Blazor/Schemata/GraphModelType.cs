using Darl.GraphQL.Process.Blazor.Models;
using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class GraphModelType : ObjectGraphType<IGraphModel>
    {
        public GraphModelType()
        {
            Name = "graphModel";
            Description = "A knowledge graph model";
            Field<ListGraphType<StringGraphObjectPairType>>("vertices").Resolve(c => GetSGOPairsFromDictionary(c.Source.vertices));
            Field<ListGraphType<StringGraphObjectPairType>>("virtualVertices").Resolve(c => GetSGOPairsFromDictionary(c.Source.virtualVertices));
            Field<ListGraphType<StringGraphObjectPairType>>("recognitionVertices").Resolve(c => GetSGOPairsFromDictionary(c.Source.recognitionVertices));
            Field<ListGraphType<StringGraphObjectPairType>>("recognitionRoots").Resolve(c => GetSGOPairsFromDictionary(c.Source.recognitionRoots));
            Field<ListGraphType<StringGraphConnectionPairType>>("edges").Resolve(c => GetSGCPairsFromDictionary(c.Source.edges));
            Field<ListGraphType<StringGraphConnectionPairType>>("virtualEdges").Resolve(c => GetSGCPairsFromDictionary(c.Source.virtualEdges));
            Field<ListGraphType<StringGraphConnectionPairType>>("recognitionEdges").Resolve(c => GetSGCPairsFromDictionary(c.Source.recognitionEdges));
            Field(c => c.author, true);
            Field(c => c.copyright, true);
            Field<DateDisplayEnum>("dateDisplay").Resolve(c => c.Source.dateDisplay);
            Field(c => c.description, true);
            Field<DarlTimeType>("fixedTime").Resolve(c => c.Source.fixedTime);
            Field<InferenceTimeEnum>("inferenceTime").Resolve(c => c.Source.inferenceTime);
            Field(c => c.initialText, true);
            Field(c => c.licenseUrl, true);
            Field(c => c.defaultTarget, true);
            Field(c => c.transient, true);
        }

        public static List<StringGraphObjectPair> GetSGOPairsFromDictionary(Dictionary<string, GraphObject> dict)
        {
            var list = new List<StringGraphObjectPair>();
            foreach (var k in dict.Keys)
            {
                list.Add(new StringGraphObjectPair { Name = k, Value = dict[k] });
            }
            return list;
        }

        public static List<StringGraphConnectionPair> GetSGCPairsFromDictionary(Dictionary<string, GraphConnection> dict)
        {
            var list = new List<StringGraphConnectionPair>();
            foreach (var k in dict.Keys)
            {
                list.Add(new StringGraphConnectionPair { Name = k, Value = dict[k] });
            }
            return list;
        }
    }

}
