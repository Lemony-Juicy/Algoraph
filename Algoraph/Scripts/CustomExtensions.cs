﻿using Algoraph.Scripts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Media;

namespace Algoraph
{
    internal static class CustomExtentions
    {
        public static BrushConverter brushConverter = new BrushConverter();
        public static Random random = new Random();

        public static string Stringify(this IEnumerable<Node> nodes)
        {
            return string.Join(',', nodes.Select(n => n.name));
        }

        public static TYPE SelectRandom<TYPE>(this IEnumerable<TYPE> items)
        {
            return items.ElementAt(random.Next(items.Count()));
        }
    }
}
