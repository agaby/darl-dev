using Darl.GraphQL.Models.Models.Noda;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace Darl.GraphQL.Process.Models.Noda.Layout
{
    class ColorGenerator
    {
        public static List<NodaTone> Pick(int num)
        {
            var colors = new List<NodaTone>();
            if (num < 2)
                return colors;
            float dx = 1.0f / (float)(num - 1);
            for (int i = 0; i < num; i++)
            {
                colors.Add(get(i * dx));
            }
            return colors;
        }

        private static NodaTone get(double x)
        {
            double r = 0.0;
            double g = 0.0;
            double b = 1.0;
            if (x >= 0.0 && x < 0.2)
            {
                x = x / 0.2;
                r = 0.0;
                g = x;
                b = 1.0;
            }
            else if (x >= 0.2 && x < 0.4)
            {
                x = (x - 0.2) / 0.2;
                r = 0.0;
                g = 1.0;
                b = 1.0 - x;
            }
            else if (x >= 0.4 && x < 0.6)
            {
                x = (x - 0.4) / 0.2;
                r = x;
                g = 1.0;
                b = 0.0;
            }
            else if (x >= 0.6 && x < 0.8)
            {
                x = (x - 0.6) / 0.2;
                r = 1.0;
                g = 1.0 - x;
                b = 0.0;
            }
            else if (x >= 0.8 && x <= 1.0)
            {
                x = (x - 0.8) / 0.2;
                r = 1.0;
                g = 0.0;
                b = x;
            }
            return new NodaTone { r = r, a = 1.0, b = b, g = g };
        }
    }
}
