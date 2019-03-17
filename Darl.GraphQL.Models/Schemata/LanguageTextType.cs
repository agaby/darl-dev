using Darl.GraphQL.Models.Services;
using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class LanguageTextType : ObjectGraphType<LanguageText>
    {
        public LanguageTextType()
        {
            Field(c => c.Name);
            Field(c => c.Text);
            Field<ListGraphType<VariantTextType>>("variantlist", resolve: context => context.Source.VariantList);
        }
    }
}
