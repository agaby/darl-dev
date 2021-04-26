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
        public IFormFile? UploadedFile { get; set; }

        [BindProperty, Display(Name = "Name of resource")]
        public string? destinationName { get; set; }

        [BindProperty, Display(Name = "Api Key")]
        public string? apiKey { get; set; }

        private IConnectivity _connectivity;

        public UploadCollateralModel(IConnectivity connectivity)
        {
            _connectivity = connectivity;
        }

        public async Task OnPostAsync()
        {
            if (string.IsNullOrEmpty(destinationName))
                return;
            string userId = "";
            if(string.IsNullOrEmpty(User.Identity.Name))
            {
                if(apiKey != null) //look up userId
                {
                    var user = await _connectivity.GetUserByApiKey(apiKey);
                    userId = user.userId;
                }
                else
                {
                    return; //no way to get userId
                }
            }
            else
            {
                userId = User.Identity.Name;
            }
            switch (uploadType)
            {
                case UploadType.document:
                    using (var ms = new MemoryStream())
                    {
                        var d = new Document { userId = userId, name = destinationName };
                        if (UploadedFile != null)
                        {
                            await UploadedFile.CopyToAsync(ms);
                            d.content = ms.ToArray();
                            await _connectivity.UpdateDocument(d);
                        }
                    }
                    break;
                default:
                    using (var ms = new MemoryStream())
                    {
                        if (UploadedFile != null)
                        {
                            await UploadedFile.CopyToAsync(ms);
                            ms.Position = 0;
                            using (var reader = new StreamReader(ms))
                            {
                                var content = await reader.ReadToEndAsync();
                                await _connectivity.UpdateCollateral(userId, destinationName, content);
                            }
                        }
                    }
                    break;
            }
        }
    }
}