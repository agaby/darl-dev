using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class QuestionDataType : ObjectGraphType<QuestionProxy>
    {

        public QuestionDataType()
        {
            Name = "Question";
            Description = "A single question";
            Field<ListGraphType<StringGraphType>>("categories", resolve: c => c.Source.categories);
            Field(c => c.dResponse);
            Field(c => c.enforceCrisp);
            Field(c => c.format);
            Field(c => c.increment);
            Field(c => c.maxval);
            Field(c => c.minval);
            Field(c => c.path);
            Field<QuestionTypeEnum>("qType", resolve: c => c.Source.qtype);
            Field(c => c.reference);
            Field(c => c.sResponse);
            Field(c => c.text);
        }
    }
}
