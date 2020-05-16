using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;
using static Darl.GraphQL.Models.Connectivity.IGraphProcessing;

namespace Darl.GraphQL.Models.Schemata
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
