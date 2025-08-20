using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xml2Fb.Shared;

namespace Xml2Fb.Shared.FbGrammar;


/// <summary>
/// EBNF格式：
/// rpc_decl = `rpc_service` ident `{` rpc_method+ `}`
/// </summary>
public class FbRpcDecl
{
    public required string serviceName;
    internal List<RpcMethodDecl> rpcMethodDecls = new();

    public FbRpcDecl WithMethod( string methodName, string argTypeName, string retTypeName, FbMetadata? metadata = null )
    {
        var has = rpcMethodDecls.Find( ( x ) => x.methodName == methodName );
        if (has != null)
        {
            if (has.argTypeName != argTypeName || has.retTypeName != retTypeName)
            {
                DebugHelper.PrintError( $"rpc {serviceName} already has method `{methodName}`, but with different return type `{has.retTypeName}`(expect:`{retTypeName}` and arg type `{has.argTypeName}`(expect:`{argTypeName}`)" );
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
            rpcMethodDecls.Add( new RpcMethodDecl
            {
                methodName = methodName,
                argTypeName = argTypeName,
                retTypeName = retTypeName,
                metadata = metadata
            } );
        }
        return this;
    }

    // rpc_service ServiceName{
    //  MethodName(ArgType):RetType;
    // }
    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append( $"rpc_service {serviceName} " ).Append( '{' )
            .AppendLine();
        foreach (var rpcMethod in rpcMethodDecls)
        {
            sb.Append( '\t' ).Append( rpcMethod )
                .AppendLine();
        }
        sb.Append( '}' ).AppendLine();

        return sb.ToString();
    }

    internal void MergeFrom( FbRpcDecl other, bool overwrite )
    {
        foreach (var e in other.rpcMethodDecls)
        {
            var has = this.rpcMethodDecls.Find( x => x.methodName == e.methodName );
            if (has != null)
            {
                has.MergeFrom( e, overwrite );
            }
            else
            {
                this.rpcMethodDecls.Add( e );
            }
        }
    }
}

/// <summary>
/// EBNF格式：
/// rpc_method = ident `(` ident `)` `:` ident metadata `;`
///
/// methodName (argTypeName) : retTypeName;
/// </summary>
internal class RpcMethodDecl
{
    public required string methodName;
    public required string argTypeName;
    public required string retTypeName;
    public FbMetadata? metadata;

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();


        sb.Append( '\t' )
            .Append( $"{methodName} ({argTypeName}) : {retTypeName}" )
            .Append( metadata )
            .Append( ';' );

        return sb.ToString();
    }

    internal void MergeFrom( RpcMethodDecl other, bool overwrite )
    {
        if (methodName != other.methodName)
        {
            throw new ArgumentException( "methodName must be same!" );
        }
        if (overwrite)
        {
            argTypeName = other.argTypeName;
            retTypeName = other.retTypeName;
            metadata = other.metadata;
        }
        else
        {
            if (argTypeName != other.argTypeName)
            {
                DebugHelper.PrintWarning( $"arg type {argTypeName} not same as {other.argTypeName}" );
            }
            if (retTypeName != other.retTypeName)
            {
                DebugHelper.PrintWarning( $"ret type {retTypeName} not same as {other.retTypeName}" );
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
    }
}
