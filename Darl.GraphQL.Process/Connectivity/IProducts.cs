using Darl.GraphQL.Models.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Connectivity
{
    public interface IProducts
    {
        List<DarlProduct> products { get; }
    }
}
