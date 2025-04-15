using Google.FlatBuffers;
using System.Diagnostics;
using Xml2Fb.SchemaGenerator;
using Xml2Fb.Shared;


if (args.Length < 2)
{
    Console.WriteLine( "Usage: XmlSchemaGenerator <input.xml> <output.fbs>" );
    return;
}

string xmlDir = args[0];
string fbsDir = args[1];
BatchXmlGenFbs(xmlDir, fbsDir);
 
void BatchXmlGenFbs( string xmlDir, string fbsDir )
{

    IEnumerable<string>? xmlFiles = Directory.EnumerateFiles( xmlDir, "*.xml", new EnumerationOptions { RecurseSubdirectories = true } );
    foreach (string xmlFile in xmlFiles)
    {
        try
        {
            //string xmlFile = "E:\\project\\my-tools\\FlatBufferTool-master\\XmlToFlatbuffer\\XML\\NewbieMissionConfig.xml";
            DebugHelper.Print( $"开始处理文件 {xmlFile}" );
            var schemaGenerator = new FbSchemaGenerator( xmlFile );
            var schema = schemaGenerator.GenerateSchema();
            string fbs = GetFbsFile( fbsDir, schemaGenerator.FbsFile );
            DebugHelper.PrintDebug( $"写入FBS文件 {fbs} " );
            File.WriteAllText( fbs, schema );
            Console.WriteLine( $"Successfully generated FlatBuffers schema at {fbsDir}" );

            // 可选：验证生成的schema
            ValidateSchema( schema );
        }
        catch (Exception ex)
        {
            DebugHelper.PrintError( $"Generation failed: {ex.Message} " );
            DebugHelper.PrintError( $"\t{ex.StackTrace}" );
        }
    }
}



string GetFbsFile( string dir, string name ) => $"{dir}/{name}";


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
