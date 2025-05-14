using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using NetTools.Serialization.Attributes;
using NetTools.Serialization.Serializers;

namespace NetTools.Serialization.JsonTools
{
    // This class was originally forked and has been heavily extended from "Really simple JSON parser in ~300 lines"
    // - Attempts to parse JSON files with minimal GC allocation
    // - Nice and simple "[1,2,3]".FromJson<List<int>>() API
    // - Classes and structs can be parsed too!
    //      class Foo { public int Value; }
    //      "{\"Value\":10}".FromJson<Foo>()
    // - Can parse JSON without type information into Dictionary<string,object> and List<object> e.g.
    //      "[1,2,3]".FromJson<object>().GetType() == typeof(List<object>)
    //      "{\"Value\":10}".FromJson<object>().GetType() == typeof(Dictionary<string,object>)
    // - No JIT Emit support to support AOT compilation on iOS
    // - Attempts are made to NOT throw an exception if the JSON is corrupted or invalid: returns null instead.
    // - Only public fields and property setters on classes/structs will be written to
    //
    // Limitations:
    // - No JIT Emit support to parse structures quickly
    // - Limited to parsing <2GB JSON files (due to int.MaxValue) x86 only.
    // - Parsing of abstract classes or interfaces is NOT supported and will throw an exception. (Extended to implement KnowObjectAttribute).
    internal class JsonReader
    {
        Type ListInterface = typeof(IList);
        Type DictionaryInterface = typeof(IDictionary);
        Type ObjectType = typeof(object);

        public object FromJson(Type t, string json, StringSerializerFactory serializerFactory, bool includePrivates, bool parseBroken)
        {
            Stack<List<string>> splitArrayPool = new Stack<List<string>>();
            StringBuilder stringBuilder = new StringBuilder();
            
            // Remove all whitespace not within strings to make parsing simpler
            for (int i = 0; i < json.Length; i++)
            {
                char c = json[i];

                if (c == '\"')
                {
                    i = AppendUntilStringEnd(true, i, json, stringBuilder, splitArrayPool);
                    continue;
                }

                if (char.IsWhiteSpace(c))
                {
                    continue;
                }

                stringBuilder.Append(c);
            }

            // Parse the object!
            return ParseValue(t, stringBuilder.ToString(), serializerFactory, stringBuilder, splitArrayPool, includePrivates, parseBroken);
        }

        public T FromJson<T>(string json, StringSerializerFactory serializerFactory, bool includePrivates, bool parseBroken)
        {
            return (T)FromJson(typeof(T), json, serializerFactory, includePrivates, parseBroken);
        }

        int AppendUntilStringEnd(bool appendEscapeCharacter, int startIdx, string json, StringBuilder stringBuilder, Stack<List<string>> splitArrayPool)
        {
            stringBuilder.Append(json[startIdx]);

            for (int i = startIdx + 1; i < json.Length; i++)
            {
                if (json[i] == '\\')
                {
                    if (appendEscapeCharacter)
                    {
                        stringBuilder.Append(json[i]);
                    }

                    stringBuilder.Append(json[i + 1]);
                    i++; // Skip next character as it is escaped
                }
                else if (json[i] == '\"')
                {
                    stringBuilder.Append(json[i]);
                    return i;
                }
                else
                {
                    stringBuilder.Append(json[i]);
                }
            }

            return json.Length - 1;
        }


        // Splits { <value>:<value>, <value>:<value> } and [ <value>, <value> ] into a list of <value> strings
        List<string> Split(string json, StringBuilder stringBuilder, Stack<List<string>> splitArrayPool)
        {
            List<string> splitArray = splitArrayPool.Count > 0 ? splitArrayPool.Pop() : new List<string>();
            splitArray.Clear();

            if (json.Length == 2)
            {
                return splitArray;
            }

            int parseDepth = 0;
            stringBuilder.Length = 0;

            for (int i = 1; i < json.Length - 1; i++)
            {
                switch (json[i])
                {
                    case '[':
                    case '{':
                        parseDepth++;
                        break;
                    case ']':
                    case '}':
                        parseDepth--;
                        break;
                    case '\"':
                        i = AppendUntilStringEnd(true, i, json, stringBuilder, splitArrayPool);
                        continue;
                    case ',':
                    case ':':
                        if (parseDepth == 0)
                        {
                            splitArray.Add(stringBuilder.ToString());
                            stringBuilder.Length = 0;
                            continue;
                        }
                        break;
                }

                stringBuilder.Append(json[i]);
            }

            splitArray.Add(stringBuilder.ToString());
            return splitArray;
        }


        /// <summary>
        /// 
        /// </summary>
        internal object ParseValue(Type type, string json, StringSerializerFactory serializerFactory, StringBuilder stringBuilder, Stack<List<string>> splitArrayPool, bool includePrivates, bool parseBroken)
        {
            // Try and parse the json value using a custom serializer from the StringSerializerFactory and if we have an object then just return it,
            // This means that custom parsers are handled first and a custom parser may be more optimized than the attempted bruteforce which occurs
            // if there is no custom serializer.
            object obj = null;

            var customSerializer = type.GetAttributes<JsonSerializer>().FirstOrDefault();

            if (customSerializer != null)
            {
                obj = customSerializer.FromString(json.RemoveDoubleQuotes(), type);
            }

            if (obj == null)
            {
                obj = serializerFactory.FromString(json.RemoveDoubleQuotes(), type);
            }

            if (obj != null)
            {
                return obj;
            }

            // There's no custom serializer so we start by looking at primitive (built-in) types. This will also return true with a null value if
            // the value of json is "null" (without quotes), since we class null as any/all types.
            if (TryGetPrimitiveValue(type, json, out object value))
            {
                return value;
            }

            // We check here to see if the type has any type-level custom attributes that suggest we should deserialize into a different type...
            var customType = type.GetAttributes<KnownObject>().FirstOrDefault()?.KnownType;

            if (customType == null)
            {
                customType = type.GetAttributes<KnownTypeAttribute>().FirstOrDefault()?.Type;
            }

            if (customType != null && customType != type && type.IsAssignableFrom(customType))
            {
                return ParseValue(customType, json, serializerFactory, stringBuilder, splitArrayPool, includePrivates, parseBroken);
            }

            // There's no custom type so let's continue trying to deserialize...
            if (type.IsArray)
            {
                json = json.RemoveDoubleQuotes();
                Type arrayType = type.GetElementType();

                if (json[0] != '[' || (!parseBroken && json[json.Length - 1] != ']'))
                {
                    return null;
                }

                List<string> elems = Split(json, stringBuilder, splitArrayPool);
                Array newArray = Array.CreateInstance(arrayType, elems.Count);

                for (int i = 0; i < elems.Count; i++)
                {
                    newArray.SetValue(ParseValue(arrayType, elems[i], serializerFactory, stringBuilder, splitArrayPool, includePrivates, parseBroken), i);
                }

                splitArrayPool.Push(elems);

                return newArray;
            }

            var info = type.GetTypeInfoCached();

            if (info.IsEnum)
            {
                // Added try/catch wrapper here to quick catch values that may be valid values but may contain whitespace.
                // This was seen in a scenario where enum values are country names such as "UnitedStates" but JSON values
                // are passed in as "United States". In this instance the value can be parsed if the whitespace is removed.
                try
                {
                    return Enum.Parse(type, json.RemoveDoubleQuotes(), true);
                }
                catch
                {
                    var j = json.RemoveDoubleQuotes().Replace(" ", "").Replace(".", "");

                    try
                    {
                        return Enum.Parse(type, j, true);
                    }
                    catch
                    {
                        return Enum.TryParse(type, j.Replace("-", "").Replace("_", ""), true, out var e) ? e : 0;
                    }
                }
            }

            var isGeneric = info.IsGenericType;

            if ((isGeneric && type.GetGenericTypeDefinition() == typeof(List<>)) || ListInterface.IsAssignableFrom(info))
            {
                json = json.RemoveDoubleQuotes();
                var args = GetGenericArguments(info, ListInterface);
                Type listType = args[0]; //info.GenericTypeArguments[0];

                if (json[0] != '[' || (!parseBroken && json[json.Length - 1] != ']'))
                {
                    return null;
                }

                List<string> elems = Split(json, stringBuilder, splitArrayPool);
                IList list = null;

                try
                {
                    list = (IList)type.GetConstructor(new Type[] { typeof(int) })?.Invoke(new object[] { elems.Count });
                    
                    if (list == null)
                    {
                        list = (IList)ReflectionUtils.GetInstanceOf(type);
                    }
                }
                catch
                {
                    list = (IList)ReflectionUtils.GetInstanceOf(type);
                }

                if (list == null)
                {
                    return null;
                }

                for (int i = 0; i < elems.Count; i++)
                {
                    list.Add(ParseValue(listType, elems[i], serializerFactory, stringBuilder, splitArrayPool, includePrivates, parseBroken));
                }

                splitArrayPool.Push(elems);

                return list;
            }
            
            if ((isGeneric && type.GetGenericTypeDefinition() == typeof(Dictionary<,>)) || DictionaryInterface.IsAssignableFrom(info))
            {
                json = json.RemoveDoubleQuotes();

                Type keyType, valueType;
                {
                    var args = GetGenericArguments(info, DictionaryInterface);
                    
                    //Type[] args = info.GenericTypeArguments;
                    keyType = args[0];
                    valueType = args[1];
                }

                // Refuse to parse dictionary keys that aren't of type string or enum.
                if (keyType != typeof(string)  && (!Json.AutoCastKeys || (!keyType.IsEnum && !typeof(IConvertible).IsAssignableFrom(keyType))))
                {
                    return null;
                }
            
                // Must be a valid dictionary element.
                if (json[0] != '{' || (!parseBroken && json[json.Length - 1] != '}'))
                {
                    return null;
                }

                // The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON.
                List<string> elems = Split(json, stringBuilder, splitArrayPool);

                if (elems.Count % 2 != 0)
                {
                    if (!parseBroken)
                    {
                        return null;
                    }

                    if (elems.Count > 0)
                    {
                        elems.RemoveAt(elems.Count - 1);
                    }
                }

                IDictionary dictionary = null;

                try
                {
                    dictionary = (IDictionary)type.GetConstructor(new Type[] { typeof(int) })?.Invoke(new object[] { elems.Count / 2 });

                    if (dictionary == null)
                    {
                        dictionary = (IDictionary)ReflectionUtils.GetInstanceOf(type);
                    }
                }
                catch
                {
                    dictionary = (IDictionary)ReflectionUtils.GetInstanceOf(type);
                }

                if (dictionary == null)
                {
                    return null;
                }

                for (int i = 0; i < elems.Count; i += 2)
                {
                    if (elems[i].Length <= 2)
                    {
                        continue;
                    }

                    string keyValue = elems[i].Substring(1, elems[i].Length - 2);
                    object val = ParseValue(valueType, elems[i + 1], serializerFactory, stringBuilder, splitArrayPool, includePrivates, parseBroken);

                    if (keyType.IsEnum)
                    {
                        if (Enum.TryParse(keyType, keyValue, true, out var enumKeyValue))
                        {
                            dictionary.Add(enumKeyValue, val);
                        }
                        else
                        {
                            return null;
                        }
                    }
                    else
                    {
                        if (keyValue.GetType() != keyType && typeof(IConvertible).IsAssignableFrom(keyType))
                        {
                            dictionary.Add(Convert.ChangeType(keyValue, keyType), val);
                        }
                        else
                        {
                            dictionary.Add(keyValue, val);
                        }
                    }
                }

                return dictionary;
            }

            if (type == typeof(object))
            {
                return ParseAnonymousValue(json, serializerFactory, stringBuilder, splitArrayPool, includePrivates, parseBroken);
            }

            // Recursive method call for nested JSON objects.
            if (json[0] == '{' && (parseBroken || json[json.Length - 1] == '}'))
            {
                return ParseObject(type, json, serializerFactory, stringBuilder, splitArrayPool, includePrivates, parseBroken);
            }

            // Check if the json value is wrapped with quotes or can be parsed as a number.
            var jsonIsPrimitive = (json[0] == '"' && json[json.Length - 1] == '"') || double.TryParse(json, out double numericValue);

            // If we get to here and we know that json is a primitive value, we use dirty reflection to check for implicit/explicit operator or a public constructor
            // which accepts the json string or its numeric value before finally returning null. This is the last resort for returning an instantiated object.
            if (jsonIsPrimitive)
            {
                // Look for a constructor which will accept a single parameter.
                foreach(var m in type.GetConstructors())
                {
                    var parameters = m.GetParameters();

                    // We only want to pull constructors that will accept a single parameter, we need to check for optional parameters here also.
                    if (parameters.Length == 1 || (parameters.Length > 1 && parameters[1].IsOptional))
                    {
                        // Try to get a matching primitive type value for the single parameter constructor.
                        if (TryGetPrimitiveValue(parameters[0].ParameterType, json, out object primitive))
                        {
                            var arguments = new object[parameters.Length];
                            arguments[0] = primitive;
                            return m.Invoke(arguments);
                        }
                    }
                }

                // Look for implicit/explicit overrides that may accept primitive types.
                foreach(var m in type.GetMethods())
                {
                    // We need only static methods since operator overrides are static. Name identifiers for operators start with "op_".
                    if (m.IsStatic && (m.Name == "op_Implicit" || m.Name == "op_Explicit"))
                    {
                        var parameters = m.GetParameters();

                        // Operator overrides should only accept a single parameter but parameter.Length check is minimal overhead and keeps the code structured
                        // similar to the above for readability.
                        if (parameters.Length == 1)
                        {
                            if (TryGetPrimitiveValue(parameters[0].ParameterType, json, out object primitive))
                            {
                                return m.Invoke(null, new object[] { primitive });
                            }
                        }
                    }
                }
            }


            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        private bool TryGetPrimitiveValue(Type type, string json, out object value)
        {
            // We treat null as a primitive value here. If the JSON string is null value then we simply set the value to null and return true since no
            // further object parsing is required.
            if (json.Equals("null", StringComparison.OrdinalIgnoreCase))
            {
                value = null;
                return true;
            }

            var isString = json[0] == '"' && json[json.Length - 1] == '"';

            // If the type is object and the value is a string it is impossible to know what type to return without getting into complex pattern matching.
            // For simplicity sake, we just return the string itself, and imply that the object should be a string.
            if (type == typeof(string) || (type == typeof(object) && isString))
            {
                if (isString)
                {
                    json = json.RemoveDoubleQuotes().DecodeUnicodeCharacters();
                }

                // If the string is empty then return empty string.
                if (json.Length == 0)
                {
                    value = string.Empty;
                    return true;
                }

                // We do some decoding of the JSON string here for JSON encoded and escaped characters...
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < json.Length; ++i)
                {
                    if (json[i] == '\\' && i + 1 < json.Length)
                    {
                        switch (json[i + 1])
                        {
                            case '"':
                                sb.Append('"');
                                break;
                            case '\\':
                                sb.Append("\\");
                                break;
                            case 'b':
                                sb.Append("\b");
                                break;
                            case 'f':
                                sb.Append("\f");
                                break;
                            case 't':
                                sb.Append("\t");
                                break;
                            case 'n':
                                sb.Append("\n");
                                break;
                            case 'r':
                                sb.Append("\r");
                                break;
                            case '0':
                                sb.Append("\0");
                                break;
                            case 'u':
                                // Possible Unicode character detected.
                                int remainingLength = json.Length - (i + 1);
                                if (remainingLength < 4) break;

                                // Attempt to parse the 32 bit hex into an ansi char code (skipping the \u).
                                uint unicode = ParseUnicode(json[i + 2], json[i + 3], json[i + 4], json[i + 5]);

                                // If unicode is greater than the max ansi code continue as normal.
                                if (unicode > 255)
                                {
                                    sb.Append(json[i]);
                                }
                                else
                                {
                                    // Append the unicode char and skip the next 4 chars in the JSON.
                                    sb.Append((char)unicode);
                                    i += 4;
                                }
                                break;
                            case '/':
                                // Special case where forward slashes can be escaped and still be valid JSON {http:\\/\\/example.com\\/)
                                sb.Append('/');

                                break;
                            default:
                                sb.Append(json[i]);
                                break;
                        }

                        i++;
                    }
                    else
                    {
                        sb.Append(json[i]);
                    }
                }

                value = sb.ToString();
                return true;
            }

            // Non-nullables...

            // any primitive types should not be wrapped in quotes so we strip them here before trying to parse the primitive.
            json = json.Trim(new char[] { '"' });

            // If the type is bool we simply check if json is "true" or "false". If it isn't either then we must assume false since we need to return a non-nullable object.
            if (type == typeof(bool))
            {
                value = json.ToLower() == "true";
                return true;
            }

            if (type == typeof(int))
            {
                int.TryParse(json, out int result);
                value = result;
                return true;
            }

            if (type == typeof(uint))
            {
                uint.TryParse(json, out uint result);
                value = result;
                return true;
            }

            if (type == typeof(long))
            {
                long.TryParse(json, out long result);
                value = result;
                return true;
            }

            if (type == typeof(ulong))
            {
                ulong.TryParse(json, out ulong result);
                value = result;
                return true;
            }

            if (type == typeof(short))
            {
                short.TryParse(json, out short result);
                value = result;
                return true;
            }

            if (type == typeof(ushort))
            {
                ushort.TryParse(json, out ushort result);
                value = result;
                return true;
            }

            if (type == typeof(float))
            {
                float.TryParse(json, out float result);
                value = result;
                return true;
            }

            if (type == typeof(double))
            {
                double.TryParse(json, out double result);
                value = result;
                return true;
            }

            if (type == typeof(decimal))
            {
                decimal.TryParse(json, out decimal result);
                value = result;
                return true;
            }

            if (type == typeof(byte))
            {
                byte.TryParse(json, out byte result);
                value = result;
                return true;
            }

            if (type == typeof(char))
            {
                char.TryParse(json, out char result);
                value = result;
                return true;
            }

            // Speciall case for IntPtr, is IntPtr a built in type?

            if (type == typeof(IntPtr))
            {
                int.TryParse(json, out int result);
                value = new IntPtr(result);
                return true;
            }

            if (type == typeof(UIntPtr))
            {
                uint.TryParse(json, out uint result);
                value = new UIntPtr(result);
                return true;
            }

            // End of known non-nullable types... Is object a primitive???
            // If the type is object we need to try and parse to something that's not typeof(string), as this check has already been performed at the top of this method.
            if (type == typeof(object))
            {
                var lowerStr = json.ToLowerInvariant();
                if (lowerStr == "true" || lowerStr == "false")
                {
                    value = lowerStr == "true";
                    return true;
                }

                if (json.IndexOf('.') > -1)
                {
                    if (double.TryParse(json, out var doub))
                    {
                        value = doub;
                        return true;
                    }

                    if (decimal.TryParse(json, out var dec))
                    {
                        value = dec;
                        return true;
                    }
                }

                if (int.TryParse(json, out var integer))
                {
                    value = integer;
                    return true;
                }

                if (long.TryParse(json, out var int64))
                {
                    value = int64;
                    return true;
                }

                // object type so just return json as a string, this way some kind of object is returned...
                //value = json == "null" ? null : json;
                //return true;
            }

            // If the json value is null, and type is not a primitive, we can just return null and true as we don't need to instantiate a value for anything that is nullable,
            // although this isn't technically primitives. If we get to here we don't have a standard primitive type and json is not "null" so we need to return false. We can
            // simplify this a little by returning whether or not json is "null". Returning false will notify the caller that more checks on the value of json need to be done.
            value = null;
            return json == "null";
        }

        private uint ParseUnicode(char c1, char c2, char c3, char c4)
        {
            uint p1 = ParseUnicodeSingleChar(c1, 0x1000);
            uint p2 = ParseUnicodeSingleChar(c2, 0x100);
            uint p3 = ParseUnicodeSingleChar(c3, 0x10);
            uint p4 = ParseUnicodeSingleChar(c4, 1);

            return p1 + p2 + p3 + p4;
        }

        private uint ParseUnicodeSingleChar(char c1, uint multipliyer)
        {
            uint p1 = 0;
            if (c1 >= '0' && c1 <= '9')
                p1 = (uint)(c1 - '0') * multipliyer;
            else if (c1 >= 'A' && c1 <= 'F')
                p1 = (uint)((c1 - 'A') + 10) * multipliyer;
            else if (c1 >= 'a' && c1 <= 'f')
                p1 = (uint)((c1 - 'a') + 10) * multipliyer;
            return p1;
        }

        object ParseAnonymousValue(string json, StringSerializerFactory serializerFactory, StringBuilder stringBuilder, Stack<List<string>> splitArrayPool, bool includePrivates, bool parseBroken)
        {
            if (json.Length == 0)
            {
                return null;
            }

            if (json[0] == '{' && (parseBroken || json[json.Length - 1] == '}'))
            {
                List<string> elems = Split(json, stringBuilder, splitArrayPool);

                if (elems.Count % 2 != 0)
                {
                    if (!parseBroken)
                    {
                        return null;
                    }

                    if (elems.Count > 0)
                    {
                        elems.RemoveAt(elems.Count - 1);
                    }
                }

                var dict = new Dictionary<string, object>(elems.Count / 2);

                for (int i = 0; i < elems.Count; i += 2)
                {
                    var key = elems[i].Substring(1, elems[i].Length - 2);
                    
                    if (dict.ContainsKey(key))
                    {
                        continue;
                    }

                    dict.Add(key, ParseValue(typeof(object), elems[i + 1], serializerFactory, stringBuilder, splitArrayPool, includePrivates, parseBroken));
                }

                return dict;
            }

            if (json[0] == '[' && (parseBroken || json[json.Length - 1] == ']'))
            {
                List<string> items = Split(json, stringBuilder, splitArrayPool);
                var finalList = new List<object>(items.Count);

                for (int i = 0; i < items.Count; i++)
                {
                    finalList.Add(ParseAnonymousValue(items[i], serializerFactory, stringBuilder, splitArrayPool, includePrivates, parseBroken));
                }

                return finalList;
            }

            if (json[0] == '\"' && json[json.Length - 1] == '\"')
            {
                string str = json.Substring(1, json.Length - 2);
                return str.Replace("\\", string.Empty);
            }

            if (char.IsDigit(json[0]) || json[0] == '-')
            {
                if (json.Contains("."))
                {
                    double result;
                    double.TryParse(json, out result);
                    return result;
                }
                else
                {
                    int result;
                    int.TryParse(json, out result);
                    return result;
                }
            }

            if (json == "true")
            {
                return true;
            }

            if (json == "false")
            {
                return false;
            }

            // handles json == "null" as well as invalid JSON
            return null;
        }

        object ParseObject(Type type, string json, StringSerializerFactory serializerFactory, StringBuilder stringBuilder, Stack<List<string>> splitArrayPool, bool includePrivates, bool parseBroken)
        {
            object obj = null;

            var customSerializer = type.GetAttributes<JsonSerializer>().FirstOrDefault();

            if (customSerializer != null)
            {
                obj = customSerializer.FromString(json.RemoveDoubleQuotes(), type);
            }

            if (obj == null)
            {
                obj = serializerFactory.FromString(json.RemoveDoubleQuotes(), type);
            }

            if (obj != null)
            {
                return obj;
            }

            object instance = ReflectionUtils.GetInstanceOf(type);

            // The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON.
            List<string> elems = Split(json, stringBuilder, splitArrayPool);

            if (elems.Count % 2 != 0)
            {
                if (!parseBroken)
                {
                    return instance;
                }

                if (elems.Count > 0)
                {
                    elems.RemoveAt(elems.Count - 1);
                }
            }

            var props = instance.GetJsonPropertiesDictionary(includePrivates);
            
            for (int i = 0; i < elems.Count; i += 2)
            {
                if (elems[i].Length <= 2)
                {
                    continue;
                }

                string key = elems[i].Substring(1, elems[i].Length - 2);
                string value = elems[i + 1];

                if (props.TryGetValue(key, out var prop))
                {
                    // Only try to parse and set recursive properties or fields if they are writeable (not read only).
                    if (prop.Member.IsWritable())
                    {
                        if (prop.Serializer != null)
                        {
                            object serializerValue = prop.Serializer.FromString(value, prop.GetMemberType(instance));

                            if (serializerValue != null)
                            {
                                prop.SetValue(instance, serializerValue);
                                continue;
                            }
                        }

                        prop.SetValue(instance, ParseValue(prop.GetMemberType(instance), value, serializerFactory, stringBuilder, splitArrayPool, includePrivates, parseBroken));
                    }
                }
                else
                {
                    // Look for potential nested property paths where the JsonPathAttribute may have been used...
                    var paths = props.Where(x =>
                        !string.IsNullOrEmpty(x.Value.Path) &&
                            (x.Value.Path.StartsWith(key + ".", StringComparison.OrdinalIgnoreCase)
                            || x.Value.Path.StartsWith(key + "[", StringComparison.OrdinalIgnoreCase)));

                    if (paths != null && paths.Count() > 0)
                    {
                        foreach (var path in paths)
                        {
                            SetValueFromJsonPath(instance, paths, value, parseBroken);
                        }
                    }
                }
            }

            return instance;
        }

        
        /// <summary>
        /// 
        /// </summary>
        public Type[] GetGenericArguments(TypeInfo instance, Type baseType)
        {
            Type instanceType = instance.AsType();

            while (instanceType.BaseType != baseType && instanceType.BaseType != ObjectType)
            {
                instanceType = instanceType.BaseType;
            }

            if (instanceType.IsGenericType)
            {
                return instanceType.GetGenericArguments();
            }

            return null;
        }


        /// <summary>
        /// 
        /// </summary>
        private void SetValueFromJsonPath(object instance, IEnumerable<KeyValuePair<string, JsonProperty>> props, string json, bool parseBroken)
        {
            if (json.StartsWith('['))
            {
                var parsed = Json.ToList(json);

                foreach (var prop in props)
                {
                    SetValueFromJsonPath(instance, prop.Value, prop.Value.Path, parsed, parseBroken);
                }
            }
            else if (json.StartsWith('{'))
            {
                var parsed = Json.ToDictionary(json, true);

                foreach (var prop in props)
                {
                    SetValueFromJsonPath(instance, prop.Value, prop.Value.Path, parsed, parseBroken);
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void SetValueFromJsonPath(object instance, JsonProperty prop, string path, List<object> parsed, bool parseBroken)
        {
            if (parsed == null)
            {
                return;
            }

            var depth = path.Split('.');
            var square = depth[0].IndexOf('[');
            
            if (square < 0 || square + 1 >= depth[0].Length)
            {
                return;
            }

            var indexStr = depth[0].Substring(square + 1).Trim(']');

            if (!int.TryParse(indexStr, out var index) || parsed.Count <= index)
            {
                return;
            }

            var element = parsed.ElementAt(index);

            if (element != null)
            {
                if (depth.Length == 1)
                {
                    var memberType = prop.GetMemberType();

                    if (element.GetType() == memberType)
                    {
                        prop.SetValue(instance, element);
                    }
                    else
                    {
                        if (element is Dictionary<string, object> dic)
                        {
                            prop.SetValue(instance, Json.TypeFromDictionary(memberType, dic, true, parseBroken));
                        }
                        else
                        {
                            if (element is string str)
                            {
                                if (prop.Serializer != null)
                                {
                                    var deserialized = prop.Serializer.FromString(str, memberType);

                                    if (deserialized != null)
                                    {
                                        prop.SetValue(instance, deserialized);
                                        return;
                                    }
                                }
                                else
                                {
                                    var deserialized = Json.SerializationFactory.FromString(str, memberType);

                                    if (deserialized != null)
                                    {
                                        prop.SetValue(instance, deserialized);
                                        return;
                                    }
                                }
                            }

                            prop.SetValue(instance, element);
                        }
                    }
                }
                else
                {
                    if (element is Dictionary<string, object> dic)
                    {
                        SetValueFromJsonPath(instance, prop, string.Join('.', depth.Skip(1)), dic, parseBroken);
                    }
                    else if (element is List<object> list)
                    {
                        SetValueFromJsonPath(instance, prop, string.Join('.', depth.Skip(1)), list, parseBroken);
                    }
                }
            }
        }


        /// <summary>
        /// 
        /// </summary>
        private void SetValueFromJsonPath(object instance, JsonProperty prop, string path, Dictionary<string, object> parsed, bool parseBroken)
        {
            if (parsed == null)
            {
                return;
            }

            var depth = path.Split('.');
            
            if (parsed.TryGetValue(depth[0], out var element) && element != null)
            {
                if (depth.Length == 1)
                {
                    var memberType = prop.GetMemberType();

                    if (element?.GetType() == memberType)
                    {
                        prop.SetValue(instance, element);
                    }
                    else
                    {
                        if (element is Dictionary<string, object> dic)
                        {
                            prop.SetValue(instance, Json.TypeFromDictionary(memberType, dic, true, parseBroken));
                        }
                        else
                        {
                            if (element is string str)
                            {
                                if (prop.Serializer != null)
                                {
                                    var deserialized = prop.Serializer.FromString(str, memberType);

                                    if (deserialized != null)
                                    {
                                        prop.SetValue(instance, deserialized);
                                        return;
                                    }
                                }
                                else
                                {
                                    var deserialized = Json.SerializationFactory.FromString(str, memberType);

                                    if (deserialized != null)
                                    {
                                        prop.SetValue(instance, deserialized);
                                        return;
                                    }
                                }
                            }
                            
                            prop.SetValue(instance, element);
                        }
                    }
                }
                else
                {
                    if (element is Dictionary<string, object> dic)
                    {
                        SetValueFromJsonPath(instance, prop, string.Join('.', depth.Skip(1)), dic, parseBroken);
                    }
                    else if (element is List<object> list)
                    {
                        SetValueFromJsonPath(instance, prop, string.Join('.', depth.Skip(1)), list, parseBroken);
                    }
                }
            }
        }
    }
}