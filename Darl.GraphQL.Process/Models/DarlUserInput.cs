/// </summary>

﻿using System;

namespace Darl.GraphQL.Models.Models
{
    public class DarlUserInput
    {
        public string userId { get; set; }

        /// The issuer = Tenant in AD if part of a corporate log in
        /// </summary>
        public string Issuer { get; set; } = string.Empty;
        /// Start date of account
        /// </summary>
        public DateTime Created { get; set; } = DateTime.Now;
        /// Date of upgrade
        /// </summary>
        public DateTime PaidUsageStarted { get; set; } = DateTime.MaxValue;
        /// Person to authorize invoice
        /// </summary>
        public string InvoiceName { get; set; }

        /// Organization to invoice
        /// </summary>
        public string InvoiceOrganization { get; set; }

        /// Email to send invoice
        /// </summary>
        public string InvoiceEmail { get; set; }

        /// end of current subscription period.
        /// </summary>
        public DateTime current_period_end { get; set; } = DateTime.Now;

        public string StripeCustomerId { get; set; }

        public string UsageStripeSubscriptionItem { get; set; }

        public string APIKey { get; set; }

        public string productId { get; set; }

    }
}
