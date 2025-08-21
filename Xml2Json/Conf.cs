using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Xml2Fb;

public class Conf
{
    public Xml2FbConf? xml2fb;
    public UnityConf? unity;


    public class Xml2FbConf
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
        /// <summary>
        /// Config Beta, Config Local,Config Version等
        /// </summary>
        public List<string>? XmlDirNames;
        public List<NodeAsArray>? node_as_array;
        public List<TablePreset>? table_preset;
        public List<GroupXml>? group_xml;


        public static Xml2FbConf GetDefault()
        {
            Xml2FbConf c = new Xml2FbConf();
            c.workdir = Directory.GetCurrentDirectory();
            c.xmldir = "xml";
            c.fbsdir = "gen/fbs";
            c.jsondir = "gen/json";
            c.fbsincdir = "fbs-inc";
            c.fbindir = "gen/bytes";
            c.flatc = "bin/flatc.exe";
            c.csharpdir = "gen/cs";
            c.pkgname = "Config.FBin";
            c.isthin = true;

            return c;
        }
    }
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

    public class GroupXml
    {
        public string main;
        public List<string> sub_names;
        public string? use_custom_fbs;
    }

    public class UnityConf
    {
        public string? basedir;
        public List<string>? whitelist;
        public DevelopConfig develop;
        public DeployConfig deploy;


        public class DevelopConfig
        {
            public string csharpdir;
            public string fbindir;
        }

        public class DeployConfig
        {
            public string xmldir;
            public string fbindir;
        }
    }

    [JsonConstructor]

    public Conf( bool isDeployMode )
    {
        xml2fb = Xml2FbConf.GetDefault();
    }


    public bool isDeployMode { get; set; }
    public bool isStart { get; set; }
    public int xmlDirNameIndex { get; set; }

    public bool InWhitelist( string filename )
    {
        /* 白名单为空，则所有都是白名单 */
        if (unity.whitelist == null || unity.whitelist.Count == 0)
            return true;

        string name = Path.GetFileNameWithoutExtension( filename );
        foreach (string e in unity.whitelist)
        {
            if (e.Equals( name, StringComparison.OrdinalIgnoreCase ))
                return true;
        }
        return false;

    }

    public List<NodeAsArray>? nodeAsArrays => xml2fb?.node_as_array;


    public static string GetAbsDir( string? path )
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

    public string WorkingDir => GetAbsDir( xml2fb.workdir );

    public string GetJsonDir()
    {
        string? dir = xml2fb?.jsondir;
        if (Path.IsPathRooted( dir ))
            return dir;
        return $"{WorkingDir}/{dir}";
    }
    public string GetFbsDir()
    {
        string? dir = xml2fb?.fbsdir;
        if (Path.IsPathRooted( dir ))
            return dir;
        return $"{WorkingDir}/{dir}";
    }
    public string GetFbinDir()
    {
        string? dir = xml2fb?.fbindir;
        if (Path.IsPathRooted( dir ))
            return dir;
        return $"{WorkingDir}/{dir}";
    }

    public string GetXmlDir()
    {
        if (xml2fb.XmlDirNames == null || xml2fb.XmlDirNames.Count == 0
            || xmlDirNameIndex >= 0)
        {

            string? dir = xml2fb?.xmldir;
            if (Path.IsPathRooted( dir ))
                return dir;
            return $"{WorkingDir}/{dir}";
        }
        else
        {
            string xml = xml2fb.XmlDirNames[xmlDirNameIndex];
            string? dir = xml2fb?.xmldir + $"/{xml}";
            if (Path.IsPathRooted( dir ))
                return dir;
            return $"{WorkingDir}/{dir}";
        }
    }

    public string GetCsharpDir()
    {
        string? dir = xml2fb?.csharpdir;
        if (Path.IsPathRooted( dir ))
            return dir;
        return $"{WorkingDir}/{dir}";
    }
    public string GetFbsIncDir()
    {
        string? dir = xml2fb?.fbsincdir;
        if (Path.IsPathRooted( dir ))
            return dir;
        return $"{WorkingDir}/{dir}";
    }
    public string GetFlatc()
    {
        string? dir = xml2fb?.flatc;
        if (Path.IsPathRooted( dir ))
            return dir;
        return $"{WorkingDir}/{dir}";
    }
    public string GetVerifyJsonDir()
    {
        return $"{GetJsonDir()}/verify";
    }


    public static Conf Instance { get; set; }


    public string UnityWorkingDir => GetAbsDir( unity?.basedir );

    public string GetUnityDevFbinDir()
    {
        string? dir = unity?.develop.fbindir;
        if (Path.IsPathRooted( dir ))
            return dir;
        return $"{UnityWorkingDir}/{dir}";
    }
    public string GetUnityDeployFbinDir()
    {
        string? dir = unity?.deploy?.fbindir;
        if (Path.IsPathRooted( dir ))
            return dir;
        return $"{UnityWorkingDir}/{dir}";
    }
    public string GetUnityCsharpDir()
    {
        string? dir = unity?.develop?.csharpdir;
        if (Path.IsPathRooted( dir ))
            return dir;
        return $"{UnityWorkingDir}/{dir}";
    }
    public string GetUnityDeployXmlDir()
    {
        string? dir = unity?.deploy?.xmldir;
        if (Path.IsPathRooted( dir ))
            return dir;
        return $"{UnityWorkingDir}/{dir}";
    }





    public void EnsureDir()
    {
        EnsureXml2FbDirs();
        EnsureUnityDirs();
    }
    static void EnsureDir( string dir )
    {
        if (!Directory.Exists( dir ))
            Directory.CreateDirectory( dir );
    }

    void EnsureUnityDirs()
    {
        string csharpDir = GetUnityCsharpDir();
        EnsureDir( csharpDir );
        string devFbinDir = GetUnityDevFbinDir();
        EnsureDir( devFbinDir );
        string xmlDir = GetUnityDeployXmlDir();
        EnsureDir( xmlDir );
        string deployFbinDir = GetUnityDeployFbinDir();
        EnsureDir( deployFbinDir );
    }

    void EnsureXml2FbDirs()
    {
        string jsonDir = GetJsonDir();
        EnsureDir( jsonDir );



        string xmlDir = GetXmlDir();
        EnsureDir( xmlDir );


        string fbsBinDir = GetFbinDir();
        EnsureDir( fbsBinDir );


        string fbsDir = GetFbsDir();
        EnsureDir( fbsDir );


        string csDir = GetCsharpDir();
        EnsureDir( csDir );


        string vjsonDir = GetVerifyJsonDir();
        EnsureDir( vjsonDir );

    }

    internal bool FindManualFbsFile( string mainName, out string? path )
    {
        path = null;
        GroupXml? has = xml2fb?.group_xml?.Find( x => x.main == mainName );
        if (has != null && !string.IsNullOrEmpty( has.use_custom_fbs ))
        {
            path = has.use_custom_fbs;
            return true;
        }
        return false;

    }
}
