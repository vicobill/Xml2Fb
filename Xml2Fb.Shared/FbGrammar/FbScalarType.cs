using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xml2Fb.Shared.FbGrammar;

/// <summary>
/// FlatBuffers 中基本类型
/// </summary>
public enum EFbScalarType
{
    None,

    FBT_Bool,

    FBT_Byte, FBT_UByte,
    FBT_Short, FBT_UShort,
    FBT_Int, FBT_UInt,
    FBT_Long, FBT_ULong,

    FBT_Float, FBT_Double,

    FBT_String,

}

public static class FbScalarType
{
    public record TypeNameAlias( EFbScalarType type, string typeName, string? aliaTypeName );

    public static readonly TypeNameAlias[] s_fbScalarTypeAndNames =
    [
    new TypeNameAlias(EFbScalarType.FBT_Bool,"bool",null),
    new TypeNameAlias(EFbScalarType.FBT_Byte,"byte","int8"),
    new TypeNameAlias(EFbScalarType.FBT_UByte,"ubyte","uint8"),
    new TypeNameAlias(EFbScalarType.FBT_Short,"short","int16"),
    new TypeNameAlias(EFbScalarType.FBT_UShort,"ushort","uint16"),
    new TypeNameAlias(EFbScalarType.FBT_Int,"int","int32"),
    new TypeNameAlias(EFbScalarType.FBT_UInt,"uint","uint32"),
    new TypeNameAlias(EFbScalarType.FBT_Long,"long","int64"),
    new TypeNameAlias(EFbScalarType.FBT_ULong,"ulong","uint64"),
    new TypeNameAlias(EFbScalarType.FBT_Float,"float","float32"),
    new TypeNameAlias(EFbScalarType.FBT_Double,"double","float64"),
    new TypeNameAlias(EFbScalarType.FBT_String,"string",null)
    ];

    public static string GetScalarTypeName( EFbScalarType scalarType )
    {
        if (scalarType == EFbScalarType.None)
            return string.Empty;

        foreach(var e in s_fbScalarTypeAndNames)
        {
            if (e.type == scalarType)
                return e.typeName;
        }

        return string.Empty;
    }

    public static bool IsFbScalarType(this string typeName)
    {
        if (string.IsNullOrEmpty( typeName ))
            return false;
        foreach(var e in s_fbScalarTypeAndNames)
        {
            if (e.typeName == typeName || e.aliaTypeName == typeName)
                return true;
        }
        return false;
    }

    public static bool IsVectorTypeName(this string typeName)
    {
        var s = typeName.Trim();
        return  s[0] == '[' && s[^1] == ']' ;
    }

    public static bool IsIntegerTypeName(this string typeName )
    {
        return typeName switch
        {
            "byte" or "ubyte" or
            "bool" or
            "short" or "ushort" or
            "int" or "uint" or
            "long" or "ulong" => true,

            "int8" or "uint8" or
            "int16" or "uint16" or
            "int32" or "uint32" or
            "int64" or "uint64" => true,
            _ => false
        };
    }

   


    public static bool IsFloatTypeName(this string typeName )
    {
        return typeName switch
        {
            "float" or "double" or "float32" or "float64" => true,
            _ => false
        };
    }

    public static bool IsNumberTypeName( this string typeName )
    {
        return typeName.IsFloatTypeName() || typeName.IsIntegerTypeName();
    }

    public static int ByteSizeOfTypeName(string typeName )
    {
        return typeName switch
        {
            "byte" or "ubyte" or "bool" or "int8" or "uint8" => 1,
            "short" or "ushort" or "int16" or "uint16" => 2,
            "int" or "uint" or "int32" or "uint32" or "float" or "float32" => 4,
            "long" or "ulong" or "int64" or "uint64" or "double" or "float64" => 8,
            "string" => 12,
            _ => 0
        };
    }
    public static bool IsUnsignedTypeName( this string typeName )
        => typeName[0] == 'u';


    public static bool IsStringTypeName( this string typeName )
        => typeName == "string";

    /// <summary>
    /// 类型优先级比较
    /// </summary>
    /// <param name="typeName">原类型</param>
    /// <param name="otherTypeName">比较类型</param>
    /// <returns></returns>
    public static bool PriorThen(this string typeName, string otherTypeName)
    {
        // Vector 类型 > 非 Vector类型
        if (typeName.IsVectorTypeName() && !otherTypeName.IsVectorTypeName())
            return true;

        // String 类型 > Number 类型
        if (typeName.IsStringTypeName() && otherTypeName.IsNumberTypeName())
            return true;

        // Float 类型 > Integer 类型
        if (typeName.IsFloatTypeName() && otherTypeName.IsIntegerTypeName())
            return true;


        int sizeA = ByteSizeOfTypeName( typeName );
        int sizeB = ByteSizeOfTypeName( otherTypeName );
        if (sizeA > sizeB)
            return true;

        // 同类型，unsigned > signed
        if ( sizeA == sizeB  &&
            typeName.IsUnsignedTypeName() && !otherTypeName.IsUnsignedTypeName())
            return true;


        return false;
    }

    /// <summary>
    /// 构造数组类型名称
    /// </summary>
    /// <param name="basicTypeName">基础类型</param>
    /// <param name="elementCount">元素数量</param>
    /// <returns></returns>
    /// <example>
    /// struct Vec3 { v: [float:3]; }
    /// </example>
    public static string ConsArrayTypeName( string basicTypeName, int elementCount )
        => $"[{basicTypeName} : {elementCount}]"; // [float:3]

    /// <summary>
    /// 构造矢量类型
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static string ConsVectorTypeName( string typeName )
        => $"[{typeName}]";

    /// <summary>
    /// 构造矢量类型字段名
    /// </summary>
    /// <param name="typeName"></param>
    /// <returns></returns>
    public static string ConsVectorFieldName( string typeName )
        => $"{typeName}s".ToLower();
}
