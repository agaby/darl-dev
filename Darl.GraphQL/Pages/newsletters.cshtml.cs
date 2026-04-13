/// <summary>
/// </summary>

using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Markdig;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Pages
{

    public class newslettersModel : PageModel
    {
        public List<DarlProduct> products;
        private readonly IConfiguration _config;
        private IKGTranslation _conn;
        public string newsletter { get; set; } = string.Empty;

        public newslettersModel(IProducts prod, IConfiguration config, IKGTranslation conn)
        {
            products = prod.products;
            _config = config;
            _conn = conn;
        }

        public async Task<ActionResult> OnGet(string newsletter)
        {
            //fetch newletter text
            var md = await _conn.GetNewsLetter(newsletter);
            if (md == null)
            {
                //return 404
                return NotFound();
            }
            //convert to HTML
            var pipeline = new MarkdownPipelineBuilder().UseAdvancedExtensions().Build();
            var result = Markdown.ToHtml(md, pipeline);
            return Content(result, "text/html");
        }
    }
}
