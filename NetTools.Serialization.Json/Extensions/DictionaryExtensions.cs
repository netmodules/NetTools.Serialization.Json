using System;
using System.Collections.Generic;

namespace NetTools.Serialization
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Gets the dictionary value recursive.
        /// Returns the dictionary key value as T from either a single or nested dictionaries. If the value is unable to cast
        /// or is not found then the object assigned to @default is returned.
        /// </summary>
        /// <returns>The single or recursive dictionary value.</returns>
        /// <param name="dict">Dictionary to get the value from.</param>
        /// <param name="default">Default to return if returning the value fails.</param>
        /// <param name="keys">Single or recursive keys to look for in the dictionary or nested dictionaries.</param>
        /// <typeparam name="T">Type to cast and return.</typeparam>
        public static T GetDictionaryValueRecursive<T>(this Dictionary<string, object> dict, T @default, params string[] keys)
        {
            try
            {
                if (dict != null)
                {
                    for (var i = 0; i < keys.Length - 1; i++)
                    {
                        dict = dict[keys[i]] as Dictionary<string, object>;

                        if (dict == null)
                        {
                            return default(T);
                        }
                    }
                }

                return (T)dict[keys[keys.Length - 1]];
            }
            catch
            {
                return default(T);
            }
        }
    }
}
