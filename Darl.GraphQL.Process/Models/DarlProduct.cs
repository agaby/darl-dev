/// </summary>

﻿namespace Darl.GraphQL.Models.Models
{
    public class DarlProduct
    {
        public string name { get; set; } = string.Empty;

        public string description { get; set; } = string.Empty;

        public string? image { get; set; }

        ///price in cents
        /// </summary>
        public long price { get; set; } = 0L;

        public string currency { get; set; } = "USD";

        public bool hidden { get; set; } = true;

        public int kgCount { get; set; }

        public int userCount { get; set; }

        public string? id { get; set; }

        public string? priceId { get; set; }
    }
}
