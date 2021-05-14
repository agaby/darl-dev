using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class DarlProduct
    {
        public string name { get; set; }

        public string description { get; set; }

        public string image { get; set; }

        /// <summary>
        ///price in cents
        /// </summary>
        public long price { get; set; }

        public string currency { get; set; }

        public bool hidden { get; set; }

        public int kgCount { get; set; }

        public int userCount { get; set; }

        public string id { get; set; }

        public string priceId { get; set; }
    }
}
