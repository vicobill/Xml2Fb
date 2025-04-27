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


// 加载配置文件
//#if DEBUG
//string confFile = "E:\\project\\my-tools\\Xml2FbSchemaGenerator\\Assets\\xml2fb.json";
//#else
string confFile = "";
//#endif
if (args.Length > 0 && string.IsNullOrEmpty( confFile ))
{
    confFile = args[0];
}
Conf? conf = LoadConf( confFile );
if (conf == null) return;

Directory.SetCurrentDirectory( conf.workdir );

IEnumerable<string>? xmlFiles = Directory.EnumerateFiles( conf.GetXmlDir(), "*.xml", new EnumerationOptions { RecurseSubdirectories = conf.isrec } );

bool _is_start = false;

foreach (var xmlFile in xmlFiles)
{
    //string xmlFile = $"{conf.GetXmlDir()}/BlindBoxConfig.xml";
    string fileName = Path.GetFileNameWithoutExtension( xmlFile );
    string jsonFile = Path.Combine( conf.GetJsonDir(), $"{fileName}.json" );
    string fbsFile = Path.Combine( conf.GetFbsDir(), $"{fileName}.fbs" );


    await Task.Run( () =>
    {
        DebugHelper.Print( $"处理Xml文件 {fileName}.xml" );
        XmlDocument doc = LoadXml( xmlFile );
        GenFbSchema( doc, xmlFile, conf );
        SaveJsonAsync( doc, jsonFile );
    } ).ConfigureAwait( false );

    await FlatcGenFbsIncCsharpCode( conf );

    await Task.Run( () =>
    {
        FlatcGenCsharpCode( fbsFile, conf );
        FlatcJsonToBin( fileName, conf );
        FlatcBinToJson( fileName, conf );
        DebugHelper.PrintOk( $"处理Xml文件 {fileName}.xml 结束！" );
    } ).ConfigureAwait( false );



}

DebugHelper.Print( "DONE!" );

void GenFbSchema( XmlDocument doc, string xmlFile, Conf conf )
{
    //DebugHelper.Print( $"[GenFbs] 开始生成FBS文件： {xmlFile}" );
    var schemaGenerator = new FbSchemaGenerator( xmlFile );
    var schema = schemaGenerator.GenerateSchema( doc );
    string fbs = GetFbsFile( conf.GetFbsDir(), schemaGenerator.FbsFile );
    //DebugHelper.PrintDebug( $"[GenFbs] 写入FBS文件 {fbs} " );
    File.WriteAllText( fbs, schema );
}


string GetFbsFile( string dir, string name ) => $"{dir}/{name}";


void FlatcJsonToBin( string fileName, Conf conf )
{
    Process process = new Process();
    process.StartInfo.FileName = conf.GetFlatc();
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.CreateNoWindow = false;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.Arguments = $"-o {conf.GetFbsBinDir()} -I {conf.GetFbsIncDir()} -b {conf.GetFbsDir()}/{fileName}.fbs {conf.GetJsonDir()}/{fileName}.json";

    process.OutputDataReceived += ( s, e ) => DebugHelper.PrintWarning( $"[Json2Bin] {fileName} {e.Data}" );
    process.ErrorDataReceived += ( s, e ) => DebugHelper.PrintError( $"[Json2Bin] {fileName} {e.Data}" );
    //DebugHelper.Print( $"[Json2Bin] 执行flatc json2bin {fileName}.json" );
    process.Start();
    //return process.WaitForExitAsync();

}



void FlatcBinToJson( string fileName, Conf conf )
{
    Process process = new Process();
    process.StartInfo.FileName = conf.GetFlatc();
    process.StartInfo.RedirectStandardError = true;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.CreateNoWindow = false;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.Arguments = $"-o {conf.GetVerifyJsonDir()} -I {conf.GetFbsIncDir()} --natural-utf8 --raw-binary -t {conf.GetFbsDir()}/{fileName}.fbs -- {conf.GetFbsBinDir()}/{fileName}.bin";

    process.OutputDataReceived += ( s, e ) => DebugHelper.PrintWarning( $"[Bin2Json] {fileName} {e.Data}" );
    process.ErrorDataReceived += ( s, e ) => DebugHelper.PrintError( $"[Bin2Json] {fileName} {e.Data}" );
    //DebugHelper.Print( $"[Json2Bin] 执行flatc json2bin {fileName}.json" );
    process.Start();
}

string get_gen_cs_cmd( Conf conf, string fbsFile )
{
    if (conf.isthin)
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

        DebugHelper.PrintDebug( $"[GenC#] 处理文件:{fbsFile}" );
        Process ps = new Process();
        ps.StartInfo.FileName = conf.GetFlatc();
        ps.StartInfo.RedirectStandardError = true;
        ps.StartInfo.RedirectStandardOutput = true;
        ps.StartInfo.UseShellExecute = false;
        ps.StartInfo.Arguments = get_gen_cs_cmd( conf, fbsFile );

        ps.OutputDataReceived += ( s, e ) => DebugHelper.PrintWarning( $"[GenC#] {fbsFile} {e.Data}" );
        ps.ErrorDataReceived += ( s, e ) => DebugHelper.PrintError( $"[GenC#] {fbsFile} {e.Data}" );
        ps.Start();
        string stdout = await ps.StandardOutput.ReadToEndAsync();
        if (!string.IsNullOrEmpty( stdout ))
            DebugHelper.PrintWarning( stdout );
        string stderr = await ps.StandardError.ReadToEndAsync();
        if (!string.IsNullOrEmpty( stderr ))
            DebugHelper.PrintError( stderr );
        await ps.WaitForExitAsync();
    }
}

async Task FlatcGenCsharpCode( string fbsFile, Conf conf )
{
    DebugHelper.PrintDebug( $"[GenC#] 处理文件:{fbsFile}" );
    Process ps = new Process();
    ps.StartInfo.FileName = conf.GetFlatc();
    ps.StartInfo.RedirectStandardError = true;
    ps.StartInfo.RedirectStandardOutput = true;
    ps.StartInfo.UseShellExecute = false;
    ps.StartInfo.Arguments = get_gen_cs_cmd( conf, fbsFile );

    ps.OutputDataReceived += ( s, e ) => DebugHelper.PrintWarning( $"[GenC#] {fbsFile} {e.Data}" );
    ps.ErrorDataReceived += ( s, e ) => DebugHelper.PrintError( $"[GenC#] {fbsFile} {e.Data}" );
    ps.Start();
    string stdout = await ps.StandardOutput.ReadToEndAsync();
    if (!string.IsNullOrEmpty( stdout ))
        DebugHelper.PrintWarning( stdout );
    string stderr = await ps.StandardError.ReadToEndAsync();
    if (!string.IsNullOrEmpty( stderr ))
        DebugHelper.PrintError( stderr );
    await ps.WaitForExitAsync();


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



public class Conf
{
    public string workdir;
    public string xmldir;
    public bool isrec;
    public string pkgname;
    public string fbsdir;
    public string jsondir;
    public string fbsincdir;
    public string fbindir;
    public string flatc;
    public string csharpdir;
    public bool isthin;

    [JsonConstructor]
    public Conf()
    {
        workdir = Directory.GetCurrentDirectory();
        xmldir = "xml";
        fbsdir = "gen/fbs";
        jsondir = "gen/json";
        fbsincdir = "fbs-inc";
        fbindir = "gen/bytes";
        flatc = "bin/flatc.exe";
        csharpdir = "gen/cs";
        pkgname = "Config.FBin";
        isthin = true;
    }

    public List<NodeAsArray> node_as_array;
    public List<TablePreset> table_preset;

    public class NodeAsArray
    {
        public string xml;
        public List<string> node_paths;
    }

    public class TablePreset
    {
        public string table;
        public string preset;
    }

    public static string GetAbsDir( string path )
    {
        if (path.StartsWith( ".." ))
        {
            string curdir = Directory.GetCurrentDirectory();
            int i = curdir.LastIndexOf( Path.DirectorySeparatorChar );
            string p = curdir.Substring( 0, i );
            path = $"{p}{path.Substring( 2 )}";
        }
        else
        if (path.StartsWith( "." ))
            path = $"{Directory.GetCurrentDirectory()}{path.Substring( 1 )}";


        return path;
    }

    public string WorkingDir => GetAbsDir( workdir );


    public string GetJsonDir()
    {
        if (Path.IsPathRooted( jsondir ))
            return jsondir;
        return $"{WorkingDir}/{jsondir}";
    }
    public string GetFbsDir()
    {
        if (Path.IsPathRooted( fbsdir ))
            return fbsdir;
        return $"{WorkingDir}/{fbsdir}";
    }
    public string GetFbsBinDir()
    {
        if (Path.IsPathRooted( fbsincdir ))
            return fbsincdir;
        return $"{WorkingDir}/{fbindir}";
    }
    public string GetXmlDir()
    {
        if (Path.IsPathRooted( xmldir ))
            return xmldir;
        return $"{WorkingDir}/{xmldir}";
    }
    public string GetCsharpDir()
    {
        if (Path.IsPathRooted( csharpdir ))
            return csharpdir;
        return $"{WorkingDir}/{csharpdir}";
    }
    public string GetFbsIncDir()
    {
        if (Path.IsPathRooted( fbsincdir ))
            return fbsincdir;
        return $"{WorkingDir}/{fbsincdir}";
    }
    public string GetFlatc()
    {
        if (Path.IsPathRooted( flatc ))
            return flatc;
        return $"{WorkingDir}/{flatc}";
    }
    public string GetVerifyJsonDir()
    {
        return $"{GetJsonDir()}/verify";
    }

    public static Conf Instance { get; set; }

    public void EnsureDir()
    {
        string jsonDir = GetJsonDir();
        if (!Directory.Exists( jsonDir ))
            Directory.CreateDirectory( jsonDir );

        string xmlDir = GetXmlDir();
        if (!Directory.Exists( xmlDir ))
            Directory.CreateDirectory( xmlDir );

        string fbsBinDir = GetFbsBinDir();
        if (!Directory.Exists( fbsBinDir ))
            Directory.CreateDirectory( fbsBinDir );

        string fbsDir = GetFbsDir();
        if (!Directory.Exists( fbsDir ))
            Directory.CreateDirectory( fbsDir );

        string csDir = GetCsharpDir();
        if (!Directory.Exists( csDir ))
            Directory.CreateDirectory( csDir );

        string vjsonDir = GetVerifyJsonDir();
        if (!Directory.Exists( vjsonDir ))
            Directory.CreateDirectory( vjsonDir );
    }

}


