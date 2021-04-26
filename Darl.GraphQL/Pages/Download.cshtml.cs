using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Darl.GraphQL.Models.Connectivity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;

namespace Darl.GraphQL.Pages
{
    public class DownloadModel : PageModel
    {
        public enum DownloadType { collateral,mlmodel,botmodel,ruleset }

        [BindProperty, Display(Name = "Type of model")]
        public DownloadType downloadType { get; set; }

        [BindProperty, Display(Name = "Name of model")]
        public string modelName { get; set; } = "far_left.model";

        [BindProperty, Display(Name = "Api Key")]
        public string? apiKey { get; set; }

        private IConnectivity _connectivity;

        public DownloadModel(IConnectivity connectivity)
        {
            _connectivity = connectivity;
        }


        public async Task<ActionResult> OnPostAsync()
        {           
            if (string.IsNullOrEmpty(modelName) && downloadType != DownloadType.collateral)
                return new OkResult();
            string userId = "";
            if (string.IsNullOrEmpty(User.Identity.Name))
            {
                if (apiKey != null) //look up userId
                {
                    var user = await _connectivity.GetUserByApiKey(apiKey);
                    userId = user.userId;
                }
                else
                {
                    return new BadRequestResult(); //no way to get userId
                }
            }
            else
            {
                userId = User.Identity.Name;
            }
            switch (downloadType)
            {
                case DownloadType.botmodel:
                    {
                        var bm = await _connectivity.GetBotModel(userId, modelName);
                        return File(bm.Model, "application/octet-stream", modelName);
                    }
                case DownloadType.collateral:
                    {
                        //get all the collateral bundled
                        var cols = await _connectivity.GetCollaterals(userId);
                        using(var ms = new MemoryStream())
                        {
                            var writer = new StreamWriter(ms);
                            writer.Write(JsonConvert.SerializeObject(cols));
                            writer.Flush();
                            ms.Position = 0;
                            return File(ms, "application/json");
                        }                      
                    }
                case DownloadType.ruleset:
                    {
                        //get all the collateral bundled
                        var rs = await _connectivity.GetRuleSet(userId,modelName);
                        using (var ms = new MemoryStream())
                        {
                            var writer = new StreamWriter(ms);
                            writer.Write(JsonConvert.SerializeObject(rs.Contents));
                            writer.Flush();
                            ms.Position = 0;
                            return File(ms, "application/json");
                        }
                    }
                case DownloadType.mlmodel:
                    {
                        //get all the collateral bundled
                        var rs = await _connectivity.GetMlModel(userId, modelName);
                        using (var ms = new MemoryStream())
                        {
                            var writer = new StreamWriter(ms);
                            writer.Write(JsonConvert.SerializeObject(rs.model));
                            writer.Flush();
                            ms.Position = 0;
                            return File(ms, "application/json");
                        }
                    }
            }
            return new OkResult();
        }

 /*       public async Task<ActionResult> PostChoices(string dType)
        {
            DownloadType dt = (DownloadType)Enum.Parse(typeof(DownloadType),dType);
            string userId = "";
            if (string.IsNullOrEmpty(User.Identity.Name))
            {
                if (apiKey != null) //look up userId
                {
                    var user = await _connectivity.GetUserByApiKey(apiKey);
                    userId = user.userId;
                }
                else
                {
                    return new OkResult(); //no way to get userId
                }
            }
            else
            {
                userId = User.Identity.Name;
            }
            switch (dt)
            {
                case DownloadType.collateral:
                    return new OkObjectResult( new List<string> {"All collateral"});
                case DownloadType.botmodel:
                    return new OkObjectResult((await _connectivity.GetBotModelsAsync(userId)).Select(a => a.Name).ToList());
                case DownloadType.mlmodel:
                    return new OkObjectResult((await _connectivity.GetMlModelsAsync(userId)).Select(a => a.Name).ToList());
                case DownloadType.ruleset:
                    return new OkObjectResult((await _connectivity.GetRuleSetsAsync(userId)).Select(a => a.Name).ToList());
            }
            return new OkResult();
        }*/
    }
}