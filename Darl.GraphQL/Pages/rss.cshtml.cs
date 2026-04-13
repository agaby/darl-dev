/// </summary>

using Darl.GraphQL.Models.Connectivity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Darl.GraphQL.Container.Pages
{
    [ResponseCache(Duration = 1200)]
    public class rssModel : PageModel
    {
        private IKGTranslation _trans;

        public rssModel(IKGTranslation trans)
        {
            _trans = trans;
        }

        public async Task<IActionResult> OnGet()
        {
            return File(await _trans.RenderRSS(HttpContext.Request.Scheme), "application/rss+xml; charset=utf-8");
        }
    }
}
