using DarlCompiler.Parsing;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class SourceLocationType : ObjectGraphType<SourceLocation>
    {
        public SourceLocationType()
        {
            Name = "sourceLocation";
            Description = "The location of a section of darl code";
            Field(c => c.Position);
            Field(c => c.Column);
            Field(c => c.Line);

        }
    }
}
