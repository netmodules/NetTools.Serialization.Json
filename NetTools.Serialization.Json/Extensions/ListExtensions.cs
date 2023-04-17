using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NetTools.Serialization
{
    public static class ListExtensions
    {
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
            try
            {
                if (list != null)
                {
                    for (var i = 0; i < indices.Length - 1; i++)
                    {
                        list = list[indices[i]] as List<object>;

                        if (list == null)
                        {
                            return default(T);
                        }
                    }
                }

                return (T)list[indices[indices.Length - 1]];
            }
            catch
            {
                return default(T);
            }
        }
    }
}
