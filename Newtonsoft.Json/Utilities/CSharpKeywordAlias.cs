using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Linq;

namespace Newtonsoft.Json.Utilities
{
    public static class CSharpKeywordAlias
    {
        public static readonly string[] s_CsharpKeywords = new string[]
        {
            // 关键字
"abstract","as","base","bool","break","byte","case","catch","char","checked","class","const","continue",
"decimal","default","delegate","do","double","else","enum",
"event","explicit","extern","false","finally","fixed","float","for","foreach","goto","if","implicit","in",
"int","interface","internal","is","lock","long",
"namespace","new","null","object","operator","out","override","params","private","protected","public","readonly",
"ref","return","sbyte","sealed","short","sizeof","stackalloc",
"static","string","struct","switch","this","throw","true","try","typeof","uint","ulong",
"unchecked","unsafe","ushort","using","virtual","void","volatile","while",
            // 上下文关键字
"add","and","alias","ascending","args","async","await","by","descending","dynamic","equals","from",
"get","global","group","init","into","join","let","managed","nameof","nint","not",
"notnull","nuint","on","or","orderby","partial","record","remove","select","set","unmanaged",
"value","var","when","where","with","yield"
        };

        public static string Alias(string name)
        {
            for (int i = 0; i < s_CsharpKeywords.Length; i++)
            {
                if (s_CsharpKeywords[i] == name)
                {
                    return $"{s_CsharpKeywords[i]}_alias";
                }
            }
            return name;
        }

        public static string CsAlias(this string s)
        {
            return Alias(s);
        }

        public static string DeAlias(this string s)
        {
            if (s.EndsWith( "_alias" ))
                return s.Replace( "_alias", "" );
            return s;
        }

        public static string DeAliasArr(this string s )
        {
            return $"{DeAlias( s )}_arr";
        }
    }
}
