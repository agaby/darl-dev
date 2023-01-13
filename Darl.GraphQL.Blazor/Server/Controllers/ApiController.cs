using Darl.GraphQL.Blazor.Server.Helpers;
using GraphQL;
using GraphQL.Instrumentation;
using GraphQL.Server.Transports.AspNetCore;
using GraphQL.SystemTextJson;
using GraphQL.Transport;
using GraphQL.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Darl.GraphQL.Blazor.Server.Controllers
{
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
