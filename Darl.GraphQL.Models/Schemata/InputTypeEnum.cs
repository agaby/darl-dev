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
            Description = "The data types of inputs";
            AddValue("NUMERIC", "input is numeric", 0);
            AddValue("CATEGORICAL", "input is categorical", 1);
            AddValue("TEXTUAL", "input is textual",2);
        }
    }
}
