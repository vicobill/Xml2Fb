using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xml2Fb.Shared;

namespace Xml2Fb.FbGrammar
{

    /// <summary>
    /// EBNF格式：
    /// rpc_decl = `rpc_service` ident `{` rpc_method+ `}`
    /// </summary>
    internal class FbRpcDecl
    {
        public required string serviceName;
        public List<RpcMethodDecl> rpcMethodDecls = new();

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
    }
}
