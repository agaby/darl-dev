using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class MatchResultType : ObjectGraphType<MatchResult>
    {
        public MatchResultType()
        {
            Name = "matchResult";
            Description = "A match found in a concept match model";
            Field(c => c.distance).Description("The distance between the matched text and the source");
            Field(c => c.confidence).Description("The confidence of the match");
            Field(c => c.index).Description("The index of the match");
            Field(c => c.matchedWords).Description("The number of word matches");
            Field(c => c.referenceText).Description("The original text matched against");
            Field(c => c.sourceText).Description("The text used for the match");
        }
    }
}
