using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Darl.GraphQL.Pages
{
    public partial class GraphEditModel : PageModel
    {
        [BindProperty]
        public IFormFile Upload { get; set; }


    }

}

