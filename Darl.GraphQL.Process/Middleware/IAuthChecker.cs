using System.Threading.Tasks;

namespace Darl.GraphQL.Process.Middleware
{
    public interface IAuthChecker
    {
        Task<bool> AuthorizedAdmin();
    }
}
