using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using NetTools.Serialization.Serializers;

namespace NetTools.Serialization.JsonTools
{
    // This class was originally forked (and has been heavily extended) from "Really simple JSON writer"
    // - Outputs JSON structures from an object
    // - Really simple API (new List<int> { 1, 2, 3 }).ToJson() == "[1,2,3]"
    // - Will only output public fields and property getters on objects
    internal class JsonWriter
    {
        /// <summary>
        /// 
        /// </summary>
        public string ToJson(object item, StringSerializerFactory serializerFactory, Dictionary<Type, List<string>> nonSerialized = null, bool appendEmpty = false, bool includePrivates = false)
        {
            StringBuilder stringBuilder = new StringBuilder();

            try
            {
                AppendValue(stringBuilder, item, serializerFactory, nonSerialized, appendEmpty, includePrivates);
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to serialize property.\r\nCurrent JSON Position:\r\n{stringBuilder}", ex);
            }

            return stringBuilder.ToString();
        }


        /// <summary>
        /// 
        /// </summary>
        void AppendValue(StringBuilder stringBuilder, object item, StringSerializerFactory serializerFactory, Dictionary<Type, List<string>> nonSerialized = null, bool appendEmpty = false, bool includePrivates = false)
        {
            if (item == null)
            {
                stringBuilder.Append("null");
                return;
            }

            try
            {
                // If our serialization factory has a custom serializer we append the returned value and return.
                var s = serializerFactory.ToString(item);

                if (!string.IsNullOrEmpty(s))
                {
                    stringBuilder.Append(s);
                    return;
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Unable to serialize object \"{item.ToString()}\".\r\nCurrent JSON Position:\r\n{stringBuilder}", ex);
            }


            // No custom serializer so continue to identify and serialize the object.
            var type = item.GetType();

            
            if (typeof(Exception).IsAssignableFrom(type) || typeof(Assembly).IsAssignableFrom(type))
            {
                stringBuilder.Append('"');
                stringBuilder.Append(item.ToString());
                stringBuilder.Append('"');
                return;
            }

            if (type == typeof(string))
            {
                stringBuilder.Append('"');
                var str = (string)item;

                for (int i = 0; i < str.Length; ++i)
                    switch (str[i])
                    {
                        case '\\':
                            stringBuilder.Append("\\\\");
                            break;
                        case '\"':
                            stringBuilder.Append("\\\"");
                            break;
                        case '\b':
                            stringBuilder.Append("\\b");
                            break;
                        case '\f':
                            stringBuilder.Append("\\f");
                            break;
                        case '\t':
                            stringBuilder.Append("\\t");
                            break;
                        case '\n':
                            stringBuilder.Append("\\n");
                            break;
                        case '\r':
                            stringBuilder.Append("\\r");
                            break;
                        case '\0':
                            stringBuilder.Append("\\0");
                            break;
                        case '/':
                            // Escaping forward slashes (http:\/\/example.com\/)
                            // See https://www.json.org/ for specification.
                            stringBuilder.Append("\\/");
                            break;
                        default:
                            // It seems that most json serializers convert any char after a tild (~) into a unicode value. The hexi value of tild
                            // is 1F and character number 159. Character 160 (Hex A0) is a https://en.wikipedia.org/wiki/Non-breaking_space
                            // non-breaking space (&nbsp;) and needs to be encoded.
                            if (str[i] > 159)
                            {
                                stringBuilder.Append("\\u");
                                stringBuilder.Append(((int)str[i]).ToString("X4", NumberFormatInfo.InvariantInfo));
                            }
                            else
                            {
                                stringBuilder.Append(str[i]);
                            }

                            break;
                    }

                stringBuilder.Append('"');
            }
            else if (type == typeof(float))
            {
                var f = (float)item;
                stringBuilder.Append(float.IsNaN(f) || float.IsInfinity(f) ? "null" : f.ToString());
            }
            else if (type == typeof(double))
            {
                var d = (double)item;
                stringBuilder.Append(double.IsNaN(d) || double.IsInfinity(d) ? "null" : d.ToString());
            }
            else if (type == typeof(byte) 
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(decimal)
                || type == typeof(IntPtr)
                || type == typeof(UIntPtr))
            {
                stringBuilder.Append(item.ToString());
            }
            else if (type == typeof(bool))
            {
                stringBuilder.Append(((bool)item) ? "true" : "false");
            }
            else if (item is IList)
            {
                stringBuilder.Append('[');

                var isFirst = true;
                var list = item as IList;

                for (int i = 0; i < list.Count; i++)
                {
                    if (isFirst)
                    {
                        isFirst = false;
                    }
                    else
                    {
                        stringBuilder.Append(',');
                    }


                    try
                    {
                        AppendValue(stringBuilder, list[i], serializerFactory, nonSerialized, appendEmpty, includePrivates);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Unable to serialize object \"{list[i].ToString()}\".\r\nCurrent JSON Position:\r\n{stringBuilder.ToString()}", ex);
                    }
                }

                stringBuilder.Append(']');
            }
            else
            {
                var info = type.GetTypeInfoCached();

                // TODO: Needs revising...
                // Dirty quick fix for classes inheriting from Dictionary<>.
                if (!info.IsGenericType && info.BaseType != null)
                {
                    if (info.BaseType.IsGenericType && info.BaseType.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                    {
                        info = info.BaseType.GetTypeInfoCached();
                    }
                }

                if (info.IsEnum)
                {
                    var val = item.ToString();

                    if (Json.AutoCamelCase)
                    {
                        stringBuilder.Append((char.ToLowerInvariant(val[0]) + (val.Length > 1 ? val.Substring(1) : "")).AddDoubleQuotes());
                    }
                    else
                    {
                        stringBuilder.Append(val.AddDoubleQuotes());
                    }
                }
                else if (info.IsGenericType && (info.GetGenericTypeDefinition() == typeof(Dictionary<,>) || item is IDictionary))
                {
                    var keyType = info.GenericTypeArguments[0];
                    
                    // If our serialization factory has a custom serializer...
                    var hasSerializer = serializerFactory.HasSerializer(keyType);

                    // Refuse to output dictionary keys that aren't of type string or IConvertible
                    if (!hasSerializer && keyType != typeof(string) &&
                        (!Json.AutoCastKeys || (!keyType.IsEnum && !typeof(IConvertible).IsAssignableFrom(keyType))))
                    {
                        stringBuilder.Append("{}");
                        return;
                    }

                    stringBuilder.Append('{');

                    var dict = item as IDictionary;
                    var isFirst = true;
                    var isEnum = keyType.IsEnum;

                    foreach (object key in dict.Keys)
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                        }
                        else
                        {
                            stringBuilder.Append(',');
                        }

                        if (hasSerializer)
                        {
                            stringBuilder.Append(serializerFactory.ToString(key));
                            stringBuilder.Append(':');
                        }
                        else
                        {
                            stringBuilder.Append('\"');

                            if (isEnum)
                            {
                                var val = key.ToString();

                                if (Json.AutoCamelCase)
                                {
                                    stringBuilder.Append(char.ToLowerInvariant(val[0]) + (val.Length > 1 ? val.Substring(1) : ""));
                                }
                                else
                                {
                                    stringBuilder.Append(val);
                                }
                            }
                            else
                            {
                                // If key is not a string type we can just force it to a string here as we have
                                // already checked that the keyType is IConvertible...
                                var val = key is string k ? k : key.ToString();

                                if (Json.AutoCamelCase && Json.ForceCamelCaseStringKeys)
                                {
                                    stringBuilder.Append(char.ToLowerInvariant(val[0]) + (val.Length > 1 ? val.Substring(1) : ""));
                                }
                                else
                                {
                                    stringBuilder.Append(val);
                                }
                            }

                            stringBuilder.Append("\":");
                        }

                        try
                        {
                            AppendValue(stringBuilder, dict[key], serializerFactory, nonSerialized, appendEmpty, includePrivates);
                        }
                        catch (Exception ex)
                        {
                            throw new Exception($"Unable to serialize property named \"{key}\".\r\nCurrent JSON Position:\r\n{stringBuilder}", ex);
                        }
                    }

                    stringBuilder.Append('}');
                }
                else
                {
                    stringBuilder.Append('{');

                    var isFirst = true;
                    var props = item.GetJsonProperties(includePrivates, nonSerialized);

                    for (int i = 0; i < props.Count; i++)
                    {
                        var prop = props[i];
                        object value = null;

                        try
                        {
                            value = prop.GetValue(item);
                        }
                        catch
                        {
                            continue;
                        }

                        if (value != null || appendEmpty)
                        {
                            if (isFirst)
                            {
                                isFirst = false;
                            }
                            else
                            {
                                stringBuilder.Append(',');
                            }

                            stringBuilder.Append('\"');
                            stringBuilder.Append(props[i].Name);
                            stringBuilder.Append("\":");

                            try
                            {
                                if (prop.Serializer != null)
                                {
                                    string serializerValue = prop.Serializer.ToString(value);

                                    if (serializerValue != null)
                                    {
                                        stringBuilder.Append(serializerValue);
                                        continue;
                                    }
                                }

                                AppendValue(stringBuilder, value, serializerFactory, nonSerialized, appendEmpty, includePrivates);
                            }
                            catch (Exception ex)
                            {
                                throw new Exception($"Unable to serialize property named \"{props[i].Name}\".\r\nCurrent JSON Position:\r\n{stringBuilder}", ex);
                            }
                        }
                    }

                    stringBuilder.Append('}');
                }
            }
        }
    }
}