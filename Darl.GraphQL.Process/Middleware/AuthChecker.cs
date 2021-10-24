using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Process.Middleware
{
    public class AuthChecker : IAuthChecker
    {
        private readonly IAuthorizationService _authServ;
        private readonly IHttpContextAccessor _httpContext;
        private readonly Dictionary<string, bool> userCache = new Dictionary<string, bool>();

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
                if (!userCache.ContainsKey(name))
                    userCache.Add(name, r.Succeeded);
                else
                    userCache[name] = r.Succeeded;
            }
            return r.Succeeded;
        }
    }
}
