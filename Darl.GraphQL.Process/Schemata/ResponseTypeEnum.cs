/// </summary>

﻿using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class ResponseTypeEnum : EnumerationGraphType
    {
        public ResponseTypeEnum()
        {
            Name = "ResponseType";
            Description = "The ways in which a response can be presented.";
            Add("Preamble", 0, "The text is a preamble");
            Add("Text", 1, "Represent as the response main text");
            Add("ScoreBar", 2, "Represent as a score bar");
            Add("Link", 3, "Represent as a hyperlink");
        }
    }
}
