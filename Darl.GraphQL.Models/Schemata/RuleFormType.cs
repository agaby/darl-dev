using Darl.GraphQL.Models.Services;
using DarlCommon;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class RuleFormType : ObjectGraphType<RuleForm>
    {
        public RuleFormType()
        {
            Field(c => c.author);
            Field(c => c.copyright);
            Field(c => c.currency);
            Field(c => c.darl);
            Field(c => c.description);
            Field<LanguageFormatType>("language", resolve: context => context.Source.language);
            Field(c => c.license);
            Field(c => c.name);
            Field(c => c.price);
            Field(c => c.testData);
            Field(c => c.version);
            Field<TriggerViewType>("trigger", resolve: context => context.Source.trigger);
            //Field(c => c.format);

        }
    }
}
