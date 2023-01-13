using DarlCompiler.Parsing;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class SourceSpanType : ObjectGraphType<SourceSpan>
    {
        public SourceSpanType()
        {
            Name = "sourceSpan";
            Description = "The location and length of a section of darl code";
            Field(c => c.Length);
            Field<SourceLocationType>("location").Resolve(c => c.Source.Location);

        }
    }
}
