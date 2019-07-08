using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class QuestionTypeEnum : EnumerationGraphType
    {
        public QuestionTypeEnum()
        {
            Name = "";
            Description = "The data type sought.";
            AddValue("numeric", "a number", 0);
            AddValue("categorical", "text from a range of texts", 1);
            AddValue("textual", "Free form text", 2);
        }
    }
}
