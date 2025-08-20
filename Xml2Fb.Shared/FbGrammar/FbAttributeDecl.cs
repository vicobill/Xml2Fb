using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xml2Fb.Shared.FbGrammar;

/// <summary>
/// EBNF格式：
/// attribute_decl = `attribute` ident | `"` ident `"` `;`
/// </summary>
public class FbAttributeDecl
{
    // attribute "priority";
    // (deprecated)
    // id: n
    // required
    // force_align: size
    // bit_flags
    // nested_flatbuffer: "table_name"
    // flexbuffer
    // key
    // hash
    // original_order
    // native
    // attribute ident | "ident";
    public bool needQuote;
    public required string attrName;

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        if (needQuote)
        {
            sb.Append( $"attribute \"{attrName}\";" );
        }
        else
        {
            sb.Append( $"attribute {attrName};" );
        }

        return sb.ToString();
    }

    internal void MergeFrom( FbAttributeDecl other, bool overwrite )
    {
        if (overwrite)
        {
            needQuote = other.needQuote;
            attrName = other.attrName;
        }
    }
}
