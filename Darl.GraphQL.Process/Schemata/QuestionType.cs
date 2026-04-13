/// <summary>
/// </summary>

﻿using DarlCommon;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class QuestionDataType : ObjectGraphType<QuestionProxy>
    {

        public QuestionDataType()
        {
            Name = "Question";
            Description = "A single question";
            Field<ListGraphType<StringGraphType>>("categories", resolve: c => c.Source.categories);
            Field(c => c.dResponse, true);
            Field(c => c.enforceCrisp, true);
            Field(c => c.format, true);
            Field(c => c.increment, true);
            Field(c => c.maxval, true);
            Field(c => c.minval, true);
            Field(c => c.path, true);
            Field<QuestionTypeEnum>("qType", resolve: c => c.Source.qtype);
            Field(c => c.reference);
            Field(c => c.sResponse, true);
            Field(c => c.text, true);
        }
    }
}
