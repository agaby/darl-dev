using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace Darl.GraphQL.Pages
{
    public class voyagerModel : PageModel
    {
        public List<DarlProduct> products;


        public voyagerModel(IProducts prod)
        {
            products = prod.products;
        }

        public void OnGet()
        {

        }
    }
}