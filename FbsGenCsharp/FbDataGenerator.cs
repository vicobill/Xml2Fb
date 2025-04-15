using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace FbsGenCsharp;

class FbDataGenerator
{

    /*
     * xml:
     * <config>
     * <item name="xxx" title="bbb" id="100"/>
     * <skin title="xlx"> Test </skin>
     * </config>
     * 
     * fbs:
     * table Config {
     *      item: Item;
     *      skin: Skin;
     * }
     * table Item {
     *      name: string;
     *      title: string;
     *      id: uint;
     * }
     * table Skin {
     *      title: string;
     *      skin_value: string;
     * }
     * 
     * fb data:
     * var builder = new FlatBufferBuilder(builder);
     * 
     * 
     * var config = new Config();
     * 
     * Config.StartConfig(builder);
     * Config.AddName("xxx");
     * Config.AddTitle("bbb");
     * Config.AddId(100);
     */

    public string XmlFile { get; private set; }
    public string FbTableName { get; private set; }
    public FbDataGenerator(string xmlFile)
    {
        this.XmlFile = xmlFile;
        this.FbTableName = Path.GetFileNameWithoutExtension( XmlFile );
    }

    void GenerateFbSerialize(string xmlFile )
    {

        using FileStream stream = File.OpenRead( xmlFile );
        using var reader = XmlReader.Create( stream, new XmlReaderSettings
        {
            IgnoreComments = true,
            IgnoreProcessingInstructions = true,
            IgnoreWhitespace = true,
            Async = true

        } );
        XmlDocument doc = new XmlDocument();
        doc.Load( reader );

        XmlElement? root = doc.DocumentElement;
        
        ProcessElement( root );

    }

    void ProcessElement( XmlElement root)
    {
        foreach(var e in root.Attributes)
        {
            /*
             Config.Add{AttributeName}(fb)
             */
        }
    }

}

