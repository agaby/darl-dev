using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Process.Models.Alexa
{
    public class Intent
    {
        public string name { get; set; }

        public List<Slot> slots { get; set; } = new List<Slot>();

        public List<string> samples { get; set; } = new List<string>();
    }
}
