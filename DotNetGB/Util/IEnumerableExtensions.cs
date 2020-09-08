using System;
using System.Collections.Generic;

namespace DotNetGB.Util
{
    public static class IEnumerableExtensions
    {
        public static void ForEach(this IEnumerable<Action> items)
        {
            foreach (var item in items)
            {
                item();
            }
        }
    }
}
