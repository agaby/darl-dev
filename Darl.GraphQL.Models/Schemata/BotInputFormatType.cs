using Darl.Lineage;
using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class BotInputFormatType : ObjectGraphType<BotInputFormat>
    {
        public BotInputFormatType()
        {
            Field<ListGraphType<StringGraphType>>("categories", resolve: context => context.Source.Categories);
            Field(c => c.EnforceCrisp).DefaultValue(false);
            Field(c => c.Increment).DefaultValue(0.0);
            Field<InputTypeEnum>("inputType", resolve: context => context.Source.InType);
            Field(c => c.MaxLength).DefaultValue(0);
            Field(c => c.Name);
            Field(c => c.NumericMax).DefaultValue(0.0);
            Field(c => c.NumericMin).DefaultValue(0.0);
            Field(c => c.Regex).DefaultValue(string.Empty);
            Field<ListGraphType<SetDefinitionType>>("sets", resolve: context => context.Source.Sets);
            Field(c => c.ShowSets).DefaultValue(false); ;
        }
    }
}
