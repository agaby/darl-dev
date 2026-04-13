/// <summary>
/// </summary>

using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Darl.GraphQL.Pages
{
    public class subscribeModel : PageModel
    {


        private readonly IHttpContextAccessor _context;
        private readonly IProducts _products;
        private readonly IKGTranslation _trans;
        private readonly IConfiguration _config;
        public List<DarlProduct> products;
        public string? SubscriptionId;
        public string? ClientSecret;
        public string StripePublicKey;
        public string? userName;
        public string? product;
        public string? price;
        public string? currency;


        public subscribeModel(IHttpContextAccessor context, IProducts prod, IKGTranslation trans, IConfiguration config)
        {
            _context = context;
            _products = prod;
            _trans = trans;
            _config = config;
            products = prod.products;
            StripePublicKey = _config["AppSettings:StripePublicKey"];
        }
        public async Task<IActionResult> OnGet(string priceId)
        {
            if (string.IsNullOrEmpty(priceId))
                return Redirect("/index");
            if (_context.HttpContext.User == null || _context.HttpContext.User.Identity == null)
                return Redirect("/index");
            var user = await _trans.GetUserById(_context.HttpContext.User.Identity.Name ?? "");
            if (user == null)
                return Redirect("/index");
            userName = user.InvoiceName;
            var prod = products.FirstOrDefault(a => a.priceId == priceId);
            if (prod == null)
                return Redirect("/index");
            product = prod.name;
            currency = prod.currency;
            price = (((double)prod.price) / 100).ToString("0.00");
            // Create subscription
            var subscriptionOptions = new SubscriptionCreateOptions
            {
                Customer = user.StripeCustomerId,
                Items = new List<SubscriptionItemOptions>
                {
                    new SubscriptionItemOptions
                    {
                        Price = priceId,
                    },
                },
                PaymentBehavior = "default_incomplete",
            };
            subscriptionOptions.AddExpand("latest_invoice.payment_intent");
            var subscriptionService = new SubscriptionService();
            try
            {
                Subscription subscription = subscriptionService.Create(subscriptionOptions);
                SubscriptionId = subscription.Id;
                ClientSecret = subscription.LatestInvoice.PaymentIntent.ClientSecret;
            }
            catch (StripeException e)
            {
                Console.WriteLine($"Failed to create subscription.{e}");
            }
            return Page();
        }
    }
}
