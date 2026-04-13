/// <summary>
/// NodaViewNodeLocation.cs - Core module for the Darl.dev project.
/// </summary>

﻿using Darl.GraphQL.Models.Models.Noda;
using Newtonsoft.Json;

namespace Darl.GraphQL.Process.Web.Models.Noda
{
    public class NodaViewNodeLocation
    {
        public enum RelativeTo { Origin, User, Window };
        public double x { get { return position.x; } set { position.x = value; } }
        public double y { get { return position.y; } set { position.y = value; } }
        public double z { get { return position.z; } set { position.z = value; } }
        public RelativeTo relativeTo { get; set; }

        [JsonIgnore]
        internal NodaPosition position { get; set; } = NodaPosition.Random();
    }
}
