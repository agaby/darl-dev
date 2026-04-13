/// </summary>

﻿using System;
using static Darl.GraphQL.Models.Models.DarlUser;

namespace Darl.GraphQL.Models.Models
{
    public class DarlUserUpdate
    {
        /// Date of upgrade
        /// </summary>
        public DateTime? PaidUsageStarted { get; set; }
        /// Person to authorize invoice
        /// </summary>
        public string InvoiceName { get; set; }

        /// Organization to invoice
        /// </summary>
        public string InvoiceOrganization { get; set; }

        /// Email to send invoice
        /// </summary>
        public string InvoiceEmail { get; set; }

        public AccountState? accountState { get; set; }
        /// end of current subscription period.
        /// </summary>
        public DateTime current_period_end { get; set; }

        public string StripeCustomerId { get; set; }

        public string UsageStripeSubscriptionItem { get; set; }

        public string apiKey { get; set; }

        public SubscriptionType? subscriptionType { get; set; }

    }
}
