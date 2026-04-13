/// <summary>
/// IBotDataInterface.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Collections.Generic;

namespace Darl.Lineage.Bot.Stores
{
    /// <summary>
    /// Veneer over IBotDataBag to avoid Darl_standard dependency on the Microsoft bot library
    /// </summary>
    public interface IBotDataInterface
    {
        bool ContainsKey(string v);
        bool TryGetValue<T>(string v, out T dval);
        void SetValue<T>(string v1, T v2);
        Dictionary<string, string> ConvertStore();
    }
}
