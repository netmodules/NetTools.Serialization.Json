using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace NetTools.Serialization
{
    public static class Extensions
    {
        /// <summary>
        /// Returns a list or dictionary value recursively from an array of parameters to look for. The keys must match the indexed types
        /// within the nested IEnumerable items. If the value is unable to cast or is not found then the object assigned to @default is returned.
        /// </summary>
        /// <returns>The single or recursive list or dictionary value.</returns>
        /// <param name="obj">IEnumerable to get the value at index from.</param>
        /// <param name="default">Default to return if returning the value fails.</param>
        /// <param name="keys">Single or recursive numeric index or key to look for in the list, dictionary, or nested lists and/or dictionaries.</param>
        /// <typeparam name="O">The instance type of the IEnumerable to iterate.</typeparam>
        /// <typeparam name="T">The Type to cast and return.</typeparam>
        public static T GetValueRecursive<O, T>(this O obj, T @default, params object[] keys)
            where O : IEnumerable
        {
            if (TryGetValueRecursive(obj, out T value, keys))
            {
                return value;
            }

            return @default;
        }


        /// <summary>
        /// Returns true if a list or dictionary value recursively from an array of parameters to look for is found. The keys must match the indexed types
        /// within the nested IEnumerable items. If the value is unable to cast or is not found then the object assigned to @default is returned.
        /// </summary>
        /// <returns>The single or recursive list or dictionary value.</returns>
        /// <param name="obj">IEnumerable to get the value at index from.</param>
        /// <param name="value">If a value is found, it is returned in this parameter.</param>
        /// <param name="keys">Single or recursive numeric index or key to look for in the list, dictionary, or nested lists and/or dictionaries.</param>
        /// <typeparam name="O">The instance type of the IEnumerable to iterate.</typeparam>
        /// <typeparam name="T">The Type to cast and return in the value parameter.</typeparam>
        public static bool TryGetValueRecursive<T>(this IEnumerable obj, out T value, params object[] keys)
        {
            try
            {
                object objObj = obj;

                if (objObj != null)
                {
                    for (var i = 0; i < keys.Length - 1; i++)
                    {
                        if (keys[i] is string key && objObj is IDictionary dictionary)
                        {
                            objObj = dictionary[key];
                        }
                        else if (keys[i] is int index && objObj is IList list)
                        {
                            objObj = list[index];
                        }

                        if (objObj == null)
                        {
                            value = default;
                            return false;
                        }
                    }
                }

                if (keys[keys.Length - 1] is string k && objObj is IDictionary d)
                {
                    value = (T)d[k];
                    return true;
                }
                else if (keys[keys.Length - 1] is int ind && objObj is IList l)
                {
                    value = (T)l[ind];
                    return true;
                }

            }
            catch { }

            value = default;
            return false;
        }
    }
}
