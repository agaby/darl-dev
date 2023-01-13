using Darl.Lineage;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class MatchedAnnotationType : ObjectGraphType<MatchedAnnotation>
    {
        public MatchedAnnotationType()
        {
            Name = "matchedAnnotation";
            Field<LineageAnnotationNodeType>("annotation").Resolve(c => c.Source.annotation);
            Field(c => c.depth);
            Field(c => c.path);
            Field<ListGraphType<DarlVarType>>("values").Resolve(c => c.Source.values);
        }
    }
}
