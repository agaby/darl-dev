/// <summary>
/// </summary>

﻿namespace Darl.GraphQL.Models.Models.Noda
{
    public class NodaTone
    {
        public double r { get; set; }
        public double g { get; set; }
        public double b { get; set; }
        public double a { get; set; }

        public string ToHex()
        {
            return ((int)(r * 255)).ToString("X2") + ((int)(g * 255)).ToString("X2") + ((int)(b * 255)).ToString("X2");
        }
    }
}
