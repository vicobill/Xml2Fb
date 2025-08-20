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
    /// metadata = [ `(` commasep( ident [ `:` single_value ] ) `)` ]
    /// </summary>
    internal class FbMetadata
    {
        public Dictionary<string, FbSingleValue> items = new();

        /// <summary>
        /// 合并其它的Metadata
        /// </summary>
        /// <param name="other"></param>
        /// <param name="overwrite">是否覆盖</param>
        public void MergeFrom( FbMetadata? other, bool overwrite = false )
        {
            if (other == null)
                return;
            foreach (var kv in other.items)
            {
                if (items.TryGetValue( kv.Key, out var value ))
                {
                    if (overwrite)
                    {
                        items[kv.Key] = value;
                    }
                    else
                    {
                        DebugHelper.PrintWarning( $"already has key:{kv.Key}, skip!" );
                    }
                }
                else
                {
                    items.Add( kv.Key, kv.Value );
                }
            }
        }

        public FbMetadata WithKey( string key, FbSingleValue? singleValue = null )
        {
            bool has = items.TryGetValue( key, out var value );
            if (has)
            {
                DebugHelper.PrintWarning( $"Already has attribute {key}" );
            }
            if (singleValue is null)
            {
                items.Add( key, FbSingleValue.Empty );
            }
            else
            {
                items.Add( key, singleValue );
            }

            return this;
        }

        public bool HasItem => items.Count > 0;



        // ( k: v, k, k: v )
        public override string ToString()
        {
            if (items.Count == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder();
            sb.Append( '(' );
            int i = 0;
            foreach (var item in items)
            {
                sb.Append( item.Key );
                if (item.Value.hasValue)
                {
                    sb.Append( ":" ).Append( item.Value );
                }
                if (++i < items.Count)
                {
                    sb.Append( ',' );
                }
            }

            sb.Append( ')' );

            return sb.ToString();
        }
    }

}
