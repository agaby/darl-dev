using Darl.GraphQL.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class GraphObjectInput : GraphElementInput
    {
        public string firstname { get; set; }//optional
        public string secondname { get; set; }//optional
    }
}
