/// <summary>
/// </summary>

﻿namespace Darl.GraphQL.Process.Web.Models.Noda
{
    public interface ILayoutLink
    {
        string uuid { get; set; }
        double length { get; set; }
        string FromNode();
        string ToNode();
    }
}
