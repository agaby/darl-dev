using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class LineageNodeAttributeUpdate
    {
        public string darl { get; set; }
        public List<string> implications { get; set; }

        public List<string> accessRoles { get; set; }

        public bool randomResponse { get; set; }

        public List<string> randomResponses { get; set; }

        public string response { get; set; }

        public string call { get; set; }

        public bool present { get; set; }

    }
}
