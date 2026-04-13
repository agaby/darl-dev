/// <summary>
/// LineageMatchNodePair.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.Lineage;

namespace Darl.GraphQL.Models.Models
{
    public class LineageMatchNodePair
    {
        public LineageMatchNodePair(string text, LineageMatchNode match)
        {
            Text = text;
            Match = match;
        }

        public string Text { get; }
        public LineageMatchNode Match { get; }
    }
}
