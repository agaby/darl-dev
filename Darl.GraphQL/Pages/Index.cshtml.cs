using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Connectivity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Stripe;
using Stripe.BillingPortal;

namespace Darl.GraphQL.Pages
{
    public class IndexModel : PageModel
    {
        private IConnectivity _connectivity;
        private IConfiguration _config;


        public IndexModel(IConnectivity conn, IConfiguration config)
        {
            _connectivity = conn;
            _config = config;
        }
        public void OnGet()
        {

        }

        public  async Task<ActionResult> OnPostAsync()
        {
            var user = await _connectivity.GetUserById(User.Identity.Name);
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
}