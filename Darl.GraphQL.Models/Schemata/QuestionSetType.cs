using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class QuestionSetType : ObjectGraphType<QuestionSetProxy>
    {
        public QuestionSetType()
        {
            Name = "QuestionSet";
            Description = "An interaction between a user and a questionnaire";
            Field(c => c.canUnwind);
            Field(c => c.complete);
            Field(c => c.ieToken);
            Field(c => c.language);
            Field(c => c.percentComplete);
            Field(c => c.preamble);
            Field(c => c.questionHeader);
            Field<ListGraphType<QuestionType>>("questions", resolve: c => c.Source.questions);
            Field(c => c.questionsRequested);
            Field(c => c.redirect);
            Field(c => c.responseHeader);
            Field<ListGraphType<ResponseType>>("responses", resolve: c => c.Source.responses);
            Field<StringStringPairType>("values", resolve: c => BotFormatType.GetSSPairsFromDictionary(c.Source.values));
        }
    }
}
