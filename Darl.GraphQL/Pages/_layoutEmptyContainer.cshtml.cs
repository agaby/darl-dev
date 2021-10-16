using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;

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

