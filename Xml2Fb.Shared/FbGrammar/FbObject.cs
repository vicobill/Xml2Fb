using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xml2Fb.Shared.FbGrammar;

/// <summary>
/// EBNF格式：
/// object = `{` commasep( ident `:` value ) `}`
/// </summary>
public class FbObject
{
    Dictionary<string, FbValue> values = new();

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();

        sb.Append( '{' );
        int i = 0;
        foreach(var kv in values)
        {
            sb.Append( kv.Key ).Append( ':' ).Append( kv.Value );
            if (++i < values.Count)
            {
                sb.Append(',' );
            }
        }

        sb.Append( '}' );

        return sb.ToString();
    }
}
/// <summary>
/// EBNF格式：
///  value = single_value | object | `[` commasep( value ) `]`
/// </summary>
internal class FbValue
{

    public FbSingleValue? single_value;
    public FbObject? @object;
    public List<FbValue>? values;

    public FbValue(FbSingleValue singleValue)
    {
        single_value = singleValue;
        @object = null;
        values = null;
    }
    public FbValue(FbObject @object)
    {
        this.@object= @object;
        single_value = null;
        values = null;
    }

    public FbValue(List<FbValue> values)
    {
        this.values = values;
        single_value = null;
        @object = null;
    }

    public override string ToString()
    {
        if(single_value != null)
        {
            return single_value.ToString();
        }
        if (@object != null)
        {
            return @object.ToString();
        }
        if (values != null)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append( '[' );
            foreach(var @value in values)
            {
                sb.Append( @value );
            }
            sb.Append( ']' );
            return sb.ToString();
        }
        return string.Empty;
    }

}
