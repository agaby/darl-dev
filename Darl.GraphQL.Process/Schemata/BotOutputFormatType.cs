using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class BotOutputFormatType : ObjectGraphType<BotOutputFormat>
    {
        public BotOutputFormatType()
        {
            Name = "BotOutputFormat";
            Description = "Format of a response presented to a bot user";
            Field<ListGraphType<StringGraphType>>("categories", resolve: context => context.Source.Categories);
            Field<DisplayTypeEnum>("displayType", resolve: context => context.Source.displayType);
            Field(c => c.Name);
            Field<OutputTypeEnum>("outputType", resolve: context => context.Source.OutputType);
            Field<ListGraphType<SetDefinitionType>>("sets", resolve: context => context.Source.Sets);
            Field(c => c.ValueFormat,true);
        }
    }
}
