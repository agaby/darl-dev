/// <summary>
/// IProducts.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Models;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IProducts
    {
        List<DarlProduct> products { get; }
    }
}
