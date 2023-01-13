namespace Darl.GraphQL.Process.Blazor.Models
{
    public enum DarlSeason { winter, spring, summer, fall };
    public class DarlTimeInput
    {
        public double raw { get; set; }

        public double precision { get; set; }

        public DateTimeOffset dateTimeOffset { get; set; }

        public int year { get; set; }

        public DarlSeason season { get; set; }
    }
}
