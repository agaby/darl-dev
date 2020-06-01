using Darl.SoftMatch;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class KGMatchResult
    {
        public bool index { get; set; } = false;

        public List<string> valueProperty { get; set; } = new List<string>();

        public List<List<MatchResult>> results { get; set; } = new List<List<MatchResult>>();
    }
}
