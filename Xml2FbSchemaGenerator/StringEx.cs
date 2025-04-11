using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xml2FbSchemaGenerator
{
    public static class StringEx
    {
        internal static bool IsBetween( this char c, char min, char max )
        => ( c - min ) <= ( max - min );
        internal static bool IsAsciiLower( this char c ) => IsBetween( c, 'a', 'z' );
        internal static bool IsAsciiUpper( this char c ) => IsBetween( c, 'A', 'Z' );
        internal static char UpperChar( this char c )
        {
            if (IsAsciiLower( c ))
            {
                c = (char)( c & 0x5F );
            }
            return c;
        }

        internal static string Capitalize( this string s )
        {
            Span<char> chars = s.ToCharArray();

            chars[0] = UpperChar( chars[0] );
            return chars.ToString();
        }


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
                if ( !isLetter)
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
                {  sb.Append( l ); continue; }

                // Ti => _ti
                // _Ti => _ti
                if (isUpper && !isNextCharUpper)
                {
                    if (i != 0 && chars[i - 1] != '_')
                        sb.Append( '_' );
                    sb.Append( l );
                    continue;
                }

                sb.Append(l);
            }
            return sb.ToString();
        }
    }
}
