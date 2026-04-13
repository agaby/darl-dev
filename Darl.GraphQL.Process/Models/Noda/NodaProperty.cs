/// <summary>
/// NodaProperty.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System;

namespace Darl.GraphQL.Models.Models.Noda
{
    public class NodaProperty
    {
        public string uuid { get; set; } = Guid.NewGuid().ToString();
        public string name { get; set; } = string.Empty;
        public string text { get; set; } = string.Empty;
        public string? image { get; set; }
        public string? video { get; set; }
        public NodaTone tone { get; set; } = new NodaTone { a = 1.0, b = 0.0, g = 0.0, r = 0.0 };
        public double size { get; set; } = 1.0;
        public string? page { get; set; }
        public string? notes { get; set; }
    }
}
