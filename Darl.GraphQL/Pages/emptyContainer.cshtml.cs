using Darl.GraphQL.Models.Connectivity;
using Darl.Thinkbase;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.IO;
using System.Threading.Tasks;

namespace Darl.GraphQL.Pages
{
    public class EmptyContainerModel : PageModel
    {
        [BindProperty]
        public IFormFile Upload { get; set; }

        private IConfiguration _config;
        private IBlobConnectivity _blob;
        IConnectivity _conn;
        public EmptyContainerModel(IConfiguration config, IBlobConnectivity blob, IConnectivity conn)
        {
            _config = config;
            _conn = conn;
            _blob = blob;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            using (var memStream = new MemoryStream())
            {
                await Upload.CopyToAsync(memStream);
                memStream.Position = 0;
                var bytes = memStream.ToArray();
                //check validity of model
                var model = BlobGraphContent.Load(bytes);
                var userId = _config["SINGLEUSERID"];
                if(model != null && model.licensed)
                {
                    await _blob.Write(Upload.FileName, bytes);

                    foreach(var s in await _conn.GetKGraphsAsync(userId))
                    {
                        await _conn.DeleteKGraph(userId,s.Name);
                        await _conn.DeleteAllKnowledgeStates(userId, s.Name);
                    }
                    await _conn.CreateKGraph(userId, Upload.FileName);
                }
                else
                {
                    ModelState.AddModelError("Upload", "The file is invalid or unlicensed. ");
                    return BadRequest(ModelState);
                }
                return RedirectToPage("/index");
            }
        }

        public IActionResult OnGet()
        {
            var fp = _config["BLOBFILEPATH"];
            if (_blob.Exists(fp).Result)
            {
                return Redirect("index");
            }
            return new PageResult();
        }
    }
}
