using Darl.GraphQL.Models.Schemata;
using Darl.Thinkbase;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    class KnowledgeStateType : ObjectGraphType<KnowledgeState>
    {
        public KnowledgeStateType()
        {
            Name = "KnowledgeState";
            Description = "Represents a state of knowledge used to make inferences from a knowledge graph";
            Field(c => c.Id).Description("The Id of the knowledge state");
            Field(c => c.knowledgeGraphName).Description("The name of the knowledge graph this relates to");
            Field(c => c.subjectId).Description("The external reference of the subject of the state");
            Field(c => c.userId).Description("The id of the owner of the knowledge state");
            Field<StringListGraphAttributePairType>("data", resolve: c => c.Source.data);
        }
    }
}
