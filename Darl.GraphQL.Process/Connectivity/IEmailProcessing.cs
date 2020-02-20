using System;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IEmailProcessing
    {
        Task<int> Mailshot(string userId, string collateral, string subject, string sendfrom, string filter, bool test);

        Task<String> SendEmail(string body, string subject, string sendfrom, string email);
    }
}
