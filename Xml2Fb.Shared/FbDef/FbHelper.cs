using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xml2Fb.Shared.FbDef
{
    public static class FbHelper
    {
        private static readonly char[] anyOfSeps = [',', ';', '|', ':'];

        /// <summary>
        /// 从Xml节点的值中，推断使用的类型
        /// </summary>
        /// <param name="xmlValue">节点值</param>
        /// <param name="xmlNodeName">节点名称</param>
        /// <returns></returns>
        public static string GetFbTypeFromXmlValue( string xmlValue, string xmlNodeName )
        {
            string s = xmlValue.Trim();
            if (s.Length == 0)
                return "string";

            if (s.HasChineseChars())
                return "string";
            
            bool hasSep = ( s.IndexOfAny( anyOfSeps ) ) != -1;
            if (hasSep)
                return "string";
 
            switch (xmlNodeName.ToLower())
            {
                case "name":
                case "describe":
                case "desc":
                case "directto":
                    return "string";
            }


            if (int.TryParse( s, out _ )) {return "int";}
            if (uint.TryParse( s, out _ )) {return "uint";}

            if (long.TryParse( s, out _ )) { return "long"; }
            if (ulong.TryParse( s, out _ )) { return "ulong"; }

            if (float.TryParse( s, out _ )){  return "float"; }
            if (double.TryParse( s, out _ )) {  return "double"; }

            if (short.TryParse( s, out _ )) { return "short"; }
            if (ushort.TryParse( s, out _ )) { return "ushort"; }

            if (sbyte.TryParse( s, out _)) { return "byte"; }
            if (byte.TryParse( s, out _ )){ return "ubyte"; }

            if (bool.TryParse( s, out _ )) { return "bool";}

            return "string";
        }

        public static bool IsUnsignedType(string type )
        {
            if (type.StartsWith( 'u' ))
                return true;
            return false;
        }
   

 

        public static string TableName( string typeName ) => typeName.Capitalize();
        public static string ObjectiveTableName( string typeName ) => $"{typeName.Capitalize()}T";
    }
}
