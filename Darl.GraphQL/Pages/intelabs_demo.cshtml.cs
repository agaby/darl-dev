using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Darl.GraphQL.Pages
{
    public class intelabs_demoModel : PageModel
    {
        IConnectivity _conn;
        private IHttpContextAccessor _context;
        public List<DarlProduct> products;
        private IKGTranslation _trans;

        public intelabs_demoModel(IConnectivity conn, IHttpContextAccessor context, IProducts prod, IKGTranslation trans)
        {
            _conn = conn;
            _context = context;
            products = prod.products;
            _trans = trans;
        }
        public IActionResult OnGet()
        {
            return new PageResult();
        }
    }
}
