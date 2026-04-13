/// <summary>
/// </summary>

﻿using Darl.Thinkbase;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Models
{
    public class StringListGraphAttributePair
    {
        public string Name { get; set; }

        public List<GraphAttribute> Value { get; set; }
    }
}
