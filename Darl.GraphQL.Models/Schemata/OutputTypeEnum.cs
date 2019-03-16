using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class OutputTypeEnum : EnumerationGraphType
    {
        public OutputTypeEnum()
        {
            Name = "outputTypes";
            AddValue("NUMERIC", "Output is numeric", 0);
            AddValue("CATEGORICAL", "Output is categorical", 1);
            AddValue("TEXTUAL", "Output is textual", 2);
        }
    }
}
