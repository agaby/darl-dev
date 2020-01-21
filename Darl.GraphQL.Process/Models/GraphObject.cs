using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class GraphObject : GraphElement
    {

        public string firstname { get; set; }//optional
        public string secondname { get; set; }//optional
        public List<GraphConnection> connections { get; set; } = new List<GraphConnection>();
   }
}
