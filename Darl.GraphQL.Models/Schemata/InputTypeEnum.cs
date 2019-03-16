using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class InputTypeEnum : EnumerationGraphType
    {

        public InputTypeEnum()
        {
            Name = "inputTypes";
            AddValue("NUMERIC", "input is numeric", 0);
            AddValue("CATEGORICAL", "input is categorical", 1);
            AddValue("TEXTUAL", "input is textual",2);
        }
    }
}
