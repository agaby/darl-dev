using ProtoBuf;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class MatchModel
    {
        public string Name { get; set; }
        public byte[] Model { get; set; }
        public string userId { get; set; }


    }
}
