using System;
using System.Collections.Generic;

namespace NetTools.Serialization
{
    public static class DictionaryExtensions
    {
        /// <summary>
        /// Returns the requested Dictionary item by key (if it exists) as the requested type. If the item is not of type T
        /// conversion is attempted with value types. If the item does not exist or it cannot be converted, the @default
        /// value is returned. This method is used by <see cref="GetDictionaryValueRecursive{T}(Dictionary{string, object}, T, string[])"/>
        /// </summary>
        public static T GetDictionaryValue<T>(this Dictionary<string, object> dict, T @default, string key)
        {
            try
            {
                object value = dict[key];
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
        /// <param name="dict">Dictionary to get the value from.</param>
        /// <param name="default">Default to return if returning the value fails.</param>
        /// <param name="keys">Single or recursive keys to look for in the dictionary or nested dictionaries.</param>
        /// <typeparam name="T">Type to cast and return.</typeparam>
        public static T GetDictionaryValueRecursive<T>(this Dictionary<string, object> dict, T @default, params string[] keys)
        {
            if (dict != null && keys.Length > 1)
            {
                for (var i = 0; i < keys.Length - 1; i++)
                {
                    dict = dict[keys[i]] as Dictionary<string, object>;

                    if (dict == null)
                    {
                        return @default;
                    }
                }
            }

            return GetDictionaryValue(dict, @default, keys[keys.Length - 1]);
        }
    }
}
