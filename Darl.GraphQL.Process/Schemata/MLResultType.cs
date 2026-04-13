/// <summary>
/// MLResultType.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class MLResultType : ObjectGraphType<MLResult>
    {
        public MLResultType()
        {
            Name = "mlResult";
            Description = "The results of a machine learning run.";
            Field(c => c.executionDate).Description("Date and time of run");
            Field(c => c.executionTime).Description("Run time in seconds");
            Field(c => c.code).Description("The DARL code generated");
            Field(c => c.errorText, true).Description("Text of any errors"); ;
            Field(c => c.testPerformance, true).Description("The performance on the test set, RMS error for numeric, percentage for categorical");
            Field(c => c.trainPercent).Description("The percentage of the data set used for training");
            Field(c => c.trainPerformance).Description("The performance on the training set, RMS error for numeric, percentage for categorical");
            Field(c => c.unknownResponsePercent).Description("The percentage of results marked as unknown");
        }
    }
}
