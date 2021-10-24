using System;

namespace Darl.GraphQL.Models.Models
{
    public class UserUsage
    {
        public UserUsage(DateTime date, int count)
        {
            Date = date;
            Count = count;
        }

        public DateTime Date { get; set; }
        public int Count { get; set; }
    }
}
