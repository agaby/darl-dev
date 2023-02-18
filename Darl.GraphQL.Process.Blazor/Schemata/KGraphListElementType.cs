using Darl.GraphQL.Process.Blazor.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ThinkBase.ComponentLibrary.Models;

namespace Darl.GraphQL.Process.Blazor.Schemata
{
    public class KGraphListElementType : ObjectGraphType<KGraphListElement>
    {
        public KGraphListElementType()
        {
            Name = "KGraphListElement";
            Description = "A KGraph name and the user group";
            Field(c => c.name).Description("The name of the KG");
            Field<KGSourceEnum>("kgSource").Description("The user group of the KG").Resolve(c => c.Source.kgSource);
        }
    }
}
