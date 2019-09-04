using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Process.Middleware
{
    public interface IAuthChecker
    {
        Task<bool> AuthorizedAdmin();
    }
}
