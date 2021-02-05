using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class InferenceTimeEnum : EnumerationGraphType<InferenceTime>
    {
        public InferenceTimeEnum()
        {
            Name = "InferenceTime";
            Description = "Determines if inferences are performed using the current time or some fixed time.";
        }

    }
}
