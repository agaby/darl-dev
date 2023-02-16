using Darl.Thinkbase;
using GraphQLParser.AST;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProtoBuf;
using System.IO;
using System.Security.Claims;

namespace Darl.GraphQL.Blazor.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly IGraphProcessing _graph;
        private readonly ILogger<FilesController> _logger;
        private readonly IGraphPrimitives _graphPrimitives;

        public FilesController(IGraphProcessing graph, IGraphPrimitives graphPrimitives, ILogger<FilesController> logger)
        {
            _graph = graph;
            _graphPrimitives = graphPrimitives;
            _logger = logger;
        }

        [HttpPost]
        public async Task<ActionResult> PostFile([FromForm] IEnumerable<IFormFile> files)
        {
            var file = files.FirstOrDefault();
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // will give the user's userId
            if (file == null) { return BadRequest("No file presnt"); }
            try
            {
                BlobGraphContent model;
                using (var rs = file.OpenReadStream())
                {
                    using (var ms = new MemoryStream())
                    {
                        await rs.CopyToAsync(ms);
                        ms.Position = 0;
                        model = ProtoBuf.Serializer.Deserialize<BlobGraphContent>(ms);
                    }
                }
                if (!model.licensed)
                {
                    return BadRequest("This model does not have a valid key.");
                }
                //Check and normalise name
                var invalids = System.IO.Path.GetInvalidFileNameChars();
                var newName = String.Join("_", file.Name.Split(invalids, StringSplitOptions.RemoveEmptyEntries)).TrimEnd('.');
                await _graphPrimitives.Store(userId + '/' + newName, model);
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpGet("{graphName}")]
        public async Task<IActionResult> DownloadFile(string graphName)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); // will give the user's userId
                var model = await _graph.GetModel(userId!, graphName);
                if (model != null)
                {
                    var ms = new MemoryStream();
                    Serializer.Serialize<BlobGraphContent>(ms, model as BlobGraphContent);
                    ms.Position = 0;
                    return File(ms, "application/octetstream", graphName);
                }
            }
            catch (Exception ex)
            {
                return BadRequest($"Error accessing this graph: {ex.Message}");
            }
            return BadRequest($"Graph {graphName} does not exist in this account.");
        }
    }
}
