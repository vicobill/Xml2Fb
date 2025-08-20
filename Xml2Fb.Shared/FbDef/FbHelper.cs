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
            bool hasSep = ( xmlValue.IndexOfAny( anyOfSeps ) ) != -1;
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


            if (int.TryParse( xmlValue, out _ )) {return "int";}
            if (uint.TryParse( xmlValue, out _ )) {return "uint";}

            if (long.TryParse( xmlValue, out _ )) { return "long"; }
            if (ulong.TryParse( xmlValue, out _ )) { return "ulong"; }

            if (float.TryParse( xmlValue, out _ )){  return "float"; }
            if (double.TryParse( xmlValue, out _ )) {  return "double"; }

            if (short.TryParse( xmlValue, out _ )) { return "short"; }
            if (ushort.TryParse( xmlValue, out _ )) { return "ushort"; }

            if (sbyte.TryParse( xmlValue, out _)) { return "byte"; }
            if (byte.TryParse( xmlValue, out _ )){ return "ubyte"; }

            if (bool.TryParse( xmlValue, out _ )) { return "bool";}

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
