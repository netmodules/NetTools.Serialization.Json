using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using reblGreen.Serialization.Serializers;

namespace reblGreen.Serialization.JsonTools
{
    //Really simple JSON writer
    //- Outputs JSON structures from an object
    //- Really simple API (new List<int> { 1, 2, 3 }).ToJson() == "[1,2,3]"
    //- Will only output public fields and property getters on objects
    public class JsonWriter
    {

        public string ToJson(object item, StringSerializerFactory serializerFactory)
        {
            StringBuilder stringBuilder = new StringBuilder();
            AppendValue(stringBuilder, item, serializerFactory);
            return stringBuilder.ToString();
        }

        void AppendValue(StringBuilder stringBuilder, object item, StringSerializerFactory serializerFactory)
        {
            if (item == null)
            {
                stringBuilder.Append("null");
                return;
            }

            Type type = item.GetType();
            
            var s = serializerFactory.ToString(item);

            if (!string.IsNullOrEmpty(s))
            {
                stringBuilder.Append(s.AddDoubleQuotes());
            }
            else if (type == typeof(string))
            {
                stringBuilder.Append('"');
                string str = (string)item;
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
                        default:
                            stringBuilder.Append(str[i]);
                            break;
                    }
                stringBuilder.Append('"');
            }
            else if (type == typeof(byte) 
                || type == typeof(short)
                || type == typeof(ushort)
                || type == typeof(long)
                || type == typeof(ulong)
                || type == typeof(int)
                || type == typeof(uint)
                || type == typeof(float)
                || type == typeof(double))
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
                bool isFirst = true;
                IList list = item as IList;
                for (int i = 0; i < list.Count; i++)
                {
                    if (isFirst)
                        isFirst = false;
                    else
                        stringBuilder.Append(',');
                    AppendValue(stringBuilder, list[i], serializerFactory);
                }
                stringBuilder.Append(']');
            }
            else
            {
                var info = type.GetTypeInfoCached();

                if (info.IsEnum)
                {
                    stringBuilder.Append(item.ToString().ToLowerInvariant().AddDoubleQuotes());
                }
                else if (info.IsGenericType && type.GetGenericTypeDefinition() == typeof(Dictionary<,>))
                {
                    Type keyType = info.GenericTypeArguments[0];

                    //Refuse to output dictionary keys that aren't of type string
                    if (keyType != typeof(string))
                    {
                        stringBuilder.Append("{}");
                        return;
                    }

                    stringBuilder.Append('{');
                    IDictionary dict = item as IDictionary;
                    bool isFirst = true;
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

                        stringBuilder.Append('\"');
                        stringBuilder.Append((string)key);
                        stringBuilder.Append("\":");
                        AppendValue(stringBuilder, dict[key], serializerFactory);
                    }
                    stringBuilder.Append('}');
                }
                else
                {
                    stringBuilder.Append('{');

                    bool isFirst = true;

                    var props = item.GetJsonProperties();

                    for (int i = 0; i < props.Count; i++)
                    {
                        object value = props[i].GetValue(item);

                        if (value != null)
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
                            AppendValue(stringBuilder, value, serializerFactory);
                        }
                    }

                    stringBuilder.Append('}');
                }
            }
        }
    }
}