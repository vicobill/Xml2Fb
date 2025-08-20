using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Xml2Fb.Shared;

namespace Xml2Fb.Shared.FbGrammar;

/// <summary>
/// EBNF格式：
/// schema = include* ( namespace_decl | type_decl | enum_decl | root_decl |
///       file_extension_decl | file_identifier_decl |
///       attribute_decl | rpc_decl | object )*
/// </summary>
public class FbSchema
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


    internal List<IncludeDecl> includeDecls = new(); // include "x.fbs";
    internal NamespaceDecl? namespaceDecl; // namespace "a.b.c";
    internal List<FbTypeDecl> typeDecls = new();
    internal List<FbEnumDecl> enumDecls = new();
    internal RootDecl? rootDecl;
    internal FileExtDecl? fileExtDecl;
    internal FileIdentDecl? fileIdentDecl;
    internal List<FbAttributeDecl> attributeDecls = new();
    internal List<FbRpcDecl> rpcDecls = new();
    internal List<FbObject> objectDecls = new();

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

        if (typeDecls.IsNotEmpty())
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

    public FbSchema WithTypeDecl( FbTypeDecl typeDecl )
    {
        var has = typeDecls.Find( ( x ) => x.typeName == typeDecl.typeName );
        if (has != null)
        {
            has.MergeFrom( typeDecl );
        }
        else
        {
            typeDecls.Add( typeDecl );
        }
        return this;
    }

    public FbSchema WithEnumDecl( FbEnumDecl enumDecl )
    {
        var has = enumDecls.Find( x => x.enumName == enumDecl.enumName );
        if (has != null)
        {
            has.MergeFrom( enumDecl );
        }
        else
        {
            enumDecls.Add( enumDecl );
        }
        return this;
    }

    /// <summary>
    /// 查找是否已经存在表定义
    /// </summary>
    /// <param name="tableName"></param>
    /// <param name="tableDecl"></param>
    /// <returns></returns>
    public bool TryPeek( string tableName, out FbTypeDecl? tableDecl )
    {
        tableDecl = null;
        foreach (var e in typeDecls)
        {
            if (e.typeName == tableName)
            {
                tableDecl = e;
                return true;
            }
        }
        return false;
    }

    public void MergeFrom(FbSchema other, bool overwrite = true)
    {
        MergeIncludes( other.includeDecls, overwrite );
        MergeTypeDecls(other.typeDecls, overwrite );
        MergeEnumDecls(other.enumDecls, overwrite );
        MergeAttrDecls( other.attributeDecls, overwrite );
        MergeRpcDecls(other.rpcDecls, overwrite );
        //MergeObjectDecls( other.objectDecls, overwrite );
    }

    void MergeIncludes(List<IncludeDecl> otherIncludeDecls, bool overwrite = true)
    {
        foreach (var e in otherIncludeDecls)
        {
            var has = this.includeDecls.Find( x => x.fileName == e.fileName );
            if (has == null)
            {
                this.includeDecls.Add( e );
            }
        }
    }
    void MergeTypeDecls(List<FbTypeDecl> otherTypeDecls, bool overwrite = true)
    {
        foreach(var e in otherTypeDecls)
        {
            var has = this.typeDecls.Find(x => x.typeName == e.typeName );
            if (has != null)
            {
                has.MergeFrom( e );
            }else
            {
                this.typeDecls.Add( e );
            }
        }
    }

    void MergeEnumDecls(List<FbEnumDecl> otherEnumDecls, bool overwrite = true)
    {
        foreach(var e in otherEnumDecls)
        {
            var has = this.enumDecls.Find(x => x.enumName ==  e.enumName );
            if (has != null)
            {
                has.MergeFrom( e,overwrite );
            }else
            {
                this.enumDecls.Add( e );
            }
        }
    }

    void MergeAttrDecls(List<FbAttributeDecl> otherAttrDecls, bool overwrite = true)
    {
        foreach(var e in otherAttrDecls)
        {
            var has = this.attributeDecls.Find(x=>x.attrName == e.attrName );
            if (has != null)
            {
                has.MergeFrom(e,overwrite );
            }else
            {
                this.attributeDecls.Add( e );
            }
        }
    }

    void MergeRpcDecls( List<FbRpcDecl> otherRpcDecls, bool overwrite = true )
    {
        foreach (var e in otherRpcDecls)
        {
            var has = this.rpcDecls.Find( x => x.serviceName == e.serviceName );
            if (has != null)
            {
                has.MergeFrom( e, overwrite );
            }
            else
            {
                this.rpcDecls.Add( e );
            }
        }
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

