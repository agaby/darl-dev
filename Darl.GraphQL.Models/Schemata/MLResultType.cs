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
            Field(c => c.code);
            Field(c => c.errorText);
            Field(c => c.testPerformance);
            Field(c => c.trainPercent);
            Field(c => c.trainPerformance);
            Field(c => c.unknownResponsePercent);
        }
    }
}
