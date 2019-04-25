using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class UserUsage
    {
        public UserUsage(DateTime date, int count)
        {
            Date = date;
            Count = count;
        }

        public DateTime Date { get; }
        public int Count { get; }
    }
}
