/// <summary>
/// docs.cshtml.cs - Core module for the Darl.dev project.
/// </summary>

using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace Darl.GraphQL.Pages
{
    public class docsModel : PageModel
    {
        public List<DarlProduct> products;

        public docsModel(IProducts prod)
        {
            products = prod.products;
        }
        public void OnGet()
        {
        }
    }
}
