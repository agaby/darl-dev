using Darl.Lineage;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class MatchedAnnotationType : ObjectGraphType<MatchedAnnotation>
    {
        public MatchedAnnotationType()
        {
            Name = "matchedAnnotation";
            Field<LineageAnnotationNodeType>("annotation", resolve: c => c.Source.annotation);
            Field(c => c.depth);
            Field(c => c.path);
            Field<ListGraphType<DarlVarType>>("values", resolve: c => c.Source.values);
        }
    }
}
