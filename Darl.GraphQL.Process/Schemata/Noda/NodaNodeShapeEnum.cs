using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata.Noda
{
    public class NodaNodeShapeEnum : EnumerationGraphType
    {
        public NodaNodeShapeEnum()
        {
            Name = "NodaNodeShape";
            AddValue("Ball", "Ball", 0);
            AddValue("Box", "Box", 1);
        }
    }
}
