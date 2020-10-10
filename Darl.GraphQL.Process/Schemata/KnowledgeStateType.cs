using Darl.GraphQL.Models.Models;
using Darl.GraphQL.Models.Schemata;
using Darl.Thinkbase;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class KnowledgeStateType : ObjectGraphType<KnowledgeState>
    {
        public KnowledgeStateType()
        {
            Name = "KnowledgeState";
            Description = "Represents a state of knowledge used to make inferences from a knowledge graph";
            Field(c => c.Id).Description("The Id of the knowledge state");
            Field(c => c.knowledgeGraphName).Description("The name of the knowledge graph this relates to");
            Field(c => c.userId).Description("The id of the owner of the knowledge state");
            Field<ListGraphType<StringListGraphAttributePairType>>("data", resolve: c => GetSGAPairsFromDictionary(c.Source.data));
        }

        public static List<StringListGraphAttributePair> GetSGAPairsFromDictionary(Dictionary<string, List<GraphAttribute>> dict)
        {
            var list = new List<StringListGraphAttributePair>();
            foreach (var k in dict.Keys)
            {
                list.Add(new StringListGraphAttributePair { Name = k, Value = dict[k]});
            }
            return list;
        }
    }
}
