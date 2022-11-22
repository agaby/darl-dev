using Darl.GraphQL.Models.Schemata;
using Darl.Thinkbase.Meta;
using DarlCompiler.Parsing;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Schemata
{
    public class SourceSpanType : ObjectGraphType<SourceSpan>
    {
        public SourceSpanType()
        {
            Name = "sourceSpan";
            Description = "The location and length of a section of darl code";
            Field(c => c.Length);
            Field<SourceLocationType>("intents", resolve: c => c.Source.Location);

        }
    }
}
