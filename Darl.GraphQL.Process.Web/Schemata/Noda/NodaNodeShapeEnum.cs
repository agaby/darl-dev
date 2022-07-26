using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata.Noda
{
    public class NodaNodeShapeEnum : EnumerationGraphType
    {
        public NodaNodeShapeEnum()
        {
            Name = "NodaNodeShape";
            Add("Ball", 0, "Ball");
            Add("Box", 1, "Box");
        }
    }
}
