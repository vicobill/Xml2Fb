using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xml2Fb.FbGrammar;
using Xml2Fb.Shared;

namespace Xml2Fb;

class SchemaBuilder
{
    public SchemaBuilder(string rootTableName)
    {
        _schema = new FbSchema { schemaName = rootTableName }
        .WithRootType(rootTableName)
        .WithNamespace( Conf.Instance == null ? "Config.ConfigObj.FBin" : Conf.Instance.xml2fb.pkgname)
        .Include("basetype")
        ;

    }

    private FbSchema _schema;

    private readonly Stack<FbTypeDecl> _context = new();

    public void AddTable( FbTypeDecl table )
    {
        _schema.WithTypeDecl( table );

        if (_context.TryPeek( out var result ))
        {
            if (result.typeName != table.typeName)
                _context.Push( table );
        }
    }



    public bool TryPeek(string tableName,  out FbTypeDecl? table )
    {
        return _schema.TryPeek( tableName, out table );
        
    }

    public override string ToString()
    {
        return _schema.ToString();
    }
}
