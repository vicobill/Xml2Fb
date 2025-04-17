using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xml2Fb.Shared;
using Xml2Fb.Shared.FbDef;

namespace Xml2Fb.Xml2FbData
{
    /// <summary>
    /// 生成C#代码，对Xml数据进行FlatBuffers式序列化，保存为文件。
    /// 生成的C#代码放入到另外的C#项目中，执行以对所有的XML文件进行序列化。
    /// </summary>
    class Xml2FbDataGenerator
    {
        public string FbsTableName { get; private set; }
        public string _xmlFile;
        public Xml2FbDataGenerator(string xmlFile)
        {
            FbsTableName = Path.GetFileNameWithoutExtension( xmlFile ).Codize();
            _xmlFile = xmlFile;
        }

        public string Generate()
        {

            using FileStream stream = File.OpenRead( _xmlFile );
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
            CsharpBuilder csharpBuilder = new CsharpBuilder( FbsTableName );
            csharpBuilder.BeginRootTable();
            ProcessElement( root, csharpBuilder, csharpBuilder.RootTableName );
            csharpBuilder.EndRootTable();

            return csharpBuilder.GenerateSerializerCode();
        }

        void ProcessElement( XmlNode node, CsharpBuilder builder, string tableName )
        {
            bool isRootTable = false;
            if (tableName == builder.RootTableName)
            {
                isRootTable = true;
            }
            if (!isRootTable)
            {
                builder.BeginTable( node.Name,tableName );
            }
            // 属性转成 Fb Fields
            builder.BeginAddFields(tableName);
            foreach (XmlAttribute attr in node.Attributes)
            {
                builder.AddField( attr.Name, attr.Value);
            }
            

            // 形如 <id> 123 </id> 的节点，值不作为子节点而是作为属性
            if (XmlHelper.IsTextNode( node, out string nodeValue ))
            {
                builder.AddField( $"{tableName.ToFieldName()}_value", nodeValue );
                builder.EndAddFields( tableName );
                builder.EndTable( node.Name,tableName );
                return;
            }
            builder.EndAddFields( tableName );
            // 按Node名称进行分组
            XmlHelper.GroupSubNodes( node, out Dictionary<string, List<XmlNode>> groups );

            foreach (var kv in groups)
            {
                string xmlNodeName = kv.Key;
                List<XmlNode> xmlNodes = kv.Value;
                string subTableName = $"{tableName}_{xmlNodeName.Capitalize()}";
                if (xmlNodes.Count > 1)
                {
                    //table.AddVectorField( xmlNodeName, subTableName );

                    builder.BeginVector( xmlNodeName, subTableName );
                    foreach (var subnode in xmlNodes)
                    {
                        builder.VectorItem( xmlNodeName, subTableName);
                        ProcessElement( subnode, builder, subTableName );
                        builder.EndVector(xmlNodeName, subTableName );
                    }
                }
                else
                {
                    //FbFieldDef field = new FbFieldDef( xmlNodeName );
                    //field.FieldType = FbHelper.GetFbTypeFromXmlValue( xmlNodes[0].Value );
                    //field.FieldValue = xmlNodes[0].Value;
                    //field.ParentTableName = table.Name;
                    //table.AddField( field );
                    var subnode = xmlNodes[0];
                    ProcessElement( subnode, builder, xmlNodeName);
                }
            }
           

            builder.EndTable(node.Name, tableName );
        }

        static FbFieldDef MakeFbFieldDef( XmlAttribute xmlAttribute )
        {
            var field = new FbFieldDef(xmlAttribute.Name);
            field.FieldValue = xmlAttribute.Value;
            field.FieldType = FbHelper.GetFbTypeFromXmlValue( xmlAttribute.Value,xmlAttribute.Name );
            return field;
        }
    }
}
