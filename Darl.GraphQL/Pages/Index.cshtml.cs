/// <summary>
/// Index.cshtml.cs - Core module for the Darl.dev project.
/// </summary>

using Darl.GraphQL.Models.Connectivity;
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
    public partial class GraphEditModel : PageModel
    {
        public List<DarlProduct> products;
        private readonly IConfiguration _config;
        private IKGTranslation _tran;
        public string filepath;
        private bool uninitialized = false;

        public GraphEditModel(IProducts prod, IConfiguration config, IBlobConnectivity _conn, IKGTranslation tran)
        {
            products = prod.products;
            _config = config;
            _tran = tran;
            if (_config["DOTNET_RUNNING_IN_CONTAINER"] == "true")
            {
                var fp = _config["BLOBFILEPATH"];
                if (_conn.Exists(fp).Result)
                {
                    filepath = fp;
                }
                else
                {
                    uninitialized = true;
                }
            }
            filepath = string.Empty;
        }

        public async Task<IActionResult> OnGet()
        {
            if (uninitialized)
                return Redirect("emptyContainer");
            return new PageResult();
        }

        public async Task<ActionResult> OnPostAsync()
        {
            if (User.Identity != null && User.Identity.Name != null)
            {
                var user = await _tran.GetUserById(User.Identity.Name);
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
