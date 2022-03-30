using GraphQL.Types;

namespace Darl.GraphQL.Models.Schemata
{
    public class DarlMineReportType : ObjectGraphType<Thinkbase.Meta.DarlMineReport>
    {
        public DarlMineReportType()
        {
            Name = "DarlMineReport";
            Description = "Code and results of a DARL training run.";
            Field(c => c.code);
            Field(c => c.errorText);
            Field(c => c.testPerformance);
            Field(c => c.trainPercent);
            Field(c => c.trainPerformance);
            Field(c => c.unknownResponsePercent);
        }
    }
}
