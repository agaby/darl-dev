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
            Field(c => c.EnforceCrisp);
            Field(c => c.Increment);
            Field<InputTypeEnum>("inputType", resolve: context => context.Source.InType);
            Field(c => c.MaxLength);
            Field(c => c.Name);
            Field(c => c.NumericMax);
            Field(c => c.NumericMin);
            Field(c => c.Regex);
            Field(c => c.Sets);
            Field(c => c.ShowSets);
        }
    }
}
