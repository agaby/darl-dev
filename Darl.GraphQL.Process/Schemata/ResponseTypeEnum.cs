using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class ResponseTypeEnum : EnumerationGraphType
    {
        public ResponseTypeEnum()
        {
            Name = "ResponseType";
            Description = "The ways in which a response can be presented.";
            AddValue("Preamble", "The text is a preamble", 0);
            AddValue("Text", "Represent as the response main text", 1);
            AddValue("ScoreBar", "Represent as a score bar", 2);
            AddValue("Link", "Represent as a hyperlink", 3);
        }
    }
}
