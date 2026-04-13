/// <summary>
/// NodaElement.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System;
using System.Collections.Generic;

namespace Darl.GraphQL.Models.Models.Noda
{
    public abstract class NodaElement
    {
        public int id { get; set; } = 0;

        public string? title { get; set; }
        public string? kind { get; set; }

        public NodaTone? tone { get; set; }
        public bool folded { get; set; } = false;

        public string uuid { get; set; } = Guid.NewGuid().ToString();

        public double size { get; set; } = 1.0;

        public List<NodaProperty> properties { get; set; } = new List<NodaProperty>();
    }
}
