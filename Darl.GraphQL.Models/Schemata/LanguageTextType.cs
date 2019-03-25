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
            Name = "LanguageText";
            Description = "A single text.";
            Field(c => c.Name);
            Field(c => c.Text);
            Field<ListGraphType<VariantTextType>>("variantlist", resolve: context => context.Source.VariantList);
        }
    }
}
