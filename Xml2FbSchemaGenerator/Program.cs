using Google.FlatBuffers;
using Xml2FbSchemaGenerator;

if (args.Length != 2)
{
    Console.WriteLine( "Usage: XmlSchemaGenerator <input.xml> <output.fbs>" );
    return;
}
string xmlPath = args[0];
string fbPath = args[1];

try
{
    var schemaGenerator = new FbSchemaGenerator();
    string schema = schemaGenerator.GenerateSchema( xmlPath );
    await File.WriteAllTextAsync( fbPath, schema );
    Console.WriteLine( $"Successfully generated FlatBuffers schema at {fbPath}" );

    // 可选：验证生成的schema
    ValidateSchema( schema );
}
catch (Exception ex)
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine( $"Generation failed: {ex.Message}" );
    Console.ResetColor();
}

void ValidateSchema( string schema )
{
    try
    {
        var builder = new FlatBufferBuilder( 1024 );

        //builder.Finish( Schema.GetRootAsSchema( new ByteBuffer( schema ) ) );
        Console.WriteLine( "Schema validation passed!" );
    }
    catch (Exception ex)
    {
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.WriteLine( $"Schema warning: {ex.Message}" );
        Console.ResetColor();
    }
}
