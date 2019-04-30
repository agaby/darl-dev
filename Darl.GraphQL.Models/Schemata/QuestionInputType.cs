using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class QuestionInputType : InputObjectGraphType<QuestionInput>
    {
        public QuestionInputType()
        {
            Name = "QuestionInput";
            Description = "One response from a questionaire user";
            Field(c => c.dResponse);
            Field(c => c.reference);
            Field(c => c.sResponse);
        }
    }
}
