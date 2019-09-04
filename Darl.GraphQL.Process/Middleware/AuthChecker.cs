using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Process.Middleware
{
    public class AuthChecker : IAuthChecker
    {
        private IAuthorizationService _authServ;
        private IHttpContextAccessor _httpContext;
        private Dictionary<string, bool> userCache = new Dictionary<string, bool>();

        public AuthChecker(IAuthorizationService authServ, IHttpContextAccessor httpContext)
        {
            _authServ = authServ;
            _httpContext = httpContext;
        }

        /// <summary>
        /// Check for access rights
        /// </summary>
        /// <param name="policies"></param>
        /// <returns>true if access allowed</returns>
        /// <remarks>defaults to permitting access if no policies exist.</remarks>
        public async Task<bool> AuthorizedAdmin()
        {
            var name = _httpContext.HttpContext.User.Identity.Name;
            if (string.IsNullOrEmpty(name))
                return false;
            if (userCache.ContainsKey(name))
                return userCache[name];
            var r = await _authServ.AuthorizeAsync(_httpContext.HttpContext.User, "AdminPolicy");
            lock (this)
            {
                userCache.Add(name, r.Succeeded);
            }
            return r.Succeeded;
        }
    }
}
