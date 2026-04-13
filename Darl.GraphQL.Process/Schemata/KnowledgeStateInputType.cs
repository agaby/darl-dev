/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class KnowledgeStateInputType : InputObjectGraphType<KnowledgeStateInput>
    {

        public KnowledgeStateInputType()
        {
            Name = "knowledgeStateInput";
            Description = "Represents a partial state of knowledge used to make inferences from a knowledge graph";
            Field(c => c.knowledgeGraphName).Description("The name of the knowledge graph this relates to");
            Field(c => c.subjectId).Description("The external reference of the subject of the state");
            Field<NonNullGraphType<ListGraphType<StringListGraphAttributeInputPairInputType>>>("data", resolve: c => c.Source.data);
            Field(c => c.transient, true).Description("If true, triggers any subscriptions but is not stored.").DefaultValue(false);
        }

    }
}
