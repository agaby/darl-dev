/// <summary>
/// graphiql.cshtml.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;

namespace Darl.GraphQL.Pages
{
    public class graphiqlModel : PageModel
    {

        public List<DarlProduct> products;

        public graphiqlModel(IProducts prod)
        {
            products = prod.products;
        }

        public void OnGet()
        {

        }
    }
}