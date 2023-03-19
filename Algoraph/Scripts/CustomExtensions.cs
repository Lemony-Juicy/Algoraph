using System;
using System.Collections.Generic;
using System.Linq;

namespace Algoraph
{
    internal static class CustomExtentions
    {
        public static Random random = new Random();

        public static TYPE SelectRandom<TYPE>(this IEnumerable<TYPE> items)
        {
            return items.ElementAt(random.Next(items.Count()));
        }
    }
}
