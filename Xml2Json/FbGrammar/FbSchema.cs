using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xml2Fb.Shared;

namespace Xml2Fb.FbGrammar
{
    /// <summary>
    /// EBNF格式：
    /// schema = include* ( namespace_decl | type_decl | enum_decl | root_decl |
    ///       file_extension_decl | file_identifier_decl |
    ///       attribute_decl | rpc_decl | object )*
    /// </summary>
    internal class FbSchema
    {
        /// <summary>
        /// fbs文件名称，不参与ToString()
        /// </summary>
        public string? schemaName;

        /// <summary>
        /// 版本说明，参与ToString()
        /// </summary>
        public string? schemaVersion;
        /// <summary>
        /// 简要说明，参与ToString()
        /// </summary>
        public string? brief;


        public List<IncludeDecl> includeDecls = new(); // include "x.fbs";
        public NamespaceDecl? namespaceDecl; // namespace "a.b.c";
        public List<FbTypeDecl> typeDecls = new();
        public List<FbEnumDecl> enumDecls = new();
        public RootDecl? rootDecl;
        public FileExtDecl? fileExtDecl;
        public FileIdentDecl? fileIdentDecl;
        public List<FbAttributeDecl> attributeDecls = new();
        public List<FbRpcDecl> rpcDecls = new();
        public List<FbObject> objectDecls = new();

        public FbSchema Include( string file )
        {
            bool included = includeDecls.Any( ( x ) => x.fileName == file );
            if (included)
                return this;
            includeDecls.Add( new IncludeDecl { fileName = file } );
            return this;
        }
        public FbSchema WithNamespace( string @namespace )
        {
            namespaceDecl = new NamespaceDecl { @namespace = @namespace };
            return this;
        }
        public FbSchema WithFileExt( string ext )
        {
            fileExtDecl = new FileExtDecl { ext = ext };
            return this;
        }
        public FbSchema WithFileIdentifier( string identifier )
        {
            fileIdentDecl = new FileIdentDecl { ident = identifier };
            return this;
        }
        public FbSchema WithRootType( string type )
        {
            rootDecl = new RootDecl { typeName = type };
            return this;
        }

        public FbSchema WithAttribute( string attrName, bool needQuote )
        {
            bool hasAttr = attributeDecls.Any( ( x ) => x.attrName == attrName );
            if (hasAttr)
                return this;

            attributeDecls.Add( new FbAttributeDecl
            {
                attrName = attrName,
                needQuote = needQuote
            } );

            return this;
        }


        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (schemaVersion != null)
            {
                sb.AppendLine( $"version: {schemaVersion}" );
            }
            if (brief != null)
            {
                sb.AppendLine( $"// {brief}" );
            }
            
            if (includeDecls.IsNotEmpty())
            {
                foreach (var e in includeDecls)
                {
                    sb.Append( e ).AppendLine();
                }
                sb.AppendLine();
            }

            if (namespaceDecl != null)
            {
                sb.Append( namespaceDecl ).AppendLine();
                sb.AppendLine();
            }

            if (fileExtDecl != null)
            {
                sb.Append( fileExtDecl ).AppendLine();
                sb.AppendLine();
            }
                

            if (fileIdentDecl != null)
            {
                sb.Append( fileIdentDecl ).AppendLine();
                sb.AppendLine();
            }
                

            if (attributeDecls.IsNotEmpty())
            {
                foreach (var e in attributeDecls)
                {
                    sb.Append( e ).AppendLine();
                }
                sb.AppendLine();
            }

            if (enumDecls.IsNotEmpty())
            {
                foreach (var e in enumDecls)
                {
                    sb.Append( e ).AppendLine();
                }
                sb.AppendLine();
            }

            if (enumDecls.IsNotEmpty())
            {
                foreach (var e in typeDecls)
                {
                    sb.Append( e ).AppendLine();
                }
                sb.AppendLine();
            }

            if (rpcDecls.IsNotEmpty())
            {
                foreach (var e in rpcDecls)
                {
                    sb.Append( e ).AppendLine();
                }
                sb.AppendLine();
            }

            if (rootDecl != null)
            {
                sb.Append( rootDecl ).AppendLine();
                sb.AppendLine();
            }

            return sb.ToString();
        }

        internal FbSchema WithTypeDecl( FbTypeDecl typeDecl )
        {
            typeDecls.Add( typeDecl );
            return this;
        }

        internal FbSchema WithEnumDecl( FbEnumDecl enumDecl )
        {
            enumDecls.Add( enumDecl );
            return this;
        }
    }

    /// <summary>
    /// EBNF格式：
    /// include = `include` string_constant `;`
    /// </summary>
    internal class IncludeDecl
    {
        public required string fileName;
        public override string ToString()
        {
            return $"include \"{fileName}.fbs\";";
        }
    }

    /// <summary>
    /// EBNF格式：
    /// namespace_decl = `namespace` ident ( `.` ident )* `;`
    /// </summary>
    internal class NamespaceDecl
    {
        public required string @namespace;
        public override string ToString()
        {
            return $"namespace {@namespace};";
        }
    }
    /// <summary>
    /// EBNF格式：
    /// file_extension_decl = `file_extension` string_constant `;`
    /// </summary>
    internal class FileExtDecl
    {
        // file_extension "ext";
        public required string ext;
        public override string ToString()
        {
            return $"file_extension \"{ext}\";";
        }
    }

    /// <summary>
    /// EBNF格式：
    /// file_identifier_decl = `file_identifier` string_constant `;`
    /// </summary>
    internal class FileIdentDecl
    {
        // file_identifier "ident";
        public required string ident;
        public override string ToString()
        {
            return $"file_identifier \"{ident}\";";
        }
    }

    /// <summary>
    /// EBNF格式：
    /// root_decl = `root_type` ident `;`
    /// </summary>
    internal class RootDecl
    {
        // root_type <typeName>;
        public required string typeName;
        public override string ToString()
        {
            return $"root_type {typeName};";
        }
    }


}
