using System;

namespace Darl.GraphQL.Blazor.Client.Models
{
    public class DarlTimeInput
    {
        public enum Season { WINTER, SPRING, SUMMER, FALL }
        public double? raw { get; set; }
        public DateTimeOffset dateTimeOffset { get; set; }
        public double? precision { get; set; }
        public int? year { get; set; }
        public Season? season { get; set; }
    }
}

