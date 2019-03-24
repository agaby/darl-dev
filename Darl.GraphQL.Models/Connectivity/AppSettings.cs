using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.GraphQL
{
    public class AppSettings
    {
        public string StorageConnectionString { get; set; }
        public string boaiuserid { get; set; }
        public string modelcontainer { get; set; }
        public string rulecontainer { get; set; }
        public string docscontainer { get; set; }
        public string collateralcontainer { get; set; }
        public string mlmodelcontainer { get; set; }
        public string newscontentcontainer { get; set; }
        public string mailcontentcontainer { get; set; }
    }
}
