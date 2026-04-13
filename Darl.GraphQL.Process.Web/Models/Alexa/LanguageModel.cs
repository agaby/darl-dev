/// <summary>
/// </summary>

﻿using System.Collections.Generic;

namespace Darl.GraphQL.Process.Models.Alexa
{
    public class LanguageModel
    {
        public string invocationName { get; set; }
        public List<Intent> intents { get; set; } = new List<Intent>();

        public List<Type> types { get; set; } = new List<Type>();
    }
}
