using Darl.Thinkbase;
using GraphQL.Types;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class OntologyActionEnum : EnumerationGraphType<OntologyAction>
    {
        public OntologyActionEnum()
        {
            Name = "ontologyAction";
            Description = "If build, the ontology is updated based on this, if check this is checked against the ontology, otherwise the ontology is ignored";
        }
    }
}
