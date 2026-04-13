/// </summary>

﻿using System;

namespace Darl.GraphQL.Models.Models
{
    public class PushSub
    {
        public string ipAddress { get; set; } = string.Empty;
        public string pushAuth { get; set; } = string.Empty;
        public string pushEndPoint { get; set; } = string.Empty;
        public string pushKey { get; set; } = string.Empty;
        public string longitude { get; set; } = string.Empty;
        public string latitude { get; set; } = string.Empty;
        public DateTime? created { get; set; }

    }
}
