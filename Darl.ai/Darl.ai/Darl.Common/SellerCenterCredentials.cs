/// <summary>
/// </summary>

﻿using System.ComponentModel.DataAnnotations;

namespace DarlCommon
{
    public class SellerCenterCredentials
    {
        [Display(Name = "Merchant ID", Description = "Supplied by the MS seller center")]
        [Required]
        public string MerchantId { get; set; }

        [Display(Name = "Stripe API Key", Description = "Supplied by Stripe")]
        [Required]
        public string StripeApiKey { get; set; }

        [Display(Name = "Live mode", Description = "If true, payments are live, false they are simulated.")]
        [Required]
        public bool LiveMode { get; set; }
    }
}
