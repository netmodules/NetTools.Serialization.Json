using System;
using System.Text;

namespace reblGreen.Serialization.JsonTools
{
    [Serializable]
    internal static class JsonFormatter
    {
        private const string Indent = "  ";

        public static string PrettyPrint(string input)
        {
            var output = new StringBuilder();
            var depth = 0;
            var len = input.Length;
            var chars = input.ToCharArray();

            for (var i = 0; i < len; ++i)
            {
                var c = chars[i];

                if (c == '\"') // Found string span so itterate to string end (").
                {
                    var str = true;

                    while (str)
                    {
                        output.Append(c);
                        c = chars[++i];
                        if (c == '\\')
                        {
                            output.Append(c);
                            c = chars[++i];
                        }
                        else if (c == '\"')
                            str = false;
                    }
                }

                switch (c)
                {
                    case '{':
                    case '[':
                        output.Append(c);
                        output.AppendLine();
                        output.AppendIndent(++depth);
                        break;
                    case '}':
                    case ']':
                        output.AppendLine();
                        output.AppendIndent(--depth);
                        output.Append(c);
                        break;
                    case ',':
                        output.Append(c);
                        output.AppendLine();
                        output.AppendIndent(depth);
                        break;
                    case ':':
                        output.Append(" : ");
                        break;
                    default:
                        // Strip any old whitespace...
                        if (!char.IsWhiteSpace(c))
                        {
                            output.Append(c);
                        }
                        break;
                }
            }

            return output.ToString();
        }


        /// <summary>
        /// A simple extension method to append indent to a StringBuilder. Using an extension method adds similar design pattern to existing sb functions.
        /// </summary>
        static void AppendIndent(this StringBuilder sb, int count)
        {
            for (; count > 0; --count)
            {
                sb.Append(Indent);
            }
        }
    }
}
