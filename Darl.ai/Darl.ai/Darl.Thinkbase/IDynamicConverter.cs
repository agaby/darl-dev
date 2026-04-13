/// <summary>
/// IDynamicConverter.cs - Core module for the Darl.dev project.
/// </summary>

﻿namespace Darl.Thinkbase
{
    public interface IDynamicConverter
    {
        string sourceName { get; set; }

        string description { get; set; }

        IGraphModel model { get; internal set; }

        /// <summary>
        /// Builds or rebuilds the GraphModel from the data source
        /// </summary>
        void Convert();
    }
}
