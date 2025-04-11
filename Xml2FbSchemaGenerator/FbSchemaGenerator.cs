using Google.FlatBuffers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;

namespace Xml2FbSchemaGenerator;

/**
 * XML根节点作为命名空间；
 * XML文件名作为类型名称；
 * XML每个节点是一个Table; 
 * XML节点的属性，是Table的字段；通过解析属性的值，获取字段的类型 
 * 
 * 1. 如果siblings 名称都相同，则合并属性（字段）
 * 
 */

class FbSchemaGenerator
{
    public string GenerateSchema( string xmlPath )
    {
        var xmlDocument = new XmlDocument();
        xmlDocument.Load( xmlPath );
        string tableName = Path.GetFileNameWithoutExtension( xmlPath );

        var rootElement = xmlDocument.DocumentElement;
        var schemaBuilder = new SchemaBuilder();

        ProcessElement( rootElement, schemaBuilder, rootElement.Name, null );

        return schemaBuilder.ToString();
    }



    void ProcessElement( XmlElement node, SchemaBuilder builder, string typeName, TableDefinition parentTable )
    {
        string tableName = typeName.Capitalize();
        if (parentTable != null)
        {
            tableName = $"{parentTable.Name}_{tableName}";
        }

        if (!builder.TryPeek( tableName, out TableDefinition table ))
        {
            table = new TableDefinition( tableName );
        }

        // 处理自身节点属性，作为FbTable的字段形式
        foreach (XmlAttribute attr in node.Attributes)
        {
            AddField( attr.Name, GetFlatbufferType( attr.Value ), table );
        }



        // 处理子节点，按节点名称分组
        // 每个组，作为FbTable的每个数组字段


        // 节点名称-节点列表
        Dictionary<string, List<XmlElement>> xmlElemGroups = new Dictionary<string, List<XmlElement>>();
        foreach (XmlElement child in node.ChildNodes)
        {
            if (!xmlElemGroups.TryGetValue( child.Name, out var xmlElemList ))
            {
                xmlElemList = new List<XmlElement>();
                xmlElemList.Add( child );
                xmlElemGroups[child.Name] = xmlElemList;
            }
            else
            {
                xmlElemList.Add( child );
            }
        }

        /* 添加数组字段 */
        foreach (var kv in xmlElemGroups)
        {
            string xmlNodeName = kv.Key;
            if (kv.Value.Count > 0)
            {
                string subTableName = $"{tableName}_{xmlNodeName.Capitalize()}";
                table.AddVectorField( xmlNodeName, subTableName );
            }
        }

        /* 递归处理子节点 */
        foreach (var kv in xmlElemGroups)
        {
            foreach (XmlElement subNode in kv.Value)
            {
                ProcessElement( subNode, builder, kv.Key, table );
            }

        }


        builder.AddTable( table );
    }


    void AddField( string name, string type, TableDefinition table )
    {

        table.AddField( name, type );
    }

    internal static string GetFlatbufferType( string xmlValue )
    {
        if (uint.TryParse( xmlValue, out _ ))
            return "uint";
        if (int.TryParse( xmlValue, out _ ))
            return "int";
        if (float.TryParse( xmlValue, out _ ))
            return "float";
        if (bool.TryParse( xmlValue, out _ ))
            return "bool";
        return "string";
    }

    internal static bool IsPrimitiveType( string typeName )
    {
        return typeName == "string" || typeName == "int" ||
            typeName == "uint" || typeName == "float" ||
            typeName == "bool";
    }
}
