using System;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using System.Data.Common;
using System.Reflection;
using System.Xml;

namespace NetTools.Serialization
{
    public static class Extensions
    {
        /// <summary>
        /// Returns the requested IEnumerable item by key (if it exists) as the requested type. If the item is not of type T conversion is
        /// attempted with value types. If the item does not exist or it cannot be converted, the @default value is returned.
        /// </summary>
        public static T GetValueAs<O, T>(this O obj, object key)
            where O : IEnumerable
        {
            try
            {
                return GetValueRecursive(obj, default(T), key);
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Returns the requested IEnumerable item by key (if it exists) as the requested type. If the item is not of type T conversion is
        /// attempted with value types. If the item does not exist or it cannot be converted, the @default value is returned.
        /// </summary>
        public static T GetValueAs<O, T>(this O obj, bool ignoreCase, object key)
            where O : IEnumerable
        {
            try
            {
                return GetValueRecursive(obj, ignoreCase, default(T), key);
            }
            catch
            {
                throw;
            }
        }


        /// <summary>
        /// Returns the requested IEnumerable item by key (if it exists) as the requested type. If the item is not of type T conversion is
        /// attempted with value types. If the item does not exist or it cannot be converted, the @default value is returned.
        /// </summary>
        public static T GetValueAs<O, T>(this O obj, T @default, object key)
            where O : IEnumerable
        {
            if (TryGetValueRecursive(obj, out T value, key))
            {
                return value;
            }

            return @default;
        }


        /// <summary>
        /// Returns the requested IEnumerable item by key (if it exists) as the requested type. If the item is not of type T conversion is
        /// attempted with value types. If the item does not exist or it cannot be converted, the @default value is returned.
        /// </summary>
        public static T GetValueAs<O, T>(this O obj, bool ignoreCase, T @default, object key)
            where O : IEnumerable
        {
            if (TryGetValueRecursive(obj, ignoreCase, out T value, key))
            {
                return value;
            }

            return @default;
        }


        /// <summary>
        /// Returns true if the requested IEnumerable item by key (if exists) can be returned as the requested type. If the item is not of
        /// type T conversion is attempted with value types. If the item does not exist or it cannot be converted, this method returns false.
        /// </summary>
        public static bool TryGetValueAs<T>(this IEnumerable obj, out T value, object key)
        {
            return TryGetValueRecursive(obj, out value, new object[] { key });
        }


        /// <summary>
        /// Returns true if the requested IEnumerable item by key (if exists) can be returned as the requested type. If the item is not of
        /// type T conversion is attempted with value types. If the item does not exist or it cannot be converted, this method returns false.
        /// </summary>
        /// <returns>The IEnumerable value at the requested key as the requested type.</returns>
        /// <param name="obj">IEnumerable to get the value at index from.</param>
        /// <param name="ignoreCase">Ignore key character case when checking for string key values.</param>
        /// <param name="value">If a value is found, it is returned in this parameter.</param>
        /// <param name="key">A numeric index or key to look for in the list, dictionary, or nested lists and/or dictionaries.</param>
        public static bool TryGetValueAs<T>(this IEnumerable obj, bool ignoreCase, out T value, object key)
        {
            return TryGetValueRecursive(obj, ignoreCase, out value, new object[] { key });
        }


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
        /// Returns a list or dictionary value recursively from an array of parameters to look for. The keys must match the indexed types
        /// within the nested IEnumerable items. If the value is unable to cast or is not found then the object assigned to @default is returned.
        /// </summary>
        /// <returns>The single or recursive list or dictionary value.</returns>
        /// <param name="obj">IEnumerable to get the value at index from.</param>
        /// <param name="ignoreCase">Ignore key character case when checking for string key values.</param>
        /// <param name="default">Default to return if returning the value fails.</param>
        /// <param name="keys">Single or recursive numeric index or key to look for in the list, dictionary, or nested lists and/or dictionaries.</param>
        /// <typeparam name="O">The instance type of the IEnumerable to iterate.</typeparam>
        /// <typeparam name="T">The Type to cast and return.</typeparam>
        public static T GetValueRecursive<O, T>(this O obj, bool ignoreCase, T @default, params object[] keys)
            where O : IEnumerable
        {
            if (TryGetValueRecursive(obj, ignoreCase, out T value, keys))
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
        /// <param name="keys">Single or recursive numeric index or key to look for in the list, dictionary, or enumerable, or nested enumerables.</param>
        /// <typeparam name="T">The Type to cast and return in the value parameter.</typeparam>
        public static bool TryGetValueRecursive<T>(this IEnumerable obj, out T value, params object[] keys)
        {
            return TryGetValueRecursive<T>(obj, out value, keys);
        }


        /// <summary>
        /// Returns true if a list or dictionary value recursively from an array of parameters to look for is found. The keys must match the indexed types
        /// within the nested IEnumerable items. If the value is unable to cast or is not found then the object assigned to @default is returned.
        /// </summary>
        /// <returns>The single or recursive list or dictionary value.</returns>
        /// <param name="obj">IEnumerable to get the value at index from.</param>
        /// <param name="ignoreCase">Ignore key character case when checking for string key values.</param>
        /// <param name="value">If a value is found, it is returned in this parameter.</param>
        /// <param name="keys">Single or recursive numeric index or key to look for in the list, dictionary, or enumerable, or nested enumerables.</param>
        /// <typeparam name="T">The Type to cast and return in the value parameter.</typeparam>
        public static bool TryGetValueRecursive<T>(this IEnumerable obj, bool ignoreCase, out T value, params object[] keys)
        {
            return TryGetValueRecursive<T>(obj, ignoreCase, out value, keys);
        }


        /// <summary>
        /// Returns true if a list or dictionary value recursively from an array of parameters to look for is found. The keys must match the indexed types
        /// within the nested IEnumerable items. If the value is unable to cast or is not found then the object assigned to @default is returned.
        /// </summary>
        /// <returns>The single or recursive list or dictionary value.</returns>
        /// <param name="obj">IEnumerable to get the value at index from.</param>
        /// <param name="keys">Single or recursive numeric index or key to look for in the list, dictionary, or nested lists and/or dictionaries.</param>
        /// <typeparam name="T">The Type to cast and return in the value parameter.</typeparam>
        public static T GetValueRecursive<T>(this IEnumerable obj, params object[] keys)
        {
            return GetValueRecursive<T>(obj, false, keys);
        }


        /// <summary>
        /// Returns true if a list or dictionary value recursively from an array of parameters to look for is found. The keys must match the indexed types
        /// within the nested IEnumerable items. If the value is unable to cast or is not found then the object assigned to @default is returned.
        /// </summary>
        /// <returns>The single or recursive list or dictionary value.</returns>
        /// <param name="obj">IEnumerable to get the value at index from.</param>
        /// <param name="ignoreCase">Ignore key character case when checking for string key values.</param>
        /// <param name="keys">Single or recursive numeric index or key to look for in the list, dictionary, or nested lists and/or dictionaries.</param>
        /// <typeparam name="T">The Type to cast and return in the value parameter.</typeparam>
        public static T GetValueRecursive<T>(this IEnumerable obj, bool ignoreCase, params object[] keys)
        {
            object objObj = obj;
            object val = null;

            try
            {
                if (objObj != null && keys.Length > 1)
                {
                    for (var i = 0; i < keys.Length - 1; i++)
                    {
                        var boolKey = keys[i] is bool;
                        var key = keys[i].ToString();
                        
                        if (objObj is IDictionary dictionary)
                        {
                            var enumerator = dictionary.GetEnumerator();

                            while (enumerator.MoveNext())
                            {
                                var entry = (DictionaryEntry)enumerator.Current;
                                if ((ignoreCase || boolKey) && keys[i] is IConvertible && entry.Key is IConvertible)
                                {
                                    var entryKey = entry.Key.ToString();
                                    
                                    if (entryKey.Equals(key, StringComparison.OrdinalIgnoreCase))
                                    {
                                        objObj = entry.Value;
                                        break;
                                    }
                                }
                                else if (entry.Key.Equals(keys[i]))
                                {
                                    objObj = entry.Value;
                                    break;
                                }
                                else
                                {
                                    objObj = null;
                                }
                            }
                        }
                        else if (objObj is IEnumerable en)
                        {
                            var stop = (int)Convert.ChangeType(i, typeof(int));
                            var enumerator = en.GetEnumerator();

                            for (int move = 0; move < stop; move++)
                            {
                                if (!enumerator.MoveNext())
                                {
                                    throw new IndexOutOfRangeException($"Key at {i} was out of range.");
                                }
                            }

                            objObj = enumerator.Current;
                        }
                        else
                        {
                            objObj = null;
                        }

                        if (objObj == null)
                        {
                            throw new KeyNotFoundException($"Key {keys[i]} at keys index {i} was not found.");
                        }
                    }
                }

                var boolKey2 = keys[keys.Length - 1] is bool;
                var key2 = keys[keys.Length - 1].ToString();

                if (objObj is IDictionary dic)
                {
                    var enumerator = dic.GetEnumerator();

                    while (enumerator.MoveNext())
                    {
                        var entry = (DictionaryEntry)enumerator.Current;
                        if ((ignoreCase || boolKey2) && keys[keys.Length - 1] is IConvertible && entry.Key is IConvertible)
                        {
                            var entryKey = entry.Key.ToString();

                            if (entryKey.Equals(key2, StringComparison.OrdinalIgnoreCase))
                            {
                                val = entry.Value;
                                break;
                            }
                        }
                        else if (entry.Key.Equals(keys[keys.Length - 1]))
                        {
                            val = entry.Value;
                            break;
                        }
                        else
                        {
                            val = null;
                        }
                    }

                    if (val == null)
                    {
                        var stop = (int)Convert.ChangeType(keys[keys.Length - 1], typeof(int));
                        var enumerator2 = dic.GetEnumerator();

                        enumerator2.MoveNext();

                        for (int move = 0; move < stop; move++)
                        {
                            enumerator2.MoveNext();
                        }

                        val = enumerator2.Current;
                    }

                    if (val is DictionaryEntry den && !(val is T))
                    {
                        var t = typeof(T);

                        var ctor = t.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic
                            , null, new Type[]
                            {
                                t.IsGenericType && t.GenericTypeArguments.Length > 0 ? t.GenericTypeArguments[0] : den.Key.GetType(),
                                t.IsGenericType && t.GenericTypeArguments.Length > 1 ? t.GenericTypeArguments[1] : den.Value.GetType() }, null);

                        if (ctor == null)
                        {
                            val = den.Value;
                        }
                        else
                        {
                            val = ctor.Invoke(new object[]
                            {
                                t.IsGenericType && t.GenericTypeArguments.Length > 0 && den.Key.GetType() == t.GenericTypeArguments[0]
                                ? den.Key
                                : Convert.ChangeType(den.Key, t.GenericTypeArguments[0]),
                                t.IsGenericType && t.GenericTypeArguments.Length > 1 && den.Value.GetType() == t.GenericTypeArguments[1]
                                ? den.Value
                                : Convert.ChangeType(den.Value, t.GenericTypeArguments[1])
                            });
                        }
                    }
                }
                else if (objObj is IEnumerable en)
                {
                    var stop = (int)Convert.ChangeType(keys[keys.Length - 1], typeof(int));
                    var enumerator = en.GetEnumerator();

                    enumerator.MoveNext();

                    for (int move = 0; move < stop; move++)
                    {
                        if (!enumerator.MoveNext())
                        {
                            throw new IndexOutOfRangeException($"Key at {keys.Length - 1} was out of range.");
                        }
                    }

                    val = enumerator.Current;
                }

                if (typeof(T) == typeof(TimeSpan) && val is string valStr)
                {
                    try
                    {
                        val = XmlConvert.ToTimeSpan(valStr);
                    }
                    catch
                    {
                        if (TimeSpan.TryParse(valStr, out var ts))
                        {
                            val = ts;
                        }
                        else
                        {
                            throw new FormatException($"The string '{valStr}' was not recognized as a valid TimeSpan. There is an unknown word starting at index '0'.");
                        }
                    }
                }

                return val is T ? (T)val : (T)Convert.ChangeType(val, typeof(T));
            }
            catch { throw; }
        }
    }
}
