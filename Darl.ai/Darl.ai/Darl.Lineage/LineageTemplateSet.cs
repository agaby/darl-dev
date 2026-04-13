/// <summary>
/// LineageTemplateSet.cs - Core module for the Darl.dev project.
/// </summary>

﻿using ProtoBuf;
using System;
using System.Collections.Generic;

namespace Darl.Lineage
{
    [ProtoContract]
    public class LineageTemplateSet
    {

        public LineageTemplateSet()
        {

        }

        [ProtoMember(1)]
        public List<LineageTemplate> templates { get; set; }

        [ProtoMember(2)]
        public string payload { get; set; }

        /// <summary>
        /// Used as an identifier during editing
        /// </summary>
        public string id { get; set; } = Guid.NewGuid().ToString();
    }
}