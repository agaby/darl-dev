using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Darl.GraphQL.Pages
{
    public class thinkbasevrModel : PageModel
    {
        public List<DarlProduct> products;
        public thinkbasevrModel(IProducts prod)
        {
            products = prod.products;
        }
        public void OnGet()
        {
        }
    }
}
