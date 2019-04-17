using Darl.GraphQL.Models.Models;
using GraphQL.Types;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Schemata
{
    public class MLResultType : ObjectGraphType<MLResult>
    {
        public MLResultType()
        {
            Name = "mlResult";
            Field(c => c.executionDate);
            Field(c => c.executionTime);
            Field(c => c.inSamplePercent);
            Field(c => c.inSamplePercentUnknown);
            Field(c => c.inSampleRMSError);
            Field(c => c.outSamplePercent);
            Field(c => c.outSamplePercentUnknown);
            Field(c => c.outSampleRMSError);
            Field(c => c.percentTest);
        }
    }
}
