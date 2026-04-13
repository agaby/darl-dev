/// <summary>
/// SupportMailMessage.cs - Core module for the Darl.dev project.
/// </summary>

﻿namespace Darl.GraphQL.Models.Models
{
    public class SupportMailMessage
    {
        public string from { get; set; }
        public string to { get; set; }
        public string subject { get; set; }
        public string content { get; set; }
    }
}
