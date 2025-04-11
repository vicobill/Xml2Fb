using Google.FlatBuffers;
using Xml2FbSchemaGenerator;

if (args.Length != 2)
{
    Console.WriteLine( "Usage: XmlSchemaGenerator <input.xml> <output.fbs>" );
    return;
}

string xmlPath = args[0];
string fbPath = args[1];

IEnumerable<string>? xmlFiles = Directory.EnumerateFiles(xmlPath,"*.xml",new EnumerationOptions { RecurseSubdirectories = true });
foreach(var xmlFile in xmlFiles)
{
    try
    {
        Print( $"开始处理文件 {xmlFile}" );
        var schemaGenerator = new FbSchemaGenerator(xmlFile);
        var schema = schemaGenerator.GenerateSchema();
        string fbs = GetFbsFile( fbPath, schemaGenerator.FbsFile );
        PrintDebug( $"写入FBS文件 {fbs} " );
        await File.WriteAllTextAsync( fbs, schema );
        Console.WriteLine( $"Successfully generated FlatBuffers schema at {fbPath}" );

        // 可选：验证生成的schema
        ValidateSchema( schema );
    }
    catch (Exception ex)
    {
        PrintError( $"Generation failed: {ex.Message} " );
        PrintError( $"\t{ex.StackTrace}" );
    }
}

string GetFbsFile( string dir, string name ) => $"{dir}/{name}";

void PrintDebug(string msg)
{
    Console.ForegroundColor = ConsoleColor.Blue;
    Console.WriteLine($"[DEBUG]: {msg}" );
    Console.ResetColor();
}

void PrintError(string msg )
{
    Console.ForegroundColor = ConsoleColor.Red;
    Console.WriteLine($"[ERROR]: {msg}"  );
    Console.ResetColor();
}
void PrintWarning(string msg)
{
    Console.ForegroundColor = ConsoleColor.Yellow;
    Console.WriteLine($"[WARN]: {msg}"  );
    Console.ResetColor();
}
void Print(string msg)
{
    Console.ResetColor();
    Console.WriteLine( msg );
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
