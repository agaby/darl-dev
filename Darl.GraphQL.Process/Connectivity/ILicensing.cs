using System;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface ILicensing
    {
        string CreateKey(DateTime endDate, string company, string email);
        bool CheckKey(string key);

    }
}
