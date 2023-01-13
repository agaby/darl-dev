using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class InferenceRecordType : ObjectGraphType<InferenceRecord>
    {
        public InferenceRecordType()
        {
            Name = "inferenceRecord";
            Description = "A record containing the results of an inference performed using a knowledge graph";
            Field(c => c.confidence).Description("The confidence [0,1] of the dominant inferred output");
            Field(c => c.unknown).Description("The status of the dominant output. True signifies that it was not possible to infer that output.");
            Field<GraphObjectType>("source").Description("the start object for the inference, annotated with any inferred results.").Resolve(c => c.Source.source);
            Field<ListGraphType<StringStringPairType>>("recommendations").Description("Values that would need to be supplied before an inference could be formed, along with their salience. ").Resolve(c => c.Source.recommendations);
        }
    }
}
