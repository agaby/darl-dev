/// </summary>

﻿using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlMineReportType : ObjectGraphType<Thinkbase.Meta.DarlMineReport>
    {
        public DarlMineReportType()
        {
            Name = "DarlMineReport";
            Description = "Code and results of a DARL training run.";
            Field(c => c.code, true);
            Field(c => c.errorText, true);
            Field(c => c.testPerformance);
            Field(c => c.trainPercent);
            Field(c => c.trainPerformance);
            Field(c => c.unknownResponsePercent);
        }
    }
}
