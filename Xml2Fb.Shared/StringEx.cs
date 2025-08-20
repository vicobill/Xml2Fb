using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xml2Fb.Shared
{
    public static class StringEx
    {
        public static string Posix(this string path)
        {
            return path.Replace( '\\', '/' );
        }

        public static string Standard(this string path)
        {
            return path.Replace( Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar );
        }

        public static bool IsBetween( this char c, char min, char max )
        => ( c - min ) <= ( max - min );
        public static bool IsAsciiLower( this char c ) => IsBetween( c, 'a', 'z' );
        public static bool IsAsciiUpper( this char c ) => IsBetween( c, 'A', 'Z' );
        public static char UpperChar( this char c )
        {
            if (IsAsciiLower( c ))
            {
                c = (char)( c & 0x5F );
            }
            return c;
        }

        /// <summary>
        /// 首字母大写
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Capitalize( this string s )
        {
            if (char.IsUpper( s[0] ))
                return s;
            Span<char> chars = s.ToCharArray();

            chars[0] = UpperChar( chars[0] );
            return chars.ToString();
        }

        /// <summary>
        /// 首字母小写
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string LowerCapital(this string s )
        {
            if (!char.IsUpper( s[0] ))
                return s;

            Span<char> span = s.ToCharArray();
            span[0] = char.ToLower( span[0] );
            return span.ToString();
        }

        /// <summary>
        /// 去掉空白字符串、将'-'转成'_'， 用于合规代码命名
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string Codize( this string s )
        {
            StringBuilder sb = new StringBuilder();
            foreach (char c in s)
            {
                if (char.IsWhiteSpace( c ))
                { continue; }
                if (c == '-')
                {
                    sb.Append( '_' );
                    continue;
                }
                sb.Append( c );

            }
            return sb.ToString();
        }

        /// <summary>
        /// 转成小写 snake_case
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string LowerSnakeCase( this string s )
        {
            Span<char> chars = s.ToCharArray();

            // dbcTitle => dbc_title
            // DbcTitle => dbc_title
            // DBCTitle => dbc_title
            StringBuilder sb = new StringBuilder();

            for (int i = 0; i < chars.Length; ++i)
            {
                char ch = chars[i];


                bool isLetter = char.IsLetter( ch );
                if (!isLetter)
                {
                    sb.Append( ch );
                    continue;
                }
                char l = char.ToLower( ch );
                bool isUpper = char.IsUpper( ch );
                bool isNextCharUpper = false;
                bool isNextCharLetter = false;
                if (i < chars.Length - 1)
                {
                    isNextCharUpper = char.IsUpper( chars[i + 1] );
                    isNextCharLetter = char.IsLetter( chars[i + 1] );
                }

                if (isUpper && isNextCharUpper)
                { sb.Append( l ); continue; }

                if (isUpper && !isNextCharLetter)
                { sb.Append( l ); continue; }

                // Ti => _ti
                // _Ti => _ti
                if (isUpper && !isNextCharUpper)
                {
                    if (i != 0 && chars[i - 1] != '_')
                        sb.Append( '_' );
                    sb.Append( l );
                    continue;
                }

                sb.Append( l );
            }
            return sb.ToString();
        }

        /// <summary>
        /// 将字符串转成字段名
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToFieldName( this string s ) // aBc-3z0
        {
            
            string ret = s.Replace( '-', '_' );// aBc_3z0
            if (ret.IndexOf( '_' ) == -1)
            {
                ret = StringUtils.ToCamelCase( ret ).ToLower();
                return ret;
            }

            ret = ret.LowerCapital().ToLower();
            return ret;
        }
            
        /// <summary>
        /// 转成Fbs TableName，格式是首字母大写
        /// </summary>
        /// <param name="s"></param>
        /// <returns></returns>
        public static string ToTableName( this string s )
            => s.Codize().Capitalize();

        public static string UpperCamelCase( this string s )
        {
            StringBuilder sb = new StringBuilder();
            foreach (var e in s.Split( '_' ))
            {
                sb.Append( e.Capitalize() );
            }
            return sb.ToString();
        }

        public static string LowerCamelCase( this string s )
        {
            StringBuilder sb = new StringBuilder();

            var words = s.Split( '_' );
            for (int i = 0; i < words.Length; ++i)
            {
                if (i == 0)
                {
                    char c = char.ToLower( words[i][0] );
                    sb.Append( c );
                    sb.Append( words[i][1..( words[i].Length )] );
                    continue;
                }
                if (int.TryParse( words[i],out int x ))
                {
                    sb.Append( '_' ).Append( words[i] );
                    continue;
                }
                sb.Append( words[i].Capitalize() );
            }

            return sb.ToString();
        }
    }
}
