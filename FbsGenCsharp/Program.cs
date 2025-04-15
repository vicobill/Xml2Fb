// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using Xml2Fb.Shared;

Console.WriteLine( "Hello, World!" );

if (args.Length < 2)
{
    Console.WriteLine( "Usage: FbsGenCsharp <input *.fbs directory> <flatc.exe>" );
    return;
}

string fbsDir = args[0];
string flatc = args[1];
BatchFbsGenCsharp( flatc, fbsDir, fbsDir.Replace( "fbs", "cs" ) );

void BatchFbsGenCsharp( string flatc, string fbsDir, string csDir )
{
    string incDir = "E:\\project\\my-tools\\Xml2FbSchemaGenerator\\Assets\\fbs";
    var fbsFiles = Directory.EnumerateFiles( fbsDir, "*.fbs", new EnumerationOptions { RecurseSubdirectories = true } );
    foreach (string fbsFile in fbsFiles)
    {
        DebugHelper.PrintDebug( $"处理文件:{fbsFile}" );
        var psi = new ProcessStartInfo
        {
            FileName = flatc,
            Arguments = $"-o {csDir} -I {incDir} --gen-onefile --gen-compare --gen-object-api --gen-nullable --csharp {fbsFile} "
        };
        Process.Start( psi );
    }
}
