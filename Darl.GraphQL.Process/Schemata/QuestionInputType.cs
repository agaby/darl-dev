/// <summary>
/// QuestionInputType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class QuestionInputType : InputObjectGraphType<QuestionInput>
    {
        public QuestionInputType()
        {
            Name = "QuestionInput";
            Description = "One response from a questionnaire user";
            Field(c => c.dResponse, true).Description("The value if numeric");
            Field(c => c.reference).Description("The reference of the question asked");
            Field(c => c.sResponse, true).Description("The value if not numeric");
            Field<QuestionTypeEnum>("qType", "The type of the question", resolve: c => c.Source.qType);
        }
    }
}
