using Darl.Thinkbase;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class KnowledgeStateInputType : InputObjectGraphType<KnowledgeStateInput>
    {

        public KnowledgeStateInputType()
        {
            Name = "KnowledgeStateInput";
            Description = "Represents a state of knowledge used to make inferences from a knowledge graph";
            Field(c => c.knowledgeGraphName).Description("The name of the knowledge graph this relates to");
            Field(c => c.subjectId).Description("The external reference of the subject of the state");
            Field<ListGraphType<StringListGraphAttributePairInputType>>("data", resolve: c => c.Source.data);
        }
    }
}
