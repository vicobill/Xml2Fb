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
//string confFile = "E:\\project\\my-tools\\Xml2FbSchemaGenerator\\Assets\\xml2fb.json";
string confFile = "";
if (args.Length > 0)
{
    confFile = args[0];
}
Conf? conf = LoadConf( confFile );
if (conf == null) return;

Directory.SetCurrentDirectory( conf.workdir );

IEnumerable<string>? xmlFiles = Directory.EnumerateFiles( conf.GetXmlDir(), "*.xml", new EnumerationOptions { RecurseSubdirectories = conf.isrec } );



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


Task FlatcGenCsharpCode( string fbsFile, Conf conf )
{
    //DebugHelper.PrintDebug( $"[GenC#] 处理文件:{fbsFile}" );
    Process ps = new Process();
    ps.StartInfo.FileName = conf.GetFlatc();
    ps.StartInfo.RedirectStandardError = true;
    ps.StartInfo.RedirectStandardOutput = true;
    ps.StartInfo.Arguments = $"-o {conf.GetCsharpDir()} -I {conf.GetFbsIncDir()} --gen-onefile --gen-compare --gen-object-api --gen-nullable --csharp {fbsFile} ";
    ps.OutputDataReceived += ( s, e ) => DebugHelper.PrintWarning( $"[GenC#] {fbsFile} {e.Data}" );
    ps.ErrorDataReceived += ( s, e ) => DebugHelper.PrintError( $"[GenC#] {fbsFile} {e.Data}" );
    ps.Start();
    return ps.WaitForExitAsync();


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
    //string json2 = JsonConvert.SerializeXmlNode( doc, Newtonsoft.Json.Formatting.None, false );

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
        DebugHelper.PrintError(ex.StackTrace);
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
    }


    public class NodeAsArray
    {
        public string xml;
        public List<string> node_paths;
    }

    public List<NodeAsArray> node_as_array;

    public string GetJsonDir()
    {
        if (Path.IsPathRooted( jsondir ))
            return jsondir;
        return $"{workdir}/{jsondir}";
    }
    public string GetFbsDir()
    {
        if (Path.IsPathRooted( fbsdir ))
            return fbsdir;
        return $"{workdir}/{fbsdir}";
    }
    public string GetFbsBinDir()
    {
        if (Path.IsPathRooted( fbsincdir ))
            return fbsincdir;
        return $"{workdir}/{fbindir}";
    }
    public string GetXmlDir()
    {
        if (Path.IsPathRooted( xmldir ))
            return xmldir;
        return $"{workdir}/{xmldir}";
    }
    public string GetCsharpDir()
    {
        if (Path.IsPathRooted( csharpdir ))
            return csharpdir;
        return $"{workdir}/{csharpdir}";
    }
    public string GetFbsIncDir()
    {
        if (Path.IsPathRooted( fbsincdir ))
            return fbsincdir;
        return $"{workdir}/{fbsincdir}";
    }
    public string GetFlatc()
    {
        if (Path.IsPathRooted( flatc ))
            return flatc;
        return $"{workdir}/{flatc}";
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


