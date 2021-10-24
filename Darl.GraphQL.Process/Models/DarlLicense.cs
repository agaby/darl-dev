using System;

namespace Darl.GraphQL.Models.Models
{
    public class DarlLicense
    {
        public string licensekey { get; set; }
        public DateTime created { get; set; } = DateTime.UtcNow;
        public DateTime terminates { get; set; } = DateTime.MaxValue;

    }
}
