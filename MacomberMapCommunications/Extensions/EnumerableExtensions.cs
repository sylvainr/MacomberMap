using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MacomberMapCommunications.Extensions
{
    /// <summary>
    /// Extension methods for <see cref="IEnumerable{T}"/>
    /// </summary>
    public static class EnumerableExtensions
    {
        /// <summary>
        /// Converts an <see cref="IEnumerable{T}"/> to a <see cref="HashSet{T}"/>.
        /// </summary>
        /// <typeparam name="T">The enumerated type.</typeparam>
        /// <param name="source">The source collection.</param>
        /// <returns>The <see cref="HashSet{T}"/>.</returns>
        /// <remarks>Using <c>new HashSet(IEnumerable&gt;T&lt;)</c> provides similar functionality.</remarks>
        public static HashSet<T> ToHashSet<T>(this IEnumerable<T> source)
        {
            return new HashSet<T>(source);
        }

        /// <summary>
        /// Extends an array and adds an item to then end.
        /// </summary>
        /// <typeparam name="T">The array type.</typeparam>
        /// <param name="array">The array.</param>
        /// <param name="item">Item to add.</param>
        /// <returns>New array with item appended.</returns>
        public static T[] Add<T>(this T[] array, T item)
        {
            var arr = new T[array.Length + 1];
            array.CopyTo(arr, 0);
            arr[array.Length] = item;
            return arr;
        }

        /// <summary>
        /// Insert an item into a list in order.
        /// </summary>
        /// <typeparam name="T">Type of item</typeparam>
        /// <param name="dis">The list</param>
        /// <param name="item">The item to insert</param>
        public static void AddSorted<T>(this List<T> dis, T item) where T : IComparable<T>
        {
            if (dis.Count == 0)
            {
                dis.Add(item);
                return;
            }
            if (dis[dis.Count - 1].CompareTo(item) <= 0)
            {
                dis.Add(item);
                return;
            }
            if (dis[0].CompareTo(item) >= 0)
            {
                dis.Insert(0, item);
                return;
            }
            int index = dis.BinarySearch(item);
            if (index < 0)
                index = ~index;
            dis.Insert(index, item);
        }

    }

    /// <summary>
    /// Extension methods for <see cref="Enum"/>
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Try Parse Enum with immediate return instead of out parameter.
        /// </summary>
        /// <typeparam name="TEnum">Enum Type</typeparam>
        /// <param name="value">String value of Enum</param>
        /// <param name="ignoreCase">Optional: Ignore case of string</param>
        /// <returns>Enum Value</returns>
        public static TEnum Parse<TEnum>(string value, bool ignoreCase = false) where TEnum : struct
        {
            TEnum val;

            if (Enum.TryParse<TEnum>(value, ignoreCase, out val))
                return val;

            return default(TEnum);
        }
    }

}
