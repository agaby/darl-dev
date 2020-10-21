using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Connectivity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Darl.GraphQL.Pages
{
    public class GraphEditModel : PageModel
    {
        IConnectivity _conn;
        private IHttpContextAccessor _context;

        public GraphEditModel(IConnectivity conn, IHttpContextAccessor context)
        {
            _conn = conn;
            _context = context;
        }
        public IActionResult OnGet()
        {
            if(!_context.HttpContext.User.IsInRole("Corp") && !_context.HttpContext.Request.QueryString.HasValue)
            {
                return Redirect("https://darl.ai/thinkbase");
            }
            return new PageResult();
        }
    }
}
