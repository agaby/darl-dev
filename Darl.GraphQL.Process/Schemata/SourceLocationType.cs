using DarlCompiler.Parsing;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Schemata
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
