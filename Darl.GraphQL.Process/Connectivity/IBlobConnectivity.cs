/// <summary>
/// IBlobConnectivity.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Collections.Generic;
using System.Threading.Tasks;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IBlobConnectivity
    {
        Task<bool> Delete(string name);
        Task<bool> Exists(string name);
        List<string> List(string prefix);
        Task<byte[]> Read(string name);
        Task Write(string name, byte[] data);
        string CreateTimedAccessUrl(string name);

        string implementation { get; }
    }
}
