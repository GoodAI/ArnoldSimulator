using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GoodAI.Arnold.Extensions
{
    public static class LinqExtensions
    {
        public static void EachWithIndex<T>(this IEnumerable<T> items, Action<int, T> action)
        {
            var i = 0;
            foreach (T item in items)
            {
                action(i, item);
                i++;
            }
        }
    }
}
