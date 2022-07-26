using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata.Noda
{
    public class NodaLinkShapeEnum : EnumerationGraphType
    {
        public NodaLinkShapeEnum()
        {
            Name = "NodaLinkShape";
            Add("Solid", 0, "Solid");
            Add("Dash", 1, "Dash");
        }
    }
}
