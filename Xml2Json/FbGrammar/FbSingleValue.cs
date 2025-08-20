using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Xml2Fb.FbGrammar
{
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

    internal class FbSingleValue
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
            this.hasValue = false;
            this.valueType = EFbScalarType.None;
            this.@string = null;
        }

        public FbSingleValue(bool b)
        {
            this.hasValue = true;
            this.valueType = EFbScalarType.FBT_Bool;
            this.scalar.@bool = b;
        }
        public FbSingleValue(byte b )
        {
            this.hasValue = true;
            this.valueType = EFbScalarType.FBT_UByte ;
            this.scalar.@byte = b;
        }
        public FbSingleValue(sbyte b )
        {
            this.hasValue = true;
            this.valueType = EFbScalarType.FBT_Byte;
            this.scalar.@sbyte = b;
        }
        public FbSingleValue(char c )
        {
            this.hasValue = true;
            this.valueType = EFbScalarType.FBT_Short;
            this.scalar.@char = c;
        }
        public FbSingleValue( short s )
        {
            this.hasValue = true;
            this.valueType = EFbScalarType.FBT_Short;
            this.scalar.@short = s;
        }
        public FbSingleValue(ushort s )
        {
            this.hasValue = true;
            this.valueType = EFbScalarType.FBT_UShort;
            this.scalar.@ushort = s;
        }
        public FbSingleValue( int i )
        {
            this.hasValue = true;
            this.valueType = EFbScalarType.FBT_Int;
            this.scalar.@int = i;
        }
        public FbSingleValue( uint i )
        {
            this.hasValue = true;
            this.valueType = EFbScalarType.FBT_UInt;
            this.scalar.@uint = i;
        }

        public FbSingleValue( long l )
        {
            this.hasValue = true;
            this.valueType = EFbScalarType.FBT_Long;
            this.scalar.@long = l;
        }
        public FbSingleValue( ulong l )
        {
            this.hasValue = true;
            this.valueType = EFbScalarType.FBT_ULong;
            this.scalar.@ulong = l;
        }
        public FbSingleValue( float f )
        {
            this.hasValue = true;
            this.valueType = EFbScalarType.FBT_Float;
            this.scalar.@float = f;
        }
        public FbSingleValue( double d )
        {
            this.hasValue = true;
            this.valueType = EFbScalarType.FBT_Double;
            this.scalar.@double = d;
        }
        public FbSingleValue(string s)
        {
            this.hasValue = true;
            this.valueType = EFbScalarType.FBT_String;
            this.@string = s;
        }

        // 42.1  or  \"string constant\"
        public override string ToString()
        {
            if (!this.hasValue)
                return string.Empty;
            StringBuilder sb = new StringBuilder();

            if(this.valueType == EFbScalarType.FBT_String)
            {
                sb.Append('"').Append(this.@string).Append('"');
            }else
            {
                switch (this.valueType)
                {
                    case EFbScalarType.FBT_Bool:sb.Append( this.scalar.@bool );break;
                    case EFbScalarType.FBT_Byte:sb.Append( this.scalar.@sbyte );break;
                    case EFbScalarType.FBT_UByte:sb.Append( this.scalar.@byte );break;
                    case EFbScalarType.FBT_Short:sb.Append( this.scalar.@short );break;
                    case EFbScalarType.FBT_UShort: sb.Append( this.scalar.@ushort );break;
                    case EFbScalarType.FBT_UInt: sb.Append( this.scalar.@uint );break;
                    case EFbScalarType.FBT_Int: sb.Append( this.scalar.@int );break;
                    case EFbScalarType.FBT_Long: sb.Append( this.scalar.@long );break;
                    case EFbScalarType.FBT_ULong: sb.Append( this.scalar.@ulong );break;
                    case EFbScalarType.FBT_Float: sb.Append( this.scalar.@float );break;
                    case EFbScalarType.FBT_Double:sb.Append(this.scalar.@double );break;
                }
            }

            return sb.ToString();
        }

    }
}
