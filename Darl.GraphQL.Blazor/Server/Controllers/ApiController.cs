using GraphQL;
using GraphQL.Server.Transports.AspNetCore;
using Microsoft.AspNetCore.Mvc;

namespace Darl.GraphQL.Blazor.Server.Controllers
{
    [Authorize]
    [ApiController]
    [Route("graphql")]
    public class ApiController : Controller
    {

        [HttpGet]
        public IActionResult Get()
               => new GraphQLExecutionActionResult();

        [HttpPost]
        public IActionResult Post()
                => new GraphQLExecutionActionResult();
    }
}
