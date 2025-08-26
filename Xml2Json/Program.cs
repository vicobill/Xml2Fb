using CommandLine;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Utilities;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Schema;
using Xml2Fb;
using Xml2Fb.Shared;
using Xml2Fb.Shared.FbGrammar;


//GroupSubXmlFiles( new List<string> {
//    "C:/a/b/ShopConfig.xml",
//    "C:/a/b/ShopConfig1.xml",
//    "C:/a/b/ShopConfig2.xml",
//    "C:/a/b/ShopConfig3.xml",
//    "C:/a/b/BaseShopConfig.xml",
//    "C:/a/b/ActivityConfig.xml",
//}, out var result );


//Example.Write( Example.Cons() );

//return;
try
{
    string confFile = "xml2fb.json";
    bool isDeployMode = false;

    Parser.Default.ParseArguments<CmdOpts>( args )
        .WithParsed( opts =>
        {
            confFile = opts.Conf;
            isDeployMode = opts.Deploy;
        } );



    Conf? conf = LoadConf( confFile );
    if (conf == null)
    {
        DebugHelper.PrintError( "Not Specific `xml2fb.json`" );
        return;
    }
    DebugHelper.logLevel = (DebugHelper.ELogLevel)conf.xml2fb.loglevel;
    bool _is_start = false;
    conf.isStart = _is_start;
    conf.isDeployMode = isDeployMode;
    Directory.SetCurrentDirectory( conf?.xml2fb?.workdir );

    /*
     非发布模式下，将生成的csharp,fbin拷贝到Unity项目目录
     */
    string xmldir = conf.GetXmlDir().Standard();
    if (!Directory.Exists( xmldir ))
    {
        throw new DirectoryNotFoundException( $"{xmldir} not found!" );
    }

    List<string>? xmlFiles = Directory.EnumerateFiles(
            xmldir, "*.xml",
            new EnumerationOptions { RecurseSubdirectories = conf.xml2fb.isrec }
        ).Where( x => conf.InWhitelist( x ) ).ToList();

    bool isgrouped = GroupSubXmlFiles( xmlFiles, out Dictionary<string, List<string>>? groups, out List<string>? scatterXmlFiles );

    if (!isDeployMode)
    {

        DebugHelper.Print( "----------------- Run in Develop Mode -----------------" );
        DebugHelper.Print( "-------------------------------------------------------" );

        if (!isgrouped)
        {
            ProcessScatterXmlFiles_Develop( xmlFiles, conf );
            DebugHelper.PrintOk( "DONE!" );
        }
        else
        {
            ProcessGroupXmlFiles_Develop( groups!, conf );
            ProcessScatterXmlFiles_Develop( scatterXmlFiles, conf );
            DebugHelper.PrintOk( "DONE Scatter xmls!" );
        }
    }

    else
    {
        DebugHelper.Print( "----------------- Run in Deply Mode -----------------" );
        DebugHelper.Print( " only copy *.xml as *.bytes (fbin) for Unity Bundle " );
        DebugHelper.Print( "-------------------------------------------------------" );
        if (!isgrouped)
        {
            ProcessScatterXmlFiles_Deploy( xmlFiles, conf );
        }
        else
        {
            ProcessGroupXmlFiles_Deploy( groups!, conf );
            ProcessScatterXmlFiles_Deploy( scatterXmlFiles, conf );
            DebugHelper.PrintOk( "DONE Scatter xmls!" );
        }
    }
}
catch (Exception ex)
{
    DebugHelper.PrintError( ex.Message );
}

void ProcessScatterXmlFiles_Develop( List<string> scatterXmlFiles, Conf conf )
{
    Parallel.ForEach( scatterXmlFiles, ( xmlFile ) =>
    {
        string fileName = Path.GetFileNameWithoutExtension( xmlFile );
        string jsonFile = Path.Combine( conf.GetJsonDir(), $"{fileName}.json" );
        string fbsFile = Path.Combine( conf.GetFbsDir(), $"{fileName}.fbs" );

        DebugHelper.Print( $"Process Xml file: {fileName}.xml" );

        XmlDocument doc = LoadXml( xmlFile );
        SaveXmlToJson( doc, jsonFile );
        GenFbSchema( doc, xmlFile, conf );


        // 生成各个fbs对应的C#代码
        List<Task> tasks = new List<Task>();
        Task.Run( async () =>
        {
            await FlatcGenFbsIncCsharpCode( conf );

            await FlatcGenCsharpCode( fbsFile, conf );
            CopyCsharpToUnityTargetDir( fbsFile, conf );

            await FlatcJsonToBin( fileName, conf );
            CopyBinToDevTargetDir( fileName, conf );
            await FlatcBinToJson( fileName, conf );
        } ).Wait();

        DebugHelper.PrintOk( $"DONE Xml file {fileName}.xml !" );

    } );
}
/* 发布模式下，拷贝xml为fbin，放置在目标目录下
 * 不处理csharp */
void ProcessScatterXmlFiles_Deploy( List<string> scatterXmlFiles, Conf conf )
{
    Parallel.ForEach( scatterXmlFiles, ( xmlFile ) =>
    {
        string fileName = Path.GetFileNameWithoutExtension( xmlFile );
        string jsonFile = Path.Combine( conf.GetJsonDir(), $"{fileName}.json" );
        string fbsFile = Path.Combine( conf.GetFbsDir(), $"{fileName}.fbs" );

        DebugHelper.Print( $"Process Xml file: {fileName}" );

        XmlDocument doc = LoadXml( xmlFile );
        SaveXmlToJson( doc, jsonFile );
        GenFbSchema( doc, xmlFile, conf );
        FlatcJsonToBin( fileName, conf ).Wait();
        CopyBinToDeployTargetDir( fileName, conf );

        DebugHelper.PrintOk( $"DONE Xml file: {fileName} !" );
    } );
}

void ProcessGroupXmlFiles_Deploy( Dictionary<string, List<string>> groups, Conf conf )
{

    Parallel.ForEach( groups, ( group ) =>
    {
        string mainName = group.Key;

        Dictionary<string, string> jsonFiles = new Dictionary<string, string>();
        //List<FbSchema> fbSchemas = new List<FbSchema>();
        string finalFbsFile = GetManualFinalFbsFilepath( mainName, conf )!;
        bool isGenFbs = string.IsNullOrEmpty( finalFbsFile );
        if (isGenFbs)
        {
            DebugHelper.PrintError( "Must set `use_custom_fbs` in config under Deploy Mode.EXIT NOW!" );
            return;
        }

        var sorted = group.Value.Order().ToList();

        foreach (string xmlFile in sorted)
        {
            string fileName = Path.GetFileNameWithoutExtension( xmlFile );
            string jsonFile = Path.Combine( conf.GetJsonDir(), $"{fileName}.json" );

            XmlDocument doc = LoadXml( xmlFile );
            SaveXmlToJson( doc, jsonFile );

            jsonFiles.Add( fileName, jsonFile );
        }

        List<Task> tasks = new List<Task>();
        foreach (var kv in jsonFiles)
        {
            string fileName = kv.Key;
            tasks.Add( Json2BinAndCopy_Deploy_Async( fileName, conf, mainName ) );
        }
        Task.WaitAll( tasks );

        DebugHelper.PrintOk( $"DONE Grouped `{mainName}`" );
    } );
}

void ProcessGroupXmlFiles_Develop( Dictionary<string, List<string>> groups, Conf conf )
{
    Parallel.ForEach( groups, ( group ) =>
    {
        string mainName = group.Key;

        List<XmlDocument> xmlDocuments = new List<XmlDocument>();
        Dictionary<string, string> jsonFiles = new Dictionary<string, string>();
        List<FbSchema> fbSchemas = new List<FbSchema>();
        string finalFbsFile = GetManualFinalFbsFilepath( mainName, conf )!;
        bool isGenFbs = string.IsNullOrEmpty( finalFbsFile );

        var sorted = group.Value.Order().ToList();

        foreach (string xmlFile in sorted)
        {
            string fileName = Path.GetFileNameWithoutExtension( xmlFile );
            string jsonFile = Path.Combine( conf.GetJsonDir(), $"{fileName}.json" );

            XmlDocument doc = LoadXml( xmlFile );
            SaveXmlToJson( doc, jsonFile );

            if (isGenFbs)
            {
                FbSchema schema = GenerateSchema( doc, xmlFile, mainName, conf );
                fbSchemas.Add( schema );
            }

            xmlDocuments.Add( doc );
            jsonFiles.Add( fileName, jsonFile );
        }

        if (isGenFbs && fbSchemas.Count > 0)
        {
            FbSchema final_schema = fbSchemas[0];
            //File.WriteAllText( "0.fbs", final_schema.ToString() );
            for (int i = 1; i < fbSchemas.Count; i++)
            {
                //File.WriteAllText( $"{i}.fbs", fbSchemas[i].ToString() );
                final_schema.MergeFrom( fbSchemas[i] );
                //File.WriteAllText( $"0+{i}.fbs", final_schema.ToString() );
            }
            finalFbsFile = SaveFbSchema( final_schema, mainName, conf );
        }

        FlatcGenCsharpCode( finalFbsFile, conf ).Wait();
        CopyCsharpToUnityTargetDir( finalFbsFile, conf );

        List<Task> tasks = new List<Task>();
        foreach (var kv in jsonFiles)
        {
            string fileName = kv.Key;
            tasks.Add( Json2BinAndCopy_Dev_Async( fileName, conf, mainName ) );
        }
        Task.WaitAll( tasks );

        DebugHelper.PrintOk( $"DONE Grouped `{mainName}`" );
    } );
}

async Task Json2BinAndCopy_Deploy_Async( string fileName, Conf conf, string mainName )
{
    await FlatcJsonToBin( fileName, conf, mainName );
    CopyBinToDeployTargetDir( fileName, conf );
}

async Task Json2BinAndCopy_Dev_Async( string fileName, Conf conf, string mainName )
{
    await FlatcJsonToBin( fileName, conf, mainName );
    CopyBinToDevTargetDir( fileName, conf );
    await FlatcBinToJson( fileName, conf, mainName );
}


bool GroupSubXmlFiles( List<string> xmlFiles, out Dictionary<string, List<string>>? grouped, out List<string> scatterXmlFiles )
{
    grouped = null;
    scatterXmlFiles = new List<string>();

    for (int i = 0; i < xmlFiles.Count; ++i)
    {
        string xmlFile = xmlFiles[i];
        string fileName = Path.GetFileNameWithoutExtension( xmlFile );
        Match m = Regex.Match( fileName, @"(\w+)(\d+)\b" ); // ShopConfig1, ... ShopConfig5
        if (m != null && m.Success)
        {

            string main = m.Groups[1].Value; // 主表名 ShopConfig

            if (grouped == null)
            {
                grouped = new Dictionary<string, List<string>>();
                grouped[main] = new List<string> { xmlFile };
            }
            else if (!grouped.TryGetValue( main, out var list ))
            {
                grouped[main] = new List<string> { xmlFile };
            }
            else
            {
                grouped[main].Add( xmlFile );
            }

            // 获取主xml路径
            string matchstr = m.Groups[0].Value; // 匹配 ShopConfig1
            string mainXmlFile = xmlFile.Replace( matchstr, main );
            if (!grouped[main].Contains( mainXmlFile ))
            {
                grouped[main].Add( mainXmlFile );
                scatterXmlFiles.Remove( mainXmlFile );
            }
        }
        else
        {
            scatterXmlFiles.Add( xmlFile );
        }


    }

    return grouped != null && grouped.Count > 0;
}

string GetFbsGeneratedCsharpName( string fbsFile )
{
    string name = Path.GetFileNameWithoutExtension( fbsFile );
    return $"{name}_generated.cs";
}

string SaveFbSchema( FbSchema schema, string mainName, Conf conf )
{
    string path = $"{conf.GetFbsDir()}/{mainName}.fbs";

    File.WriteAllText( path, schema.ToString() );

    return path;

}

string? GetManualFinalFbsFilepath( string mainName, Conf conf )
{
    if (conf.FindManualFbsFile( mainName, out string? path ))
        return path;
    return null;
}

void CopyCsharpToUnityTargetDir( string fbsFile, Conf conf )
{
    try
    {
        string cs = GetFbsGeneratedCsharpName( fbsFile );
        string src = $"{conf.GetCsharpDir()}/{cs}";
        string dst = $"{conf.GetUnityCsharpDir()}/{cs}";
        DebugHelper.Print( $"[cs=>unity] copy `{cs}` to unity project `{conf.GetUnityCsharpDir()}` ..." );
        File.Copy( src, dst, true );
    }
    catch (Exception ex)
    {
        DebugHelper.PrintError( ex.Message );
    }
}
///<summary> 将fbin文件拷贝到Unity项目下，后缀名为*.bytes </summary>
void CopyBinToDevTargetDir( string fileName, Conf conf )
{
    try
    {
        string src = $"{conf.GetFbinDir()}/{fileName}.bin";
        string targetDir = conf.GetUnityDevFbinDir();
        string dst = $"{targetDir}/{fileName}.bytes";
        DebugHelper.Print( $"[bin->bytes] copy `{fileName}.bin` as `{fileName}.bytes` to directory: {targetDir}" );
        File.Copy( src, dst, true );

        // 如果同目录下存在同名xml文件，删除xml文件
        string xml = $"{conf.GetUnityDevFbinDir()}/{fileName}.xml";
        if (File.Exists( xml ))
        {
            DebugHelper.PrintDanger( $"DELETE {xml}..." );
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
        string targetDir = conf.GetUnityDeployFbinDir();
        string dst = $"{targetDir}/{fileName}.bytes";
        DebugHelper.Print( $"[bin->bytes] copy `{fileName}.bin` as `{fileName}.bytes` to directory: {targetDir}" );
        File.Copy( src, dst, true );
    }
    catch (Exception ex)
    {
        DebugHelper.PrintError( ex.Message );
    }
}


FbSchema GenerateSchema( XmlDocument doc, string xmlFile, string mainName, Conf conf )
{
    var gen = new SchemaGenerator( xmlFile, mainName );
    var schema = gen.GenerateSchema( doc );
    return gen.FbSchema;
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


string GetFbsFile( string dir, string name )
    => $"{dir}/{name}";


string get_json_to_fbin_cmd( string fileName, Conf conf, string? mainFbsName = null )
{
    string fbsName = string.IsNullOrEmpty( mainFbsName ) ? fileName : mainFbsName;
    return $"-o {conf.GetFbinDir()} -I {conf.GetFbsIncDir()} -b {conf.GetFbsDir()}/{fbsName}.fbs {conf.GetJsonDir()}/{fileName}.json";
}



async Task RunCmdAsync( string exe, string argLine )
{
    DebugHelper.Print( $"run `{exe} {argLine}` ({SourceInfo.File()}#{SourceInfo.Line()})" );
    var cts = new CancellationTokenSource( TimeSpan.FromSeconds( 30 ) );
    var psi = new ProcessStartInfo
    {
        FileName = exe,
        UseShellExecute = false,
        RedirectStandardOutput = true,
        RedirectStandardError = true,
        RedirectStandardInput = true,
        Arguments = argLine,

    };
    using var process = Process.Start( psi )!;

    process.ErrorDataReceived += ( s, e ) => DebugHelper.PrintError( e.Data );
    process.OutputDataReceived += ( s, e ) => DebugHelper.Print( e.Data );
    // 必须处理输出（即使你不关心内容）
    var outputTask = process.StandardOutput.ReadToEndAsync();
    var errorTask = process.StandardError.ReadToEndAsync();

    process.Start();

    try
    {
        await process.WaitForExitAsync( cts.Token );
        // 确保完全读取输出
        var buffer = await Task.WhenAll( outputTask, errorTask );
        if (buffer != null)
        {
            if (!string.IsNullOrEmpty( buffer[0] ))
                DebugHelper.Print( buffer[0] );
            if (!string.IsNullOrEmpty( buffer[1] ))
                DebugHelper.PrintError( buffer[1] );
        }
    }
    catch (OperationCanceledException)
    {
        // 超时处理
        if (!process.HasExited)
        {
            process.Kill( entireProcessTree: true ); // 关键参数！
            await Task.Delay( 1000 ); // 等待实际终止
        }
        DebugHelper.PrintError( "Process is timeout, now cancel!" );
    }



}


///<summary> 解析json文件，按照对应的fbs文件，生成对应的二进制数据文件 </summary>
async Task FlatcJsonToBin( string fileName, Conf conf, string? mainFbsName = null )
{
    DebugHelper.Print( $"[json->fbin]: gen from {fileName}.json to {fileName}.bin ... ({SourceInfo.File()}#{SourceInfo.Line()})" );

    string argLine = get_json_to_fbin_cmd( fileName, conf, mainFbsName );
    await RunCmdAsync( conf.GetFlatc(), argLine );
}

string get_fbin_to_json_cmd( string fileName, Conf conf, string? mainFbsName = null )
{
    string fbsName = string.IsNullOrEmpty( mainFbsName ) ? fileName : mainFbsName;
    return $"-o {conf.GetVerifyJsonDir()} -I {conf.GetFbsIncDir()} --natural-utf8 --raw-binary -t {conf.GetFbsDir()}/{fbsName}.fbs -- {conf.GetFbinDir()}/{fileName}.bin";
}

///<summary> 将fbin文件，按照对应fbs文件，生成为json文件 </summary>
async Task FlatcBinToJson( string fileName, Conf conf, string? mainFbsName = null )
{
    string argLine = get_fbin_to_json_cmd( fileName, conf, mainFbsName );
    await RunCmdAsync( conf.GetFlatc(), argLine );
}

///<summary> 生成fbs文件对应的C#代码 </summary>
string get_gen_cs_cmd( Conf conf, string fbsFile )
{
    if (conf.xml2fb!.isthin)
        return $"-o {conf.GetCsharpDir()} -I {conf.GetFbsIncDir()} --gen-onefile --csharp {fbsFile} ";
    else
        return $"-o {conf.GetCsharpDir()} -I {conf.GetFbsIncDir()} --gen-onefile --gen-compare --gen-object-api --gen-nullable --cs-gen-json-serializer --csharp {fbsFile} ";
}

///<summary> 遍历所有fbs文件，并生成对应的C#代码，然后拷贝到Unity工程目录下 </summary>
async Task FlatcGenFbsIncCsharpCode( Conf conf )
{
    if (conf.isStart)
        return;

    conf.isStart = true;
    var fbsList = Directory.GetFiles( conf.GetFbsIncDir(), "*.fbs" );
    Parallel.ForEach( fbsList, fbsFile =>
    {
        DebugHelper.Print( $"[Gen C#] process fbs file:{fbsFile}({SourceInfo.File()}#{SourceInfo.Line()})" );
        string argLine = get_gen_cs_cmd( conf, fbsFile );
        RunCmdAsync( conf.GetFlatc(), argLine ).Wait();

        CopyCsharpToUnityTargetDir( fbsFile, conf );
    } );
}

///<summary> 生成 fbs文件对应的csharp代码 </summary>
async Task FlatcGenCsharpCode( string fbsFile, Conf conf )
{
    DebugHelper.Print( $"[Gen C#] process :{fbsFile} ({SourceInfo.File()}#{SourceInfo.Line()})" );
    string argline = get_gen_cs_cmd( conf, fbsFile );
    await RunCmdAsync( conf.GetFlatc(), argline );

}

XmlDocument LoadXml( string xmlFile )
{
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
}

void SaveXmlToJson( XmlDocument doc, string jsonFile )
{
    //DebugHelper.Print( $"处理文件 {jsonFile}" );
    string name = Path.GetFileNameWithoutExtension( jsonFile );

    XmlNodeConverter converter = new MyXmlNodeConverter
    {
        OmitRootObject = false,
        WriteArrayAttribute = true,
        CurrentXmlDocName = name,
    };

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
         return StringUtils.Codize( m.Groups[1].Value );
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


