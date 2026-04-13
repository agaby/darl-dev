/// <summary>
/// OntologyActionEnum.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Thinkbase;
using GraphQL.Types;

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
