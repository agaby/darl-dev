using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class BotUsage
    {
        public BotUsage(DateTime date, int count)
        {
            Date = date;
            Count = count;
        }

        public DateTime Date { get; }
        public int Count { get; }
    }
}
