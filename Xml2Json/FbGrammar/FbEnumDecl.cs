using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xml2Fb.FbGrammar
{
    /// <summary>
    /// EBNF格式：
    /// enum_decl = ( `enum` ident `:` type | `union` ident )  metadata `{` commasep( enumval_decl ) `}
    /// </summary>
    internal class FbEnumDecl
    {
        public bool isUnion;
        public required string enumName;
        public EFbScalarType underlyingType = EFbScalarType.FBT_Int;
        public FbMetadata? metadata;
        public List<EnumValDecl> enumValDecls = new();

        public FbEnumDecl WithField( string fieldName, int? integer = null, FbMetadata? metadata = null )
        {
            var ed = enumValDecls.Find( ( x ) => x.fieldName == fieldName );
            if (ed != null)
            {
                if (integer != null)
                    ed.integer = integer;
                if (metadata != null)
                    ed.metadata = metadata;
            }
            else
            {
                enumValDecls.Add( new EnumValDecl
                {
                    fieldName = fieldName,
                    integer = integer,
                    metadata = metadata
                } );
            }
            return this;

        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();


            if (isUnion) // union <name> (metadata) { fields... }
            {

                sb.Append( $"union {enumName} {metadata} " ).Append( '{' )
                    .AppendLine();
                int i = 0;
                foreach (var e in enumValDecls)
                {
                    sb.Append( '\t' ).Append( e );
                    if (++i < enumValDecls.Count)
                    {
                        sb.Append( ", " ).AppendLine();
                    }
                    
                }
                sb.AppendLine();
                sb.Append( "}" );
            }
            else // enum <name> : type (metadata) { fields... }
            {
                sb.Append( $"enum {enumName} " );
                string underlyingTypeName = FbScalarType.GetScalarTypeName( underlyingType );
                if (!string.IsNullOrEmpty( underlyingTypeName ))
                {
                    sb.Append( $": {underlyingTypeName} {metadata} " )
                        .Append( '{' )
                        .AppendLine();
                }
                int i = 0;
                foreach (var e in enumValDecls)
                {
                    sb.Append( '\t' ).Append( e );
                    if (i++ < enumValDecls.Count)
                    { sb.Append( ", " ); }
                    sb.AppendLine();
                }
                sb.AppendLine();
                sb.Append( '}' );
            }

            return sb.ToString();
        }
    }

    /// <summary>
    /// EBNF格式：
    /// enumval_decl = ident [ `=` integer_constant ] metadata
    /// </summary>
    internal class EnumValDecl
    {
        public required string fieldName;
        public int? integer;
        public FbMetadata? metadata;


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append( fieldName );

            if (integer.HasValue)
            {
                sb.Append( $" = {integer}" );
            }

            if (metadata != null)
            {
                sb.Append( metadata );
            }

            return sb.ToString();
        }
    }
}
