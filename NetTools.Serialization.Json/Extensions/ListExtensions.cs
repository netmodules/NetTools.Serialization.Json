using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NetTools.Serialization
{
    public static class ListExtensions
    {
        /// <summary>
        /// Returns the requested List item by index (if it exists) as the requested type. If the item is not of type T
        /// conversion is attempted with value types. If the item does not exist or it cannot be converted, the @default
        /// value is returned. This method is used by <see cref="GetListValueRecursive{T}(List{object}, T, int[])"/>
        /// </summary>
        public static T GetListValue<T>(this List<object> list, T @default, int index)
        {
            try
            {
                object value = list[index];
                return value is T ? (T)value : (T)Convert.ChangeType(value, typeof(T));
            }
            catch { }
            return @default;
        }

        /// <summary>
        /// Gets the dictionary value recursive.
        /// Returns the dictionary key value as T from either a single or nested dictionaries. If the value is unable to cast
        /// or is not found then the object assigned to @default is returned.
        /// </summary>
        /// <returns>The single or recursive dictionary value.</returns>
        /// <param name="list">List to get the value at index from.</param>
        /// <param name="default">Default to return if returning the value fails.</param>
        /// <param name="indices">Single or recursive index to look for in the list or nested lists.</param>
        /// <typeparam name="T">Type to cast and return.</typeparam>
        public static T GetListValueRecursive<T>(this List<object> list, T @default, params int[] indices)
        {
            if (list != null && indices.Length > 1)
            {
                for (var i = 0; i < indices.Length - 1; i++)
                {
                    list = (List<object>)list[indices[i]];

                    if (list == null)
                    {
                        return default(T);
                    }
                }
            }

            return GetListValue(list, @default, indices[indices.Length - 1]);
        }
    }
}
