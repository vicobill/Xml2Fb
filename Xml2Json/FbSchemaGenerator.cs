using System.Xml;
using Xml2Fb.Shared;
using Xml2Fb.Shared.FbDef;

namespace Xml2Fb;

/**
 * XML根节点作为命名空间；
 * XML文件名作为类型名称；
 * XML每个节点是一个Table; 
 * XML节点的属性，是Table的字段；通过解析属性的值，获取字段的类型 
 * 
 * 1. 如果siblings 名称都相同，则合并属性（字段）
 * 
 */

public class FbSchemaGenerator
{
    string _xmlFile;
    string _fbsFile;
    public string FbsFile => _fbsFile;
    public string FbsTableName { get; private set; }

    public FbSchemaGenerator( string xmlFile )
    {
        _xmlFile = xmlFile;
        FbsTableName = Path.GetFileNameWithoutExtension(_xmlFile);
        _fbsFile = Path.GetFileName( xmlFile ).Replace( ".xml", ".fbs" );
    }


    public string GenerateSchema(XmlDocument doc)
    {
        string tableName = Path.GetFileNameWithoutExtension( _xmlFile );

        var rootElement = doc.DocumentElement;
        var schemaBuilder = new SchemaBuilder();
        schemaBuilder.RootType = FbsTableName;
        ProcessElement( rootElement, schemaBuilder, FbsTableName, null );

        return schemaBuilder.ToString();
    }



    void ProcessElement( XmlNode node, SchemaBuilder builder, string typeName, TableDefinition parentTable )
    {
        string tableName = typeName.Capitalize();
        if (parentTable != null /*&& parentTable.Name != FbsTableName*/)
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
            table.AddField( attr.Name, FbHelper.GetFbTypeFromXmlValue( attr.Value, attr.Name ) );
        }



        // 处理子节点，按节点名称分组
        // 每个组，作为FbTable的每个数组字段

        if ( node.ChildNodes.Count == 1 && node.FirstChild.NodeType == XmlNodeType.Text) // <id> 1234 </id>
        {
            table.AddField( $"{typeName}_value", FbHelper.GetFbTypeFromXmlValue( node.FirstChild.Value, node.FirstChild.Name ) );
            builder.AddTable( table );
            return;
        }


        // 节点名称-节点列表
        Dictionary<string, List<XmlNode>> xmlElemGroups = new Dictionary<string, List<XmlNode>>();
        foreach (XmlNode child in node.ChildNodes)
        {
            if (!xmlElemGroups.TryGetValue( child.Name, out var xmlElemList ))
            {
                xmlElemList = new List<XmlNode>();
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
            List<XmlNode> xmlNodes = kv.Value;
            string subTableName = /* tableName == FbsTableName ? $"{xmlNodeName.Capitalize()}" :*/ $"{tableName}_{xmlNodeName.Capitalize()}";
            if (xmlNodes.Count > 1)
            {
                table.AddVectorField( xmlNodeName, subTableName );
            }
            else
            {
                if (IsNodeAsArray( xmlNodes.First()))
                {
                    table.AddVectorField( xmlNodeName, subTableName );
                }
                else
                {
                    table.AddField( xmlNodeName, subTableName );
                }
            }
        }

        /* 递归处理子节点 */
        foreach (var kv in xmlElemGroups)
        {
            foreach (XmlNode subNode in kv.Value)
            {
                ProcessElement( subNode, builder, kv.Key, table );
            }

        }


        builder.AddTable( table );
    }

    static bool IsNodeAsArray(XmlNode node)
    {
        string path = $"{node.ParentNode.LocalName}/{node.LocalName}";
        return IsNodeAsArray( path );
    }

    public static bool IsNodeAsArray( string path)
    {
        string lower = path.ToLower();
        if (lower.EndsWith( "item" ))
            return true;
        /* ActivityConfig 特殊处理 */
        switch (lower)
        {
            case "boxitem/rare": // ActivityConfig/mysterybox/boxitem/rare
            case "previewgroup/preview":// ActivityConfig/shopactivititys/activity/
            case "time/previewgroup":
            case "item/skin":
            case "item/rewards":
                return true;
        }

        return false;
    }

}

