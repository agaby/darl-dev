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
            Field(c => c.Language);
            Field(c => c.Text);
        }
    }
}

