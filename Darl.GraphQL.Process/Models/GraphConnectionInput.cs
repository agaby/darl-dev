/// <summary>
/// GraphConnectionInput.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Darl.GraphQL.Models.Models
{
    public class GraphConnectionInput  : GraphElementInput
    {
        public double? weight { get; set; }
        public string startId { get; set; }
        public string endId { get; set; }

    }
}
