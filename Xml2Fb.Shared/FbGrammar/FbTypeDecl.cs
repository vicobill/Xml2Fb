using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xml2Fb.Shared;

namespace Xml2Fb.Shared.FbGrammar;

/// <summary>
/// EBNF格式：
/// type_decl = ( `table` | `struct` ) ident metadata `{` field_decl+ `}
/// </summary>
public class FbTypeDecl
{
    public bool isStruct;// table | struct
    public required string typeName; // ident
    public FbMetadata? metadata;
    internal List<FieldDecl> fieldDecls = new();

    public FbTypeDecl WithField( string fieldName, string typeName, FbSingleValue? singleValue = null, FbMetadata? metadata = null )
    {
        var has = fieldDecls.Find( ( x ) => x.fieldName == fieldName );
        if (has != null)
        {
            if (has.typeName != typeName)
            {
                if (typeName.PriorThen( has.typeName ))
                {
                    DebugHelper.PrintWarning( $"field `{fieldName}`, has type `{has.typeName}`, change to:`{typeName}`" );
                    has.typeName = typeName;
                }
            }
            if (singleValue is not null)
            {
                has.singleValue = singleValue;
            }
            if (metadata != null)
            {
                if (has.metadata != null)
                {
                    has.metadata.MergeFrom( metadata );
                }
                else
                {
                    has.metadata = metadata;
                }
            }

        }
        else
        {
            fieldDecls.Add( new FieldDecl
            {
                fieldName = fieldName,
                typeName = typeName,
                singleValue = singleValue,
                metadata = metadata
            } );
        }

        return this;
    }


    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        if (isStruct)
        {
            sb.Append( $"struct {typeName} " );
        }
        else
        {
            sb.Append( $"table {typeName} " );
        }

        sb.Append( metadata );

        sb.Append( '{' ).AppendLine();

        foreach (var e in fieldDecls)
        {
            sb.Append( '\t' ).Append( e ).AppendLine();
        }
        sb.Append( '}' ).AppendLine();

        return sb.ToString();
    }


    public void MergeFrom( FbTypeDecl other )
    {
        if (metadata == null)
        {
            metadata = other.metadata;
        }
        else
        {
            metadata.MergeFrom( other.metadata );
        }

        foreach (var e in other.fieldDecls)
        {
            var has = fieldDecls.Find( ( x ) => x.fieldName == e.fieldName );
            if (has != null)
            {
                has.MergeFrom( e, FieldDecl.EMergeCategory.DefaultPriority );
            }
            else
            {
                fieldDecls.Add( e );
            }
        }
    }


}

/// <summary>
/// EBNF格式：
/// field_decl = ident `:` type [ `=` scalar ] metadata `;`
///
/// </summary>
internal class FieldDecl
{
    public required string fieldName;
    /// <summary>
    /// type = `bool` | `byte` | `ubyte` | `short` | `ushort` | `int` | `uint` |
    ///   `float` | `long` | `ulong` | `double` | `int8` | `uint8` | `int16` |
    ///   `uint16` | `int32` | `uint32`| `int64` | `uint64` | `float32` |
    ///   `float64` | `string` | `[` type `]` | ident
    /// </summary>
    public required string typeName;

    public FbSingleValue? singleValue;
    public FbMetadata? metadata;

    public enum EMergeCategory
    {
        UseMine, // 使用我的
        Overwrite, // 无脑覆盖
        DefaultPriority, // 按默认优先级
    }

    public void MergeFrom( FieldDecl other, EMergeCategory mergeCategory = EMergeCategory.UseMine )
    {
        if (fieldName != other.fieldName)
        {
            throw new ArgumentException( "field name is not same,cannot merge" );
        }

        if (mergeCategory == EMergeCategory.Overwrite)
        {
            typeName = other.typeName;
            singleValue = other.singleValue;
        }
        else if (mergeCategory == EMergeCategory.UseMine)
        {
            if (typeName != other.typeName)
            {
                DebugHelper.PrintWarning( $"field `{fieldName}` type is:{typeName}, merged type is:{other.typeName},Check it!" );
            }
            if (singleValue != other.singleValue)
            {
                DebugHelper.PrintWarning( $"field `{fieldName}` value is:{singleValue}, merged value:{other.singleValue},Check it!" );
            }
        }
        else if (mergeCategory == EMergeCategory.DefaultPriority)
        {
            if (other.typeName.PriorThen( typeName ))
            {
                typeName = other.typeName;
                singleValue = other.singleValue;
            }
        }


        if (metadata == null)
        {
            metadata = other.metadata;
        }
        else
        {
            metadata.MergeFrom( other.metadata );
        }

    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append( $"{fieldName}: {typeName}" );
        if (singleValue is not null && singleValue.hasValue)
        {
            sb.Append( $" = {singleValue}" );
        }
        sb.Append( metadata );
        sb.Append( ";" );

        return sb.ToString();
    }
}
