using Darl.GraphQL.Models.Services;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class MLSpecType : ObjectGraphType<DarlCommon.MLModel>
    {
        public MLSpecType()
        {
            Field(c => c.author);
            Field(c => c.copyright);
            Field(c => c.darl);
            Field(c => c.dataSchema);
            Field(c => c.description);
            Field(c => c.destinationRulesetName);
            Field(c => c.license);
            Field(c => c.name);
            Field(c => c.percentTest);
            Field(c => c.sets);
            Field(c => c.trainData);
            Field(c => c.version);
        }
    }
}
