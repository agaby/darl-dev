/// </summary>

﻿using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class SetGraphType : IntGraphType
    {
        public SetGraphType()
        {
            Name = "SetGraphType";
        }

        public override object ParseValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (int.TryParse(value.ToString(), out var intResult))
            {
                if (intResult == 3 || intResult == 5 || intResult == 7 || intResult == 9)
                    return intResult;
            }
            return null;
        }

    }
}
