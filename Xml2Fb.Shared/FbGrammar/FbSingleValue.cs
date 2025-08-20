using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Xml2Fb.Shared.FbGrammar;

/// <summary>
/// EBNF格式：
/// scalar = boolean_constant | integer_constant | float_constant
/// </summary>
// 
[StructLayout( LayoutKind.Explicit )]
public struct Scalar
{
    [FieldOffset( 0 )]
    public bool @bool;

    [FieldOffset( 0 )]
    public byte @byte;
    [FieldOffset( 0 )]
    public sbyte @sbyte;

    [FieldOffset( 0 )]
    public char @char;


    [FieldOffset( 0 )]
    public short @short;
    [FieldOffset( 0 )]
    public ushort @ushort;


    [FieldOffset( 0 )]
    public int @int;
    [FieldOffset( 0 )]
    public uint @uint;
    [FieldOffset( 0 )]

    public long @long;
    [FieldOffset( 0 )]
    public ulong @ulong;


    [FieldOffset( 0 )]
    public float @float;

    [FieldOffset( 0 )]
    public double @double;


    public static bool operator ==( Scalar lhs, Scalar rhs )
    {
        return lhs.@ulong == rhs.@ulong;
    }
    public static bool operator !=( Scalar lhs, Scalar rhs )
    {
        return !( lhs == rhs );
    }

}
/// <summary>
/// EBNF格式：
/// single_value = scalar | string_constant
/// </summary>

public class FbSingleValue
{
    private static FbSingleValue _empty = new FbSingleValue();
    public static FbSingleValue Empty => _empty;

    public bool hasValue;
    public EFbScalarType valueType;
    public Scalar scalar;

    public string? @string;

    public static bool operator == ( FbSingleValue left, FbSingleValue right )
    {
        if (left.hasValue!=right.hasValue) return false;
        if (left.valueType!=right.valueType) return false;
        if (left.scalar != right.scalar) return false;
        if (left.@string != right.@string) return false;
        return true;
    }
    public static bool operator !=( FbSingleValue left, FbSingleValue right )
    {
        return !( left == right );
            
    }

    public FbSingleValue()
    {
        hasValue = false;
        valueType = EFbScalarType.None;
        @string = null;
    }

    public FbSingleValue(bool b)
    {
        hasValue = true;
        valueType = EFbScalarType.FBT_Bool;
        scalar.@bool = b;
    }
    public FbSingleValue(byte b )
    {
        hasValue = true;
        valueType = EFbScalarType.FBT_UByte ;
        scalar.@byte = b;
    }
    public FbSingleValue(sbyte b )
    {
        hasValue = true;
        valueType = EFbScalarType.FBT_Byte;
        scalar.@sbyte = b;
    }
    public FbSingleValue(char c )
    {
        hasValue = true;
        valueType = EFbScalarType.FBT_Short;
        scalar.@char = c;
    }
    public FbSingleValue( short s )
    {
        hasValue = true;
        valueType = EFbScalarType.FBT_Short;
        scalar.@short = s;
    }
    public FbSingleValue(ushort s )
    {
        hasValue = true;
        valueType = EFbScalarType.FBT_UShort;
        scalar.@ushort = s;
    }
    public FbSingleValue( int i )
    {
        hasValue = true;
        valueType = EFbScalarType.FBT_Int;
        scalar.@int = i;
    }
    public FbSingleValue( uint i )
    {
        hasValue = true;
        valueType = EFbScalarType.FBT_UInt;
        scalar.@uint = i;
    }

    public FbSingleValue( long l )
    {
        hasValue = true;
        valueType = EFbScalarType.FBT_Long;
        scalar.@long = l;
    }
    public FbSingleValue( ulong l )
    {
        hasValue = true;
        valueType = EFbScalarType.FBT_ULong;
        scalar.@ulong = l;
    }
    public FbSingleValue( float f )
    {
        hasValue = true;
        valueType = EFbScalarType.FBT_Float;
        scalar.@float = f;
    }
    public FbSingleValue( double d )
    {
        hasValue = true;
        valueType = EFbScalarType.FBT_Double;
        scalar.@double = d;
    }
    public FbSingleValue(string s)
    {
        hasValue = true;
        valueType = EFbScalarType.FBT_String;
        @string = s;
    }

    // 42.1  or  \"string constant\"
    public override string ToString()
    {
        if (!hasValue)
            return string.Empty;
        StringBuilder sb = new StringBuilder();

        if(valueType == EFbScalarType.FBT_String)
        {
            sb.Append('"').Append( @string ).Append('"');
        }else
        {
            switch (valueType)
            {
                case EFbScalarType.FBT_Bool:sb.Append( scalar.@bool );break;
                case EFbScalarType.FBT_Byte:sb.Append( scalar.@sbyte );break;
                case EFbScalarType.FBT_UByte:sb.Append( scalar.@byte );break;
                case EFbScalarType.FBT_Short:sb.Append( scalar.@short );break;
                case EFbScalarType.FBT_UShort: sb.Append( scalar.@ushort );break;
                case EFbScalarType.FBT_UInt: sb.Append( scalar.@uint );break;
                case EFbScalarType.FBT_Int: sb.Append( scalar.@int );break;
                case EFbScalarType.FBT_Long: sb.Append( scalar.@long );break;
                case EFbScalarType.FBT_ULong: sb.Append( scalar.@ulong );break;
                case EFbScalarType.FBT_Float: sb.Append( scalar.@float );break;
                case EFbScalarType.FBT_Double:sb.Append( scalar.@double );break;
            }
        }

        return sb.ToString();
    }

}
