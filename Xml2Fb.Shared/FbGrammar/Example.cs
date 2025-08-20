namespace Xml2Fb.Shared.FbGrammar;

internal static class Example
{

    public static FbSchema Cons()
    {
        FbSchema schema = new FbSchema();

        schema.Include( "basetype" )
        .WithAttribute( "priority", needQuote: true )
        .WithNamespace( "FbTest" )
        .WithEnumDecl( new FbEnumDecl
        {
            enumName = "Color",
            underlyingType = EFbScalarType.FBT_Byte
        }.WithField( "Red", 1 )
            .WithField( "Green" )
            .WithField( "Blue" )
        ).WithEnumDecl( new FbEnumDecl
        {
            enumName = "Any",
            isUnion = true,
        }.WithField( "Monster" )
            .WithField( "Weapon" )
            .WithField( "PickUp" )
        )
        .WithTypeDecl( new FbTypeDecl
        {
            isStruct = true,
            typeName = "Vec3"
        }.WithField( "x", "float" )
        .WithField( "y", "float" )
        .WithField( "z", "float" )
        ).WithTypeDecl( new FbTypeDecl
        {
            isStruct = false,
            typeName = "Monster"
        }.WithField( "pos", "Vec3" )
            .WithField( "mana", "short", new FbSingleValue( 150 ) )
            .WithField( "hp", "short", new FbSingleValue( 100 ) )
            .WithField( "name", "string" )
            .WithField( "friendly", "bool", new FbSingleValue( false ),
            new FbMetadata().WithKey( "deprecated" ).WithKey( "priority", new FbSingleValue( 1 ) )
            )
            .WithField( "inventory", "[ubyte]" )
            .WithField( "test", "Any" )
        ).WithTypeDecl( new FbTypeDecl
        {
            isStruct = false,
            typeName = "Weapon"
        }
        ).WithTypeDecl( new FbTypeDecl
        {
            isStruct = false,
            typeName = "PickUp"
        }
        ).WithRootType( "Monster" )
        .WithFileExt("FBIN")
        .WithFileIdentifier("NIBF");

        return schema;
    }

    public static void Write( FbSchema schema )
    {
        schema.schemaName = "Example";
        File.WriteAllText( "Example-out.fbs", schema.ToString() );
    }
}
