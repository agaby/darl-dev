using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IBlobConnectivity
    {
        Task<bool> Exists(string name);
        Task<byte[]> Read(string name);
        Task Write(string name, byte[] data);
    }
}
