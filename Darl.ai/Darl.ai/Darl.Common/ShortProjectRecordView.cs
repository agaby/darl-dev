/// <summary>
/// ShortProjectRecordView.cs - Core module for the Darl.dev project.
/// </summary>

﻿using System;

namespace DarlCommon
{
    [Serializable]
    public class ShortProjectRecordView
    {
        public string description { get; set; }
        public string id { get; set; }
        public string name { get; set; }
    }
}
