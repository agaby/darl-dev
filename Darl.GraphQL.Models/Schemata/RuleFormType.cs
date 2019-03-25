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
            Name = "RuleForm";
            Description = "Questionnaire detail";
            Field(c => c.author,true).DefaultValue(string.Empty);
            Field(c => c.copyright,true).DefaultValue(string.Empty);
            Field(c => c.currency, true).DefaultValue(string.Empty);
            Field(c => c.darl).DefaultValue(string.Empty);
            Field(c => c.description, true).DefaultValue(string.Empty);
            Field<LanguageFormatType>("language", resolve: context => context.Source.language);
            Field(c => c.license, true).DefaultValue(string.Empty);
            Field(c => c.name);
            Field(c => c.price).DefaultValue(0.0);
            Field(c => c.testData, true);
            Field(c => c.version, true).DefaultValue(string.Empty);
            Field<TriggerViewType>("trigger", resolve: context => context.Source.trigger);
            //Field(c => c.format);

        }
    }
}
