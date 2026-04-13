/// <summary>
/// DarlUserInput.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System;

namespace Darl.GraphQL.Models.Models
{
    public class DarlUserInput
    {
        public string userId { get; set; }

        /// <summary>
        /// The issuer = Tenant in AD if part of a corporate log in
        /// </summary>
        public string Issuer { get; set; } = string.Empty;
        /// <summary>
        /// Start date of account
        /// </summary>
        public DateTime Created { get; set; } = DateTime.Now;
        /// <summary>
        /// Date of upgrade
        /// </summary>
        public DateTime PaidUsageStarted { get; set; } = DateTime.MaxValue;
        /// <summary>
        /// Person to authorize invoice
        /// </summary>
        public string InvoiceName { get; set; }

        /// <summary>
        /// Organization to invoice
        /// </summary>
        public string InvoiceOrganization { get; set; }

        /// <summary>
        /// Email to send invoice
        /// </summary>
        public string InvoiceEmail { get; set; }

        /// <summary>
        /// end of current subscription period.
        /// </summary>
        public DateTime current_period_end { get; set; } = DateTime.Now;

        public string StripeCustomerId { get; set; }

        public string UsageStripeSubscriptionItem { get; set; }

        public string APIKey { get; set; }

        public string productId { get; set; }

    }
}
