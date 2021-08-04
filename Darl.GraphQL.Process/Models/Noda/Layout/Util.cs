using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Darl.GraphQL.Process.Models.Noda.Layout
{ 
    public class Util
    {
        private static Random random = new Random();
        public static double Random()
        {
            var result = random.NextDouble();
            return (double)result;
        }
    }

}
