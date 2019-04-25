using System;
using System.Collections.Generic;
using System.Text;
using static Darl.GraphQL.Models.Models.DarlUser;

namespace Darl.GraphQL.Models.Models
{
    public class DarlUserInput
    {
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

        public AccountState accountState { get; set; } = AccountState.trial;
        /// <summary>
        /// end of current subscription period.
        /// </summary>
        public DateTime current_period_end { get; set; }

        public string StripeCustomerId { get; set; }

        public string UsageStripeSubscriptionItem { get; set; }
    }
}
