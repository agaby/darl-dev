using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class DarlUser
    {
        public ObjectId id { get; set; }
        public enum AccountState { trial, trial_expired, paying, delinquent, suspended, closed, admin }
        public enum SubscriptionType { individual, corporate, embedded, inhouse }
        public string userId { get; set; }

        /// <summary>
        /// The issuer = Tenant in AD if part of a corporate log in
        /// </summary>
        public string Issuer { get; set; } = string.Empty;
        /// <summary>
        /// Start date of account
        /// </summary>
        public DateTime Created { get; set; }
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

        public AccountState? accountState { get; set; }
        /// <summary>
        /// end of current subscription period.
        /// </summary>
        public DateTime current_period_end { get; set; } 

        public string StripeCustomerId { get; set; }

        public string APIKey { get; set; } = Guid.NewGuid().ToString();

        public SubscriptionType? subscriptionType { get; set; }

        public string parentAccount { get; set; }

        public List<string> subUsers { get; set; } = new List<string>();

        public string productId { get; set; }

    }
}
