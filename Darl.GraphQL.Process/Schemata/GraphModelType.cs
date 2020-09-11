using Darl.GraphQL.Models.Models;
using Darl.Thinkbase;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class GraphModelType : ObjectGraphType<IGraphModel>
    {
        public GraphModelType()
        {
            Name = "graphModel";
            Description = "A knowledge graph model";
            Field<ListGraphType<StringGraphObjectPairType>>("vertices", resolve: c => GetSGOPairsFromDictionary(c.Source.vertices));
            Field<ListGraphType<StringGraphObjectPairType>>("virtualVertices", resolve: c => GetSGOPairsFromDictionary(c.Source.virtualVertices));
            Field<ListGraphType<StringGraphObjectPairType>>("recognitionVertices", resolve: c => GetSGOPairsFromDictionary(c.Source.recognitionVertices));
            Field<ListGraphType<StringGraphObjectPairType>>("recognitionRoots", resolve: c => GetSGOPairsFromDictionary(c.Source.recognitionRoots));
            Field<ListGraphType<StringGraphObjectPairType>>("edges", resolve: c => GetSGCPairsFromDictionary(c.Source.edges));
            Field<ListGraphType<StringGraphObjectPairType>>("virtualEdges", resolve: c => GetSGCPairsFromDictionary(c.Source.virtualEdges));
            Field<ListGraphType<StringGraphObjectPairType>>("recognitionEdges", resolve: c => GetSGCPairsFromDictionary(c.Source.recognitionEdges));
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
