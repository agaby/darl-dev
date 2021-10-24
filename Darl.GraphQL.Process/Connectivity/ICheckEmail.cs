using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface ICheckEmail
    {
        Task<bool> CheckEmail(string email, string ipaddress = "");
    }
}
