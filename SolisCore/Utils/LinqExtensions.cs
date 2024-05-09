using System;
using System.Collections.Generic;
using System.Text;

namespace SolisCore.Utils
{
    public static class LinqExtensions
    {
        public static IEnumerable<(T, int)> SolisWithIndex<T>(this IEnumerable<T> items)
        {
            if (items == null) throw new ArgumentNullException(nameof(items));

            using IEnumerator<T> e1 = items.GetEnumerator();
            int i = 0;
            while (e1.MoveNext())
            {
                yield return (e1.Current, i++);
            }
        }

        // Zip doesn't ship with this for all versions
        public static IEnumerable<(TFirst First, TSecond Second)> SolisZip<TFirst, TSecond>(this IEnumerable<TFirst> first, IEnumerable<TSecond> second)
        {
            if (first == null) throw new ArgumentNullException(nameof(first));
            if (second == null) throw new ArgumentNullException(nameof(second));

            using IEnumerator<TFirst> e1 = first.GetEnumerator();
            using IEnumerator<TSecond> e2 = second.GetEnumerator();
            while (e1.MoveNext() && e2.MoveNext())
            {
                yield return (e1.Current, e2.Current);
            }
        }
    }
}
