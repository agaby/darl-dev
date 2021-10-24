using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata.Noda
{
    public class NodaLinkShapeEnum : EnumerationGraphType
    {
        public NodaLinkShapeEnum()
        {
            Name = "NodaLinkShape";
            AddValue("Solid", "Solid", 0);
            AddValue("Dash", "Dash", 1);
        }
    }
}
