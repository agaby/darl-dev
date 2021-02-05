using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public enum DarlSeason { winter,spring,summer,fall};
    public class DarlTimeInput
    {
        public double raw { get; set; }

        public double precision { get; set; }

        public DateTime dateTime { get; set; }

        public int year { get; set; }

        public DarlSeason season { get; set; }
    }
}
