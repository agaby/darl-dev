/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class QuestionSetInputType : InputObjectGraphType<QuestionSetInput>
    {
        public QuestionSetInputType()
        {
            Name = "QuestionSetInput";
            Description = "A set of questionnaire responses";
            Field(c => c.ieToken);
            Field<ListGraphType<QuestionInputType>>("questions");
        }
    }
}
