/// </summary>

﻿using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.BillingPortal;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Pages
{
    public class IndexModel : PageModel
    {
        private readonly IKGTranslation _connectivity;
        private readonly IConfiguration _config;
        public List<DarlProduct> products;


        public IndexModel(IKGTranslation conn, IConfiguration config, IProducts prod)
        {
            _connectivity = conn;
            _config = config;
            products = prod.products;
        }
        public void OnGet()
        {

        }

        public async Task<ActionResult> OnPostAsync()
        {
            if (User != null && User.Identity != null && User.Identity.Name != null)
            {
                var user = await _connectivity.GetUserById(User.Identity.Name);
                if (!string.IsNullOrEmpty(user.StripeCustomerId))
                {
                    StripeConfiguration.ApiKey = _config["AppSettings:StripeAPIKey"];
                    var options = new SessionCreateOptions
                    {
                        Customer = user.StripeCustomerId,
                        ReturnUrl = "https://darl.dev",
                    };
                    var service = new SessionService();
                    var session = service.Create(options);
                    return Redirect(session.Url);
                }
            }
            return Redirect("Https://darl.dev");
        }
    }
}