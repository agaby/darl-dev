/// <summary>
/// DisplayModel.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System.Collections.Generic;

namespace Darl.Thinkbase
{
    public class DisplayModel
    {
        public List<DisplayObject> nodes { get; set; }

        public List<DisplayConnection> edges { get; set; }

    }
}
