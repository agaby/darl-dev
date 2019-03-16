using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class PostTypeEnum : EnumerationGraphType
    {
        public PostTypeEnum()
        {
            Name = "postTypes";
            AddValue("DARLVARLIST", "pass a list of DarlVar objects", 0);
            AddValue("FORM", "Use a POST form of name value pairs", 1);
        }
    }
}
