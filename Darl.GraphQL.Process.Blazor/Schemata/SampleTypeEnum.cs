using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class SampleTypeEnum : EnumerationGraphType
    {

        public SampleTypeEnum()
        {
            Name = "sampleType";
            Add("events", 0, "The data consists of irregular events");
            Add("sampled", 1, "The data consists of regularly sampled items");
        }
    }
}
