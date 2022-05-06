using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Stripe;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Darl.GraphQL
{
    [Route("api/[controller]")]
    [ApiController]
    public class webhookController : ControllerBase
    {

        private readonly IHttpContextAccessor _context;
        private readonly IProducts _products;
        private readonly IKGTranslation _trans;
        private readonly IConfiguration _config;

        public webhookController(IHttpContextAccessor context, IProducts prod, IKGTranslation trans, IConfiguration config)
        {
            _context = context;
            _products = prod;
            _trans = trans;
            _config = config;
        }

        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    json,
                    Request.Headers["Stripe-Signature"],
                    _config["AppSettings:StripeWebHookSecret"],
                    throwOnApiVersionMismatch: false
                );
                Console.WriteLine($"Webhook notification with type: {stripeEvent.Type} found for {stripeEvent.Id}");
            }
            catch (Exception e)
            {
                Console.WriteLine($"Something failed {e}");
                return BadRequest();
            }

            if (stripeEvent.Type == "invoice.payment_succeeded")
            {
                var invoice = stripeEvent.Data.Object as Invoice;
                if (invoice != null)
                {
                    if (invoice.BillingReason == "subscription_create")
                    {
                        var service = new PaymentIntentService();
                        var paymentIntent = service.Get(invoice.PaymentIntentId);
                        var options = new SubscriptionUpdateOptions
                        {
                            DefaultPaymentMethod = paymentIntent.PaymentMethodId,
                        };
                        var subscriptionService = new SubscriptionService();
                        subscriptionService.Update(invoice.SubscriptionId, options);
                    }
                    //mark the user as "paying".
                    await _trans.UpdateUserAccountState(invoice.CustomerId, DarlUser.AccountState.paying);
                }
            }

            if (stripeEvent.Type == "invoice.paid")
            {
                // Used to provision services after the trial has ended.
                // The status of the invoice will show up as paid. Store the status in your
                // database to reference when a user accesses your service to avoid hitting rate
                // limits.
            }
            if (stripeEvent.Type == "invoice.payment_failed")
            {
                // If the payment fails or the customer does not have a valid payment method,
                // an invoice.payment_failed event is sent, the subscription becomes past_due.
                // Use this webhook to notify your user that their payment has
                // failed and to retrieve new card details.
            }
            if (stripeEvent.Type == "invoice.finalized")
            {
                // If you want to manually send out invoices to your customers
                // or store them locally to reference to avoid hitting Stripe rate limits.
            }
            if (stripeEvent.Type == "customer.subscription.deleted")
            {
                var subs = stripeEvent.Data.Object as Subscription;
                if (subs != null)
                {
                    var state = await _trans.GetUserAccountState(subs.CustomerId);
                    if (state != DarlUser.AccountState.admin)
                    {
                        await _trans.UpdateUserAccountState(subs.CustomerId, DarlUser.AccountState.closed);
                    }
                }
            }
            if (stripeEvent.Type == "customer.subscription.trial_will_end")
            {
                // Send notification to your user that the trial will end
            }

            return Ok();
        }

        public ActionResult<string> Get()
        {
            return new ActionResult<string>("hello");
        }
    }
}
