using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text;
using reblGreen.Serialization.Serializers;

namespace reblGreen.Serialization.JsonTools
{
    // Really simple JSON parser in ~300 lines
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
    // - Limited to parsing <2GB JSON files (due to int.MaxValue)
    // - Parsing of abstract classes or interfaces is NOT supported and will throw an exception.
    public class JsonReader
    {
        public T FromJson<T>(string json, StringSerializerFactory serializerFactory)
        {
            Stack<List<string>> splitArrayPool = new Stack<List<string>>();
            StringBuilder stringBuilder = new StringBuilder();

            // Remove all whitespace not within strings to make parsing simpler
            stringBuilder.Length = 0;

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
            return (T)ParseValue(typeof(T), stringBuilder.ToString(), serializerFactory, stringBuilder, splitArrayPool);
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

        internal object ParseValue(Type type, string json, StringSerializerFactory serializerFactory, StringBuilder stringBuilder, Stack<List<string>> splitArrayPool)
        {
            var obj = serializerFactory.FromString(json.RemoveDoubleQuotes(), type);

            if (obj != null)
            {
                return obj;
            }
            else if (type == typeof(string) || (type == typeof(object) && json[0] == '"' && json[json.Length - 1] == '"'))
            {
                if (json.Length <= 2)
                {
                    return string.Empty;
                }

                StringBuilder sb = new StringBuilder();
                for (int i = 1; i < json.Length - 1; ++i)
                {
                    if (json[i] == '\\' && i + 1 < json.Length - 1)
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

                                // Attempt to parse the 32 bit hex into an ansi char code (skipping the \u)
                                uint unicode = ParseUnicode(json[i + 2], json[i + 3], json[i + 4], json[i + 5]);

                                // If unicode is greater than the max ansi code continue as normal.
                                if (unicode > 255)
                                {
                                    sb.Append(json[i]);
                                }
                                else
                                {
                                    // Append the unicode char and skip the next 4 chars in the json.
                                    sb.Append((char)unicode);
                                    i += 4;
                                }
                                break;
                            case '/':
                                // Special case for json where forward slashes are escaped {http:\\/\\/example.com\\/)
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

                return sb.ToString();
            }

            if (type == typeof(short))
            {
                short.TryParse(json, out short result);
                return result;
            }

            if (type == typeof(ushort))
            {
                ushort.TryParse(json, out ushort result);
                return result;
            }

            if (type == typeof(long))
            {
                long.TryParse(json, out long result);
                return result;
            }

            if (type == typeof(ulong))
            {
                ulong.TryParse(json, out ulong result);
                return result;
            }

            if (type == typeof(int))
            {
                int.TryParse(json, out int result);
                return result;
            }

            if (type == typeof(uint))
            {
                uint.TryParse(json, out uint result);
                return result;
            }

            if (type == typeof(byte))
            {
                byte.TryParse(json, out byte result);
                return result;
            }

            if (type == typeof(float))
            {
                float.TryParse(json, out float result);
                return result;
            }

            if (type == typeof(double))
            {
                double.TryParse(json, out double result);
                return result;
            }

            if (type == typeof(bool))
            {
                return json.ToLower() == "true";
            }

            if (json == "null")
            {
                return null;
            }

            if (type.IsArray)
            {
                json = json.RemoveDoubleQuotes();
                Type arrayType = type.GetElementType();

                if (json[0] != '[' || json[json.Length - 1] != ']')
                {
                    return null;
                }

                List<string> elems = Split(json, stringBuilder, splitArrayPool);
                Array newArray = Array.CreateInstance(arrayType, elems.Count);

                for (int i = 0; i < elems.Count; i++)
                {
                    newArray.SetValue(ParseValue(arrayType, elems[i], serializerFactory, stringBuilder, splitArrayPool), i);
                }

                splitArrayPool.Push(elems);

                return newArray;
            }

            var info = type.GetTypeInfoCached();

            if (info.IsEnum)
            {
                return Enum.Parse(type, json.RemoveDoubleQuotes(), true);
            }

            if (info.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
            {
                json = json.RemoveDoubleQuotes();
                Type listType = info.GenericTypeArguments[0];

                if (json[0] != '[' || json[json.Length - 1] != ']')
                {
                    return null;
                }

                List<string> elems = Split(json, stringBuilder, splitArrayPool);

                var list = (IList)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Count });

                for (int i = 0; i < elems.Count; i++)
                {
                    list.Add(ParseValue(listType, elems[i], serializerFactory, stringBuilder, splitArrayPool));
                }

                splitArrayPool.Push(elems);

                return list;
            }
            if (info.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
            {
                json = json.RemoveDoubleQuotes();

                Type keyType, valueType;
                {
                    Type[] args = info.GenericTypeArguments;
                    keyType = args[0];
                    valueType = args[1];
                }

                // Refuse to parse dictionary keys that aren't of type string.
                if (keyType != typeof(string))
                {
                    return null;
                }
            
                // Must be a valid dictionary element.
                if (json[0] != '{' || json[json.Length - 1] != '}')
                {
                    return null;
                }

                // The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON.
                List<string> elems = Split(json, stringBuilder, splitArrayPool);

                if (elems.Count % 2 != 0)
                {
                    return null;
                }


                var dictionary = (IDictionary)type.GetConstructor(new Type[] { typeof(int) }).Invoke(new object[] { elems.Count / 2 });

                for (int i = 0; i < elems.Count; i += 2)
                {
                    if (elems[i].Length <= 2)
                    {
                        continue;
                    }

                    string keyValue = elems[i].Substring(1, elems[i].Length - 2);
                    object val = ParseValue(valueType, elems[i + 1], serializerFactory, stringBuilder, splitArrayPool);
                    dictionary.Add(keyValue, val);
                }

                return dictionary;
            }

            if (type == typeof(object))
            {
                return ParseAnonymousValue(json, serializerFactory, stringBuilder, splitArrayPool);
            }

            if (json[0] == '{' && json[json.Length - 1] == '}')
            {
                return ParseObject(type, json, serializerFactory, stringBuilder, splitArrayPool);
            }

            return null;
        }


        private uint ParseUnicode(char c1, char c2, char c3, char c4)
        {
            uint p1 = ParseSingleChar(c1, 0x1000);
            uint p2 = ParseSingleChar(c2, 0x100);
            uint p3 = ParseSingleChar(c3, 0x10);
            uint p4 = ParseSingleChar(c4, 1);

            return p1 + p2 + p3 + p4;
        }

        private uint ParseSingleChar(char c1, uint multipliyer)
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

        object ParseAnonymousValue(string json, StringSerializerFactory serializerFactory, StringBuilder stringBuilder, Stack<List<string>> splitArrayPool)
        {
            if (json.Length == 0)
            {
                return null;
            }

            if (json[0] == '{' && json[json.Length - 1] == '}')
            {
                List<string> elems = Split(json, stringBuilder, splitArrayPool);

                if (elems.Count % 2 != 0)
                {
                    return null;
                }

                var dict = new Dictionary<string, object>(elems.Count / 2);

                for (int i = 0; i < elems.Count; i += 2)
                {
                    dict.Add(elems[i].Substring(1, elems[i].Length - 2), ParseValue(typeof(object), elems[i + 1], serializerFactory, stringBuilder, splitArrayPool));
                }

                return dict;
            }

            if (json[0] == '[' && json[json.Length - 1] == ']')
            {
                List<string> items = Split(json, stringBuilder, splitArrayPool);
                var finalList = new List<object>(items.Count);

                for (int i = 0; i < items.Count; i++)
                {
                    finalList.Add(ParseAnonymousValue(items[i], serializerFactory, stringBuilder, splitArrayPool));
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

        object ParseObject(Type type, string json, StringSerializerFactory serializerFactory, StringBuilder stringBuilder, Stack<List<string>> splitArrayPool)
        {
            var obj = serializerFactory.FromString(json.RemoveDoubleQuotes(), type);

            if (obj != null)
            {
                return obj;
            }

            object instance = ReflectionUtils.GetInstanceOf(type);

            // The list is split into key/value pairs only, this means the split must be divisible by 2 to be valid JSON
            List<string> elems = Split(json, stringBuilder, splitArrayPool);

            if (elems.Count % 2 != 0)
            {
                return instance;
            }

            var props = instance.GetJsonPropertiesDictionary();

            for (int i = 0; i < elems.Count; i += 2)
            {
                if (elems[i].Length <= 2)
                {
                    continue;
                }

                string key = elems[i].Substring(1, elems[i].Length - 2);
                string value = elems[i + 1];

                if (props.ContainsKey(key))
                {
                    var prop = props[key];
                    prop.SetValue(instance, ParseValue(prop.GetMemberType(instance), value, serializerFactory, stringBuilder, splitArrayPool));
                }
            }

            return instance;
        }
    }
}