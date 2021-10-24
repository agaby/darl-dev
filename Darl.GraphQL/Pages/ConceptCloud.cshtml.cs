using Darl.GraphQL.Models.Connectivity;
using Darl.GraphQL.Models.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace Darl.GraphQL.Pages
{
    public class ConceptCloudModel : PageModel
    {
        public List<DarlProduct> products;
        private readonly IConfiguration _config;
        public string filepath;

        public ConceptCloudModel(IProducts prod, IConfiguration config, IBlobConnectivity _conn)
        {
            products = prod.products;
            _config = config;
            if (_config["DOTNET_RUNNING_IN_CONTAINER"] == "true")
            {
                var fp = _config["BLOBFILEPATH"];
                if (_conn.Exists(fp).Result)
                {
                    filepath = fp;
                }
            }
            filepath = string.Empty;
        }
        public void OnGet()
        {
        }
    }
}
