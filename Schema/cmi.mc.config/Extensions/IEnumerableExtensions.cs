using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace cmi.mc.config.Extensions
{
    internal static class IEnumerableExtensions
    {
        /// <summary>
        /// Determines the equality of elements in a sequence. 
        /// </summary>
        /// <typeparam name="T">Type of elements in the sequence.</typeparam>
        /// <param name="source">The sequence.</param>
        /// <param name="value">
        /// When true is returned, '<see cref="value"/>' will contain the distinct value of all elements.
        /// When the sequence is empty, '<see cref="value"/>' is null.
        /// </param>
        /// <returns>
        /// True if all elements of the sequence are equal.
        /// True when the sequence is empty.
        /// Otherwise false.
        /// </returns>
        /// <remarks>Source: https://stackoverflow.com/questions/4354902/check-that-all-items-of-ienumerablet-has-the-same-value-using-linq </remarks>
        public static bool AllEqual<T>(this IEnumerable<T> source, out T value)
        {
            using (var enumerator = source.GetEnumerator())
            {
                if (!enumerator.MoveNext())
                {
                    value = default(T);
                    return true;
                }

                value = enumerator.Current;
                var comparer = EqualityComparer<T>.Default;

                while (enumerator.MoveNext())
                {
                    if (!comparer.Equals(value, enumerator.Current))
                    {
                        return false;
                    }
                }

                return true;
            }
        }
    }
}
