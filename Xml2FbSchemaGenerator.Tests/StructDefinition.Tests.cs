using NUnit.Framework;
using Xml2FbSchemaGenerator;

namespace Xml2FbSchemaGenerator.Tests;

[TestFixture]
public class PrimeService_IsPrimeShould
{
    StructDefinition structDefinition;
    [SetUp]
    public void SetUp()
    {
        structDefinition = new StructDefinition( "Vec3" );
    }


    [Test]
    public void IsPrime_InputIs1_ReturnFalse()
    {
        Assert.That( structDefinition.Name == "Vec3", Is.True);

        structDefinition.AddField( "x", "float" )
            .AddField( "y", "double" )
            .AddField( "z", "short" );

        string s = structDefinition.ToString();

        Assert.That( s, Is.EqualTo(@"struct Vec3 {
    x : float;
    y : double;
    z : short;
}" ));


    }
}
