using System;
using System.Collections.Generic;
using System.Text;

namespace Xml2Fb.Shared.FbDef
{
    public static class FbHelper
    {

        public enum EFbScalarType
        {
            None,
            FBT_Byte, FBT_UByte,
            FBT_Bool,
            FBT_Short, FBT_UShort,
            FBT_Int, FBT_UInt,
            FBT_Long, FBT_ULong,
            FBT_Float, FBT_Double,
            FBT_String,
        }

        public readonly struct FbScalarTypeAndName
        {
            public readonly EFbScalarType type;
            public readonly string typeName;
            public readonly string? aliaTypeName;
            public FbScalarTypeAndName( EFbScalarType type, string typeName, string? alisTypeName )
            {
                this.type = type;
                this.typeName = typeName;
                this.aliaTypeName = alisTypeName;
            }
        }

        public static readonly FbScalarTypeAndName[] s_fbScalarTypeAndNames = new FbScalarTypeAndName[]
        {
            new FbScalarTypeAndName(EFbScalarType.FBT_Byte,"byte","int8"),
            new FbScalarTypeAndName(EFbScalarType.FBT_UByte,"ubyte","uint8"),
            new FbScalarTypeAndName(EFbScalarType.FBT_Bool,"bool",null),
            new FbScalarTypeAndName(EFbScalarType.FBT_Short,"short","int16"),
            new FbScalarTypeAndName(EFbScalarType.FBT_UShort,"ushort","uint16"),
            new FbScalarTypeAndName(EFbScalarType.FBT_Int,"int","int32"),
            new FbScalarTypeAndName(EFbScalarType.FBT_UInt,"uint","uint32"),
            new FbScalarTypeAndName(EFbScalarType.FBT_Long,"long","int64"),
            new FbScalarTypeAndName(EFbScalarType.FBT_ULong,"ulong","uint64"),
            new FbScalarTypeAndName(EFbScalarType.FBT_Float,"float","float32"),
            new FbScalarTypeAndName(EFbScalarType.FBT_Double,"double","float32"),
            new FbScalarTypeAndName(EFbScalarType.FBT_String,"string",null)
        };

        public static EFbScalarType Get_EFbScalarType( string type )
        {
            foreach (var e in s_fbScalarTypeAndNames)
            {
                if (type == e.typeName || type == e.aliaTypeName)
                    return e.type;
            }
            return EFbScalarType.None;
        }
        public static string? Get_FbScalarTypeName( EFbScalarType scalarType )
        {
            foreach (var e in s_fbScalarTypeAndNames)
            {
                if (scalarType == e.type)
                    return e.typeName;
            }
            return null;
        }



        public static string GetFbTypeFromXmlValue( string xmlValue, string xmlNodeName )
        {
            bool hasSep = ( xmlValue.IndexOfAny( new char[] { ',', ';', '|',':' } ) ) != -1;
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
   

        public static bool IsFbScalarType( string typeName )
        {
            return typeName == "string" || typeName == "int" ||
            typeName == "uint" || typeName == "float" ||
            typeName == "bool";
        }

        public static string TableName( string typeName ) => typeName.Capitalize();
        public static string ObjectiveTableName( string typeName ) => $"{typeName.Capitalize()}T";
    }
}
