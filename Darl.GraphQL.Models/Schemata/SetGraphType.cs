using GraphQL.Language.AST;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class SetGraphType : IntGraphType
    {
        public SetGraphType()
        {
            Name = "SetDefinition";
        }

        public override object ParseValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (int.TryParse(value.ToString(), out var intResult))
            {
                if(intResult == 3 || intResult == 5 || intResult == 7 || intResult == 9)
                    return intResult;
            }
            return null;
        }

        public override object ParseLiteral(IValue value)
        {
            if (value is IntValue intValue)
            {
                if (intValue.Value == 3 || intValue.Value == 5 || intValue.Value == 7 || intValue.Value == 9)
                    return intValue.Value;
            }
            return null;
        }

    }
}
