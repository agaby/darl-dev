using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class VariantTextType : ObjectGraphType<VariantText>
    {
        public VariantTextType()
        {
            Name = "VariantText";
            Description = "An alternate text in the given language.";
            Field(c => c.Language);
            Field(c => c.Text);
        }
    }
}

