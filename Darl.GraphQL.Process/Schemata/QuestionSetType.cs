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
            Field(c => c.canUnwind).Description("If true there are previous answers to go back to ");
            Field(c => c.complete).Description("If true the questionnaire is complete");
            Field(c => c.ieToken).Description("The identifier for the current questionnaire");
            Field(c => c.language,true).Description("The language requested");
            Field(c => c.percentComplete).Description("The percentage of the questionnaire currently complete");
            Field(c => c.preamble,true).Description("Text to display before the questionnaire");
            Field(c => c.questionHeader,true).Description("Text to display before the questions");
            Field<ListGraphType<QuestionDataType>>("questions","The list of questions to be asked", resolve: c => c.Source.questions);
            Field(c => c.questionsRequested).Description("The number of questions to be asked at a time. Default: 1");
            Field(c => c.responseHeader,true).Description("text to display before responses");
            Field<ListGraphType<ResponseDataType>>("responses","A list of responses - the results of the ruleset", resolve: c => c.Source.responses);
            Field<ListGraphType<StringStringPairType>>("values","Status information of unfilled inputs and outputs", resolve: c => BotFormatType.GetSSPairsFromDictionary(c.Source.values));
        }
    }
}
