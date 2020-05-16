using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class KGMatchResult
    {
        public bool index { get; set; } = false;

        public List<List<MatchResult>> results { get; set; } = new List<List<MatchResult>>();
    }
}
