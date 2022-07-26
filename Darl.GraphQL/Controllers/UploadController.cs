using Darl.GraphQL.Models.Connectivity;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Darl.GraphQL.Controllers
{
    [Route("upload")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly IKGTranslation _trans;

        public UploadController(IKGTranslation trans)
        {
            _trans = trans;
        }

        [HttpPost]
        public async Task<IActionResult> Post(IFormFile file)
        {
            Request.Headers.TryGetValue("authorization", out var authHeader);
            if (authHeader.Count > 0 && authHeader[0].StartsWith("Basic", StringComparison.OrdinalIgnoreCase))
            {
                var token = authHeader[0].Substring("Basic ".Length).Trim();
                var du = await _trans.GetUserByApiKey(token);
                if (du == null)// can indicate user is barred
                {
                    return Forbid();
                }
                try
                {
                    await _trans.CreateTempKG(du.userId, file.FileName, file);
                }
                catch
                {
                    return NoContent();
                }
                return Ok(new { size = file.Length });
            }
            return Forbid();
        }
    }
}
