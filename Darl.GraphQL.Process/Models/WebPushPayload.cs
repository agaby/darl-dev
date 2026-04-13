/// <summary>
/// WebPushPayload.cs - Core module for the Darl.dev project.
/// </summary>

﻿namespace Darl.GraphQL.Models.Models
{
    public class WebPushPayload
    {
        public string title { get; set; } = string.Empty;

        public WebPushOptions? options { get; set; }
    }
}
