/// <summary>
/// StringDarlVarPair.cs - Core module for the Darl.dev project.
/// </summary>

﻿using DarlCommon;

namespace Darl.GraphQL.Models.Models
{
    public class StringDarlVarPair
    {
        public string Name { get; set; }

        public DarlVar Value { get; set; }
    }
}
