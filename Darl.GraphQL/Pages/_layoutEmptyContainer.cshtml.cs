using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;

namespace Darl.GraphQL.Pages
{
    public class _layoutEmptyContainerModel : PageModel
    {
        [BindProperty]
        public IFormFile Upload { get; set; }

        public void OnGet()
        {
        }
        public async Task OnPostAsync()
        {
            /*            using (var fileStream = new FileStream(_config["BLOBFILEPATH"] + Upload.FileName, FileMode.Create))
                        {
                            await Upload.CopyToAsync(fileStream);
                        }*/
        }


    }

}

