
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class PercentGraphType : IntGraphType
    {
        public PercentGraphType()
        {
            Name = "Percent";
        }

        public override object ParseValue(object value)
        {
            if (value == null)
            {
                return null;
            }

            if (int.TryParse(value.ToString(), out var intResult))
            {
                if (intResult >= 0 && intResult <= 100)
                    return intResult;
            }
            return null;
        }
        /*        public override object ParseLiteral(GraphQLValue value)
                {
                    if (value is GraphQLIntValue intValue)
                    {
                        if (intValue.Value >= 0 && intValue.Value <= 100)
                            return intValue.Value;
                    }
                    return null;
                }*/

    }
}
