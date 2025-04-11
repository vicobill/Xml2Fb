using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Xml2FbSchemaGenerator.Tests
{
    class StringTests
    {
        [TestCase( "dbcTitle" )]
        [TestCase( "DbcTitle" )]
        [TestCase( "DBCTitle" )]
        [TestCase("DBC_TITLE")]
        [TestCase( "Dbc_Title" )]
        [Test]
        public void String_Tests( string s )
        {
            string r = s.LowerSnakeCase();
            Assert.That( r == "dbc_title", Is.True );
        }

    }
}
