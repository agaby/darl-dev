using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface ICheckEmail
    {
        Task<bool> CheckEmail(string email, string ipaddress = "");
    }
}
