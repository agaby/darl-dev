using System;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IEmailProcessing
    {
        Task<int> Mailshot(string collateral, string subject, string sendfrom, bool test);

        Task<String> SendEmail(string body, string subject, string sendfrom, string email);
        Task<string> InviteUser(string userId, string email);
    }
}
