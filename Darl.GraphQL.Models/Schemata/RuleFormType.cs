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
            //Field(c => c.language);
            Field(c => c.license);
            Field(c => c.name);
            Field(c => c.price);
            Field(c => c.testData);
            Field(c => c.version);
            //Field(c => c.trigger);
            //Field(c => c.format);

        }
    }
}
