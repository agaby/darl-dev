/// <summary>
/// Collateral.cs - Core module for the Darl.dev project.
/// </summary>

﻿namespace Darl.GraphQL.Models.Models
{
    public class Collateral
    {
        public string Name { get; set; }
        public string Value { get; set; }
        public string userId { get; set; }
        public byte[] content { get; set; }

    }
}
