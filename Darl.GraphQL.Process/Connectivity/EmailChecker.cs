using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public class EmailChecker : ICheckEmail
    {

        private IConfiguration _config;

        public EmailChecker(IConfiguration config)
        {
            _config = config;
        }
        public async Task<bool> CheckEmail(string email, string ipaddress = "")
        {
            var zeroBounceAPI = new ZeroBounce.ZeroBounceAPI();
            zeroBounceAPI.api_key = _config["AppSettings:ZeroBounceAPIKey"];
            zeroBounceAPI.RequestTimeOut = 150000; // Any integer value in milliseconds
            zeroBounceAPI.EmailToValidate = email;
            zeroBounceAPI.ip_address = ipaddress;
            var apiProperties = await zeroBounceAPI.ValidateEmailAsync();
            switch (apiProperties.status.ToLower())
            {
                case "unknown":
                case "valid":
                case "catch-all":
                    return true;
                default:
                    return false;
            }
        }
    }
}
