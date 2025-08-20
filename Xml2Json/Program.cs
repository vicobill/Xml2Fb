using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using System;
using System.Diagnostics;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;
using Xml2Fb;
using Xml2Fb.Shared;
using CommandLine;
using Xml2Fb.FbGrammar;



//Example.Write( Example.Cons() );

//return;

string confFile = "xml2fb.json";
bool isDeployMode = false;

Parser.Default.ParseArguments<CmdOpts>( args )
    .WithParsed( opts =>
    {
        confFile = opts.Conf;
        isDeployMode = opts.Deploy;
    } );

// 加载配置文件

//confFile = "E:\\project\\my-tools\\Xml2FbSchemaGenerator\\Assets\\xml2fb.json";

Conf? conf = LoadConf( confFile );
if (conf == null)
{
    DebugHelper.PrintError( "Not Specific `xml2fb.json`" );
    return;
}
bool _is_start = false;
conf.isDeployMode = isDeployMode;
Directory.SetCurrentDirectory( conf?.xml2fb?.workdir );

/*
 非发布模式下，将生成的csharp,fbin拷贝到Unity项目目录
 */

if (!isDeployMode)
{

    DebugHelper.Print("----------------- Run in Develop Mode -----------------");
    DebugHelper.Print("-------------------------------------------------------");

    string xmldir = conf.GetXmlDir().Standard();
    if (!Directory.Exists( xmldir ))
    {
        throw new DirectoryNotFoundException( $"{xmldir} not found!" );
    }

    IEnumerable<string>? xmlFiles = Directory.EnumerateFiles(
            xmldir, "*.xml",
            new EnumerationOptions { RecurseSubdirectories = conf.xml2fb.isrec }
        ).Where(x => conf.InWhitelist(x));


    foreach (var xmlFile in xmlFiles)
    {
        //string xmlFile = $"{conf.GetXmlDir()}/BlindBoxConfig.xml";
        string fileName = Path.GetFileNameWithoutExtension( xmlFile );
        string jsonFile = Path.Combine( conf.GetJsonDir(), $"{fileName}.json" );
        string fbsFile = Path.Combine( conf.GetFbsDir(), $"{fileName}.fbs" );


        await Task.Run( () =>
        {
            DebugHelper.Print( $"Process Xml file: {fileName}.xml" );
            XmlDocument doc = LoadXml( xmlFile );
            GenFbSchema( doc, xmlFile, conf );
            SaveJsonAsync( doc, jsonFile );
        } ).ConfigureAwait( false );

        await FlatcGenFbsIncCsharpCode( conf );


        await Task.Run( async () =>
        {
            await FlatcGenCsharpCode( fbsFile, conf );
            CopyCsharpToUnityTargetDir( fbsFile, conf );

            await FlatcJsonToBin( fileName, conf );
            CopyBinToDevTargetDir( fileName, conf );
            FlatcBinToJson( fileName, conf );
            DebugHelper.PrintOk( $"Done Xml file {fileName}.xml !" );
        } ).ConfigureAwait( false );



    }

    DebugHelper.Print( "DONE!" );
}
/* 发布模式下，拷贝xml为fbin，放置在目标目录下
 * 不处理csharp */
else
{
    DebugHelper.Print("----------------- Run in Deply Mode -----------------");
    DebugHelper.Print(" only copy *.xml as *.bytes (fbin) for Unity Bundle ");
    DebugHelper.Print("-------------------------------------------------------");

    IEnumerable<string>? xmlFiles = Directory.EnumerateFiles(
            conf.GetUnityDeployXmlDir(), "*.xml",
            new EnumerationOptions { RecurseSubdirectories = conf.xml2fb.isrec }
         ).Where( x => conf.InWhitelist( x ) );

    foreach (var xmlFile in xmlFiles)
    {
        string fileName = Path.GetFileNameWithoutExtension( xmlFile );
        string jsonFile = Path.Combine( conf.GetJsonDir(), $"{fileName}.json" );
        string fbsFile = Path.Combine( conf.GetFbsDir(), $"{fileName}.fbs" );


        await Task.Run( () =>
        {
            DebugHelper.Print( $"Process Xml file: {fileName}" );
            XmlDocument doc = LoadXml( xmlFile );
            GenFbSchema( doc, xmlFile, conf );
            SaveJsonAsync( doc, jsonFile );
        } ).ConfigureAwait( false );

        //await FlatcGenFbsIncCsharpCode( conf );

        await Task.Run( async () =>
        {
            //FlatcGenCsharpCode( fbsFile, conf );
            await FlatcJsonToBin( fileName, conf );
            //FlatcBinToJson( fileName, conf );
            CopyBinToDeployTargetDir( fileName, conf );
            DebugHelper.PrintOk( $"Done Xml file: {fileName} !" );
        } ).ConfigureAwait( false );



    }

    DebugHelper.Print( "DONE!" );
}

string GetFbsGeneratedCsharpName( string fbsFile )
{
    string name = Path.GetFileNameWithoutExtension( fbsFile );
    return $"{name}_generated.cs";
}

void CopyCsharpToUnityTargetDir( string fbsFile, Conf conf )
{
    try
    {
        string cs = GetFbsGeneratedCsharpName( fbsFile );
        string src = $"{conf.GetCsharpDir()}/{cs}";
        string dst = $"{conf.GetUnityCsharpDir()}/{cs}";
        File.Copy( src, dst, true );
    }
    catch (Exception ex)
    {
        DebugHelper.PrintError( ex.Message );
    }
}

void CopyBinToDevTargetDir( string fileName, Conf conf )
{
    try
    {

        string src = $"{conf.GetFbinDir()}/{fileName}.bin";
        string dst = $"{conf.GetUnityDevFbinDir()}/{fileName}.bytes";
        File.Copy( src, dst, true );

        // 如果同目录下存在同名xml文件，删除xml文件
        string xml = $"{conf.GetUnityDevFbinDir()}/{fileName}.xml";
        if (File.Exists( xml ))
        {
            File.Delete( xml );
        }
    }
    catch (Exception ex)
    {
        DebugHelper.PrintError( ex.Message );
    }
}

void CopyBinToDeployTargetDir( string fileName, Conf conf )
{
    try
    {
        string src = $"{conf.GetFbinDir()}/{fileName}.bin";
        string dst = $"{conf.GetUnityDeployFbinDir()}/{fileName}.bytes";
        File.Copy( src, dst, true );
    }
    catch (Exception ex)
    {
        DebugHelper.PrintError( ex.Message );
    }
}

void GenFbSchema( XmlDocument doc, string xmlFile, Conf conf )
{
    //DebugHelper.Print( $"[GenFbs] 开始生成FBS文件： {xmlFile}" );
    var schemaGenerator = new SchemaGenerator( xmlFile );
    var schema = schemaGenerator.GenerateSchema( doc );
    string fbs = GetFbsFile( conf.GetFbsDir(), schemaGenerator.FbsFileName );
    //DebugHelper.PrintDebug( $"[GenFbs] 写入FBS文件 {fbs} " );
    File.WriteAllText( fbs, schema );
}


string GetFbsFile( string dir, string name ) => $"{dir}/{name}";


async Task FlatcJsonToBin( string fileName, Conf conf )
{
    Process process = new Process();
    process.StartInfo.FileName = conf.GetFlatc();
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.CreateNoWindow = true;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.Arguments = $"-o {conf.GetFbinDir()} -I {conf.GetFbsIncDir()} -b {conf.GetFbsDir()}/{fileName}.fbs {conf.GetJsonDir()}/{fileName}.json";

    process.OutputDataReceived += ( s, e ) => DebugHelper.PrintWarning( $"[Json2Bin] {fileName} {e.Data}" );
    process.ErrorDataReceived += ( s, e ) => DebugHelper.PrintError( $"[Json2Bin] {fileName} {e.Data}" );
    //DebugHelper.Print( $"[Json2Bin] 执行flatc json2bin {fileName}.json" );
    process.Start();

    // 读取标准输出
    string output = process.StandardOutput.ReadToEnd();
    // 读取错误输出
    string error = process.StandardError.ReadToEnd();

    await process.WaitForExitAsync(); // 等待命令执行完成

    // 打印结果
    //Console.WriteLine( "标准输出:" );
    //Console.WriteLine( output );

    if (!string.IsNullOrEmpty( error ))
    {
        DebugHelper.PrintError( $"Error: {error}" );
    }

    //Console.WriteLine( $"\n退出代码: {process.ExitCode}" );




}



async Task FlatcBinToJson( string fileName, Conf conf )
{
    Process process = new Process();
    process.StartInfo.FileName = conf.GetFlatc();
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.CreateNoWindow = false;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.Arguments = $"-o {conf.GetVerifyJsonDir()} -I {conf.GetFbsIncDir()} --natural-utf8 --raw-binary -t {conf.GetFbsDir()}/{fileName}.fbs -- {conf.GetFbinDir()}/{fileName}.bin";

    process.OutputDataReceived += ( s, e ) => DebugHelper.PrintWarning( $"[Bin2Json] {fileName} {e.Data}" );
    process.ErrorDataReceived += ( s, e ) => DebugHelper.PrintError( $"[Bin2Json] {fileName} {e.Data}" );
    //DebugHelper.Print( $"[Json2Bin] 执行flatc json2bin {fileName}.json" );
    process.Start();

    // 读取标准输出
    string output = process.StandardOutput.ReadToEnd();
    // 读取错误输出
    string error = process.StandardError.ReadToEnd();

    await process.WaitForExitAsync(); // 等待命令执行完成

    // 打印结果
    //Console.WriteLine( "标准输出:" );
    //Console.WriteLine( output );

    if (!string.IsNullOrEmpty( error ))
    {
        DebugHelper.PrintError( $"Error: {error}" );
    }
}

string get_gen_cs_cmd( Conf conf, string fbsFile )
{
    if (conf.xml2fb.isthin)
        return $"-o {conf.GetCsharpDir()} -I {conf.GetFbsIncDir()} --gen-onefile --csharp {fbsFile} ";
    else
        return $"-o {conf.GetCsharpDir()} -I {conf.GetFbsIncDir()} --gen-onefile --gen-compare --gen-object-api --gen-nullable --cs-gen-json-serializer --csharp {fbsFile} ";
}
async Task FlatcGenFbsIncCsharpCode( Conf conf )
{
    if (_is_start)
        return;

    _is_start = true;
    var fbsList = Directory.GetFiles( conf.GetFbsIncDir(), "*.fbs" );
    foreach (var fbsFile in fbsList)
    {

        DebugHelper.PrintDebug( $"[GenC#] process fbs file:{fbsFile}" );
        Process process = new Process();
        process.StartInfo.FileName = conf.GetFlatc();
        process.StartInfo.RedirectStandardError = true;
        process.StartInfo.RedirectStandardOutput = true;
        process.StartInfo.UseShellExecute = false;
        process.StartInfo.Arguments = get_gen_cs_cmd( conf, fbsFile );

        process.OutputDataReceived += ( s, e ) => DebugHelper.PrintWarning( $"[GenC#] {fbsFile} {e.Data}" );
        process.ErrorDataReceived += ( s, e ) => DebugHelper.PrintError( $"[GenC#] {fbsFile} {e.Data}" );
        process.Start();

        await process.WaitForExitAsync(); // 等待命令执行完成

        string stdout = await process.StandardOutput.ReadToEndAsync();

        string stderr = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();
        if (!string.IsNullOrEmpty( stdout ))
            DebugHelper.PrintWarning( stdout );
        if (!string.IsNullOrEmpty( stderr ))
            DebugHelper.PrintError( stderr );

        CopyCsharpToUnityTargetDir( fbsFile, conf );
    }
}

async Task FlatcGenCsharpCode( string fbsFile, Conf conf )
{
    DebugHelper.PrintDebug( $"[GenC#] process :{fbsFile}" );
    Process ps = new Process();
    ps.StartInfo.FileName = conf.GetFlatc();
    ps.StartInfo.RedirectStandardError = true;
    ps.StartInfo.RedirectStandardOutput = true;
    ps.StartInfo.UseShellExecute = false;
    ps.StartInfo.Arguments = get_gen_cs_cmd( conf, fbsFile );

    ps.OutputDataReceived += ( s, e ) => DebugHelper.PrintWarning( $"[GenC#] {fbsFile} {e.Data}" );
    ps.ErrorDataReceived += ( s, e ) => DebugHelper.PrintError( $"[GenC#] {fbsFile} {e.Data}" );
    ps.Start();
    await ps.WaitForExitAsync();


    string stdout = await ps.StandardOutput.ReadToEndAsync();
    if (!string.IsNullOrEmpty( stdout ))
        DebugHelper.PrintWarning( stdout );
    string stderr = await ps.StandardError.ReadToEndAsync();
    if (!string.IsNullOrEmpty( stderr ))
        DebugHelper.PrintError( stderr );

}

XmlDocument LoadXml( string xmlFile )
{
    //return Task.Run<XmlDocument>( () =>
    //{
    using FileStream stream = File.OpenRead( xmlFile );
    using var xmlreader = XmlReader.Create( stream, new XmlReaderSettings
    {
        IgnoreComments = true,
        IgnoreProcessingInstructions = true,
        IgnoreWhitespace = true,
        Async = true

    } );
    XmlDocument doc = new XmlDocument();
    doc.Load( xmlreader );

    return doc;
    //} );

}

void SaveJsonAsync( XmlDocument doc, string jsonFile )
{
    //DebugHelper.Print( $"处理文件 {jsonFile}" );
    string name = Path.GetFileNameWithoutExtension( jsonFile );
    XmlNodeConverter converter = new MyXmlNodeConverter { OmitRootObject = false, WriteArrayAttribute = true };
    var settings = new JsonSerializerSettings
    {
        Converters = new List<JsonConverter> { converter },
        NullValueHandling = NullValueHandling.Ignore,

    };
    string json2 = JsonConvert.SerializeObject( doc, Newtonsoft.Json.Formatting.None, settings );

    JObject? jobj = JObject.Parse( json2 );

    bool rm = jobj.Remove( "?xml" );

    ConfigNodeUp( jobj );

    string json = jobj.ToString();
    string ret = Regex.Replace( json, @"@(\w+)", ( m ) =>
     {
         return m.Groups[1].Value.Codize();
     }, RegexOptions.Multiline );

    File.WriteAllText( jsonFile, ret );
}

void ConfigNodeUp( JObject jobj )
{
    IJEnumerable<JToken>? config = jobj.Property( "Config" ).Values();
    try
    {
        if (config != null && config.Count() > 0)
        {
            jobj.Remove( "Config" );
            jobj.Add( config );
        }
    }
    catch
    {
        try
        {
            config = jobj.Property( "config" ).Values();
            if (config != null)
            {
                jobj.Remove( "config" );
            }
            if (config.Count() > 0)
            {
                jobj.Add( config );
            }
        }
        catch { }
    }

}


Conf? LoadConf( string confFile = null )
{

    if (string.IsNullOrEmpty( confFile ))
    {
        confFile = Path.Combine( Directory.GetCurrentDirectory(), "xml2fb.json" );
    }
    try
    {
        var jsonSerializer = JsonSerializer.CreateDefault();
        using StreamReader sr = File.OpenText( confFile );
        using var reader = new JsonTextReader( sr );
        Conf? conf = jsonSerializer.Deserialize<Conf>( reader );
        conf?.EnsureDir();
        Conf.Instance = conf!;
        return conf;
    }
    catch (Exception ex)
    {
        DebugHelper.PrintError( ex.Message );
        DebugHelper.PrintError( ex.StackTrace );
        return null;
    }
}



public class CmdOpts
{
    /// <summary>
    /// 配置文件 xml2fb.json
    /// </summary>
    [Option( 'c', "config" )]
    public string Conf { get; set; }

    /// <summary>
    /// 是否发布模式
    /// </summary>
    [Option( 'd', "deploy" )]
    public bool Deploy { get; set; }

}


