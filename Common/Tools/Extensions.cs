using System;
using System.Collections.Generic;
using System.Linq;

namespace gbo.api.extensions
{
    public static class ListExtensions
    {
        public static void Run4All<TIn>(this IEnumerable<TIn> collection, Action<TIn> method)
        {
            if (collection != null && collection.Any() && method != null)
            {
                foreach (var item in collection)
                    method(item);
            }
        }
    }
    public static class Extensions
    {
        public static string ToBinaryString(this byte value)
        {
            return $"0b{Convert.ToString(value, 2).PadLeft(8,'0')}";
        }
        public static string ToHexString(this byte value)
        {
            return $"0x{value.ToString("X")}";
        }
        public static string ToBinaryString(this int value)
        {
            return $"0b{Convert.ToString(value, 2).PadLeft(32,'0')}";
        }
        public static string ToHexString(this int value)
        {
            return $"0x{value.ToString("X")}";
        }
    }
}