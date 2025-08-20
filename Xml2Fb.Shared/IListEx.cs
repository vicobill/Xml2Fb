using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xml2Fb.Shared;

public static class IListEx
{
    public static bool IsEmpty<T>(this IList<T> list)
    {
        return list is null || list.Count == 0;
    }
    public static bool IsNotEmpty<T>(this IList<T> list)
    {
        return !( IsEmpty( list ) );
    }
}
