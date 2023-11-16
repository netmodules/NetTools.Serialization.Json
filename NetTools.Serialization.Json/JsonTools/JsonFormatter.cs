using System;
using System.Text;

namespace NetTools.Serialization.JsonTools
{
    [Serializable]
    internal static class JsonFormatter
    {
        private const string Indent = "  ";

        public static string PrettyPrint(string jsonString)
        {
            var output = new StringBuilder();
            var depth = 0;
            
            for (var i = 0; i < jsonString.Length; ++i)
            {
                var c = jsonString[i];

                if (c == '"') // Found string litteral so itterate to string end (").
                {
                    var str = true;

                    while (str)
                    {
                        output.Append(c);

                        if (i == jsonString.Length - 1)
                        {
                            str = false;
                            break;
                        }

                        c = jsonString[++i];
                        if (c == '\\')
                        {
                            output.Append(c);
                            c = jsonString[++i];
                        }
                        else if (c == '"')
                        {
                            str = false;
                        }
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
        /// A helper method for removing standard single line and multiline comments out of a JSON object. This will also remove 
        /// any whitespace characters, essentially minifying the JSON object. If you need to reinsert whitespace characters
        /// use <see cref="Json.BeautifyJson(string)"/>.
        /// </summary>
        public static string RemoveCommentsAndWhiteSpace(string jsonString)
        {
            /* The statement below is from the creator of JSON Doublas Crockford and is included here for people who say that
             * a JSON object should NOT contain comments...
             * 
             * Comments were removed from JSON by design.
             * 
             * I removed comments from JSON because I saw people were using them to hold parsing directives, a practice which
             * would have destroyed interoperability.I know that the lack of comments makes some people sad, but it shouldn't.
             * 
             * Suppose you are using JSON to keep configuration files, which you would like to annotate. Go ahead and insert
             * all the comments you like.Then pipe it through JSMin before handing it to your JSON parser.
             * 
             * Source: Public statement by Douglas Crockford on G+ https://plus.google.com/118095276221607585885/posts/RK8qyGVaGSr
             */

            if (string.IsNullOrWhiteSpace(jsonString))
            {
                return string.Empty;
            }

            var output = new StringBuilder();

            var isSingleComment = false;
            var isMultilineComment = false;
            var isString = false;
            var slashes = 0;

            for (var i = 0; i < jsonString.Length; i++)
            {
                var c = jsonString[i];

                if (!isMultilineComment && !isSingleComment)
                {
                    // We're not inside a comment...

                    if (c == '"')
                    {
                        // Did we find a string literal?
                        // We need to do a look behind to see if the string terminator is escaped with backslashes. If it is
                        // escaped then it doesn't terminate the string, so if we're currently in a string we should stay in it.

                        slashes = 0;
                        for (var j = i - 1; j > 0; j--)
                        {
                            if (jsonString[j] != '\\')
                            {
                                break;
                            }

                            slashes += 1;
                        }

                        // If slashes is divisable by 2 then the string literal is not escaped. For example a string which contains
                        // an escaped doublequote at the end would look like "...\"" which becomes slashes = 1 to which 1 / 2 = 0.5
                        // so we are still inside the string literal.

                        if (slashes % 2 == 0)
                        {
                            isString = !isString;
                        }
                    }

                    if (isString)
                    {
                        // If we're inside a string literal we need to append all characters including whitespace and anything that
                        // would resemble a single line or multiline comment.

                        output.Append(c);
                    }
                    else
                    {
                        if (c == '#')
                        {
                            // If we're outside of a string literal then we know the only valid JSON characters are { [ " : , and
                            // that anything which is not bool, numeric or null must be wrapped in doublequotes. If we find a hash
                            // we treat it as the start of a comment.

                            isSingleComment = true;
                            i++;
                        }
                        else if (c == '/' && i + 1 < jsonString.Length)
                        {
                            // As with the above statement, anything which is outside of valid JSON we know if a foreign character.
                            // Check to see if the next character is also a / and if it is then it's a // - The start of a single
                            // line comment. If the next char is a * then it's /* - The start of a multiline comment. Since we're
                            // looking forward +1 char here, we can increment i if the condition is met.

                            if (jsonString[i + 1] == '/')
                            {
                                isSingleComment = true;
                                i++;
                            }
                            else if (jsonString[i + 1] == '*')
                            {
                                isMultilineComment = true;
                                i++;
                            }
                        }
                        else
                        {
                            // We're not inside a string literal and we're not inside a comment, so if the character is not whitespace
                            // then append it to our output string.

                            if (char.IsWhiteSpace(c))
                            {
                                continue;
                            }

                            output.Append(c);
                        }
                    }
                }
                else if (isSingleComment && (c == '\n' || c == '\r'))
                {
                    // If we're inside a single line comment, we look for a line terminator and pull out of the comment when we find one.
                    isSingleComment = false;
                }
                else if (isMultilineComment && c == '*' && i + 1 < jsonString.Length && jsonString[i + 1] == '/')
                {
                    // As with the statement above, if we're inside a multiline comment we look for the terminating */ for multiline comments.
                    isMultilineComment = false;
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
