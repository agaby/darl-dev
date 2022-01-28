using Darl.Thinkbase;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Schemata
{
    public  class VRDisplayAttType : ObjectGraphType<VRDisplayAtt>
    {
        public VRDisplayAttType()
        {
            Name = "vrDisplayAtt";
            Description = "A simplified version of a knowledge graph attribute for VR display purposes";
            Field(c => c.name);
            Field(c => c.value,true);
            Field(c => c.lineage);
            Field(c => c.confidence,true).DefaultValue(1.0);
        }
    }
}
