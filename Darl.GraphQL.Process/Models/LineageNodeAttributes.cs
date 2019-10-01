using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class LineageNodeAttributes
    {
        public string darl { get; set; } = string.Empty;
        public List<string> implications { get; set; } = new List<string>();

        public List<string> accessRoles { get; set; } = new List<string>();

        public string path { get; set; } = string.Empty;

        public bool randomResponse { get; set; } = false;

        public List<string> randomResponses { get; set; } = new List<string>();

        public string response { get; set; } = string.Empty;

        public string call { get; set; } = string.Empty;

        public bool present { get; set; } = false;


    }
}
