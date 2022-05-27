using System;

namespace Darl.GraphQL.Process.Models.Noda.Layout
{
    public class Util
    {
        private static readonly Random random = new Random();
        public static double Random()
        {
            var result = random.NextDouble();
            return (double)result;
        }
    }

}
