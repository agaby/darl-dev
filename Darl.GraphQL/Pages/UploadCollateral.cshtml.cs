using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Darl.GraphQL.Pages
{
    public class UploadCollateralModel : PageModel
    {
        public enum UploadType { document,collateral}

        [BindProperty, Display(Name = "Type of resource")]
        public UploadType uploadType { get; set; }

        [BindProperty, Display(Name = "File to upload")]
        public IFormFile UploadedFile { get; set; }

        [BindProperty, Display(Name = "Name of resource")]
        public string destinationName { get; set; }

        private IConnectivity _connectivity;

        public UploadCollateralModel(IConnectivity connectivity)
        {
            _connectivity = connectivity;
        }

        public async Task OnPostAsync()
        {
            if (string.IsNullOrEmpty(destinationName) || string.IsNullOrEmpty(User.Identity.Name))
                return;
            switch(uploadType)
            {
                case UploadType.document:
                    using (var ms = new MemoryStream())
                    {
                        var d = new Document { userId = User.Identity.Name, name = destinationName };
                        await UploadedFile.CopyToAsync(ms);
                        d.content = ms.ToArray();
                        await _connectivity.UpdateDocument(d);
                    }
                    break;
                default:
                    using (var ms = new MemoryStream())
                    {
                        await UploadedFile.CopyToAsync(ms);
                        using (var reader = new StreamReader(ms))
                        {
                           await _connectivity.UpdateCollateral(User.Identity.Name, destinationName, reader.ReadToEnd());
                        }
                    }
                    break;
            }

        }
    }
}