using NetTools.Serialization.JsonTools;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace NetTools.Serialization
{
    public static class StringExtensions
    {
        // \A\s*("([^"\\]*|\\["\\bfnrt\/]|\\u[0-9a-f]{4})*"|-?(?=[1-9]|0(?!\d))\d+(\.\d+)?([eE][+-]?\d+)?|true|false|null|\[\s*(?:(?1)(?:,\s*(?1))*)?\s*\]|\{(?:\s*"([^"\\]*|\\["\\bfnrt\/]|\\u[0-9a-f]{4})*"\s*:\s*(?1)(?:\s*,\s*"([^"\\]*|\\["\\bfnrt\/]|\\u[0-9a-f]{4})*"\s*:\s*(?1))*)?\s*\})\s*\Z
        //internal static Regex JsonMatch
            //= new Regex("\\A\\s*(\"([^\"\\\\]*|\\\\[\"\\\\bfnrt\\/]|\\\\u[0-9a-f]{4})*\"|-?(?=[1-9]|0(?!\\d))\\d+(\\.\\d+)?([eE][+-]?\\d+)?|true|false|null|\\[\\s*(?:(?1)(?:,\\s*(?1))*)?\\s*\\]|\\{(?:\\s*\"([^\"\\\\]*|\\\\[\"\\\\bfnrt\\/]|\\\\u[0-9a-f]{4})*\"\\s*:\\s*(?1)(?:\\s*,\\s*\"([^\"\\\\]*|\\\\[\"\\\\bfnrt\\/]|\\\\u[0-9a-f]{4})*\"\\s*:\\s*(?1))*)?\\s*\\})\\s*\\Z", RegexOptions.Compiled);
        //    = new Regex("\\A\\s*(\"([^\"\\\\]*|\\\\[\"\\\\bfnrt\\/]|\\\\u[0-9a-f]{4})*\"|-?(?=[1-9]|0(?!\\d))\\d+(\\.\\d+)?([eE][+-]?\\d+)?|true|false|null|\\[\\s*(?:( \\1)(?:,\\s*(\\1))*)?\\s*\\]|\\{(?:\\s*\"([^\"\\\\]*|\\\\[\"\\\\bfnrt\\/]|\\\\u[0-9a-f]{4})*\"\\s*:\\s*(\\1)(?:\\s*,\\s*\"([^\"\\\\]*|\\\\[\"\\\\bfnrt\\/]|\\\\u[0-9a-f]{4})*\"\\s*:\\s*(\\1))*)?\\s*\\})\\s*\\Z", RegexOptions.Compiled);


        ///// <summary>
        ///// 
        ///// </summary>
        //public static bool IsJson(this string s)
        //{
        //    return JsonMatch.IsMatch(Json.MinifyJson(s));
        //}


        /// <summary>
        /// 
        /// </summary>
        public static bool IsValidJson(this string s, out string invalid, bool allowComments = true, bool allowTrailingCommas = true, bool allowUnescapedNewlines = true)
        {
            try
            {
                ReadOnlySpan<byte> data = null;

                if (allowComments)
                {
                    s = JsonFormatter.RemoveCommentsAndWhiteSpace(s);
                }

                if (allowUnescapedNewlines)
                {
                    s = s.Replace("\r", string.Empty).Replace("\n", string.Empty);
                }
                
                data = Encoding.UTF8.GetBytes(s);
                
                var reader = new Utf8JsonReader(data, new JsonReaderOptions()
                {
                    CommentHandling = allowComments ? JsonCommentHandling.Skip : JsonCommentHandling.Disallow,
                    AllowTrailingCommas = allowTrailingCommas ? true : false,
                });
                reader.Read();
                reader.Skip();
                invalid = null;
                return true;
            }
            catch (Exception ex)
            {
                invalid = ex.Message;
                return false;
            }
        }
    }
}
