using System.Text.RegularExpressions;
using System.Xml;
using Xml2Fb.FbGrammar;
using Xml2Fb.Shared;
using Xml2Fb.Shared.FbDef;

namespace Xml2Fb;


/// <summary>
/// 根据XML文件，生成 Fbs Schema 文件
/// 在生成fbs以及合规的csharp代码时，需要处理名称转换：
/// 1. 字段名称可能是C#关键字，需要处理；
/// 2. 转成C#代码可用的名称，如'-' 需要转成 '_'
///
/// 在Xml转fbs过程中：
/// 1. 
/// </summary>
public class SchemaGenerator
{
    string _xmlFilePath;
    string _fbsFileName;
    public string FbsFileName => _fbsFileName;
    public string FbsRootTableName { get; private set; }

    public SchemaGenerator( string xmlFilePath )
    {
        _xmlFilePath = xmlFilePath;
        string name = Path.GetFileNameWithoutExtension( _xmlFilePath );
        _fbsFileName = $"{name}.fbs";

        FbsRootTableName = name.ToTableName();

        CurrentXmlDocName = name;
    }

    /// <summary>
    /// 解析xml文件，生成fbs文件内容
    /// </summary>
    /// <param name="doc">xml文件</param>
    /// <returns></returns>
    public string GenerateSchema( XmlDocument doc )
    {
        var rootElement = doc.DocumentElement;
        SchemaBuilder builder = new SchemaBuilder( FbsRootTableName );
        ProcessElement( rootElement, builder, FbsRootTableName, null );

        return builder.ToString();
    }

    void ProcessElement( XmlNode node, SchemaBuilder builder, string curTableName, FbTypeDecl? parentTable )
    {
        string tableName = curTableName.ToTableName();
        if (parentTable != null)
        {
            tableName = $"{parentTable.typeName}_{tableName}"; // 衔接父表名称
        }

        bool isPresetTable = IsTablePreset( tableName, out var presetTableName );

        if (isPresetTable)
        {
            tableName = presetTableName;
        }

        // 如果尚无此名称的表定义，新增表
        if (!builder.TryPeek( tableName, out FbTypeDecl? table ))
        {
            table = new FbTypeDecl
            {
                isStruct = false,
                typeName = tableName
            };
        }


        if (node.Attributes == null)
        {
            DebugHelper.PrintWarning( $"{_xmlFilePath} has error arround node: `{node.NextSibling.OuterXml}` " );
            return;
        }
        // 处理自身节点属性，作为FbTable的字段形式
        foreach (XmlAttribute attr in node.Attributes)
        {
            // 所有 XmlAttribute 都前缀 attr_
            table!.WithField(
                fieldName: $"attr_{attr.Name}".ToFieldName(),
                typeName: FbHelper.GetFbTypeFromXmlValue( attr.Value, attr.Name )
            );
        }



        if (node.ChildNodes.Count == 1 && node.FirstChild.NodeType == XmlNodeType.Text) // <id> 1234 </id>
        {
            // 普通形如 <id> 1234 </id> 的xml节点，作为特殊字段取值
            table!.WithField(
                $"{curTableName}_value".ToFieldName(),
                FbHelper.GetFbTypeFromXmlValue( node.FirstChild.Value, node.FirstChild.Name )
            );
            builder.AddTable( table );
            return;
        }


        // 节点名称-节点列表
        XmlHelper.GroupSubNodes( node, out var xmlElemGroups );

        List<string> presetTableXmlNodes = new List<string>();

        /* 添加数组字段 */
        foreach (var kv in xmlElemGroups)
        {
            string xmlNodeName = kv.Key;
            List<XmlNode> xmlNodes = kv.Value;
            // 当前表为父表，子节点为子表
            string subTableName = $"{tableName}_{xmlNodeName.ToTableName()}";

            if (IsTablePreset( subTableName, out string presetSubTableName ))
            {
                subTableName = presetSubTableName;
                presetTableXmlNodes.Add( xmlNodeName );
            }

            if (xmlNodes.Count > 1)
            {
                if (AllIsTextNodes( xmlNodes ))
                {
                    AddVectorField( xmlNodeName, "string", table! );
                }
                else
                {
                    AddVectorField(xmlNodeName, subTableName, table! );
                }
            }
            else
            {
                if (IsNodeAsArray( xmlNodes.First() ))
                {
                    AddVectorField( xmlNodeName, subTableName, table! );
                }
                else
                {
                    if (XmlHelper.IsTextNode( xmlNodes[0], out XmlNode child ))
                    {
                        table!.WithField( xmlNodeName, "string" );
                    }
                    else
                    {

                        table!.WithField( xmlNodeName, subTableName );
                    }
                }
            }
        }

        /* 递归处理子节点 */
        foreach (KeyValuePair<string, List<XmlNode>> kv in xmlElemGroups)
        {
            if (kv.Value.Count == 1 && XmlHelper.IsTextNode( kv.Value.First(), out var _ ))
                continue;

            if (presetTableXmlNodes.IndexOf( kv.Key ) >= 0)
                continue;

            foreach (XmlNode subNode in kv.Value)
            {
                ProcessElement( subNode, builder, kv.Key, table );
            }

        }


        builder.AddTable( table! );
    }

    
    bool IsTablePreset( string tableName, out string presetTableName )
    { // tableName = BaseShopConfig_BallEquipment_Item
        // xmlConfig = BaseShopConfig
        presetTableName = null;
        foreach (var e in Conf.Instance.xml2fb.table_preset)
        {
            var m = Regex.Match( tableName, e.table );
            if (!m.Success)
                continue;
            presetTableName = e.preset;
            return true;
        }
        return false;
    }


    bool AllIsTextNodes( List<XmlNode> nodes )
    {
        foreach (var e in nodes)
        {
            if (!XmlHelper.IsTextNode( e, out var _ ))
                return false;
        }
        return true;
    }

    bool IsNodeAsArray( XmlNode node )
    {
        string path = $"{node.ParentNode.LocalName}/{node.LocalName}";

        return IsNodeAsArray( path );
    }
    public static string CurrentXmlDocName = "";
    public static bool IsNodeAsArray( string path )
    {
        string domain = CurrentXmlDocName.ToLower();
        string lower = path.ToLower();
        if (lower.EndsWith( "item" ))
            return true;

        switch (domain)
        {
            case "activityconfig":
                {
                    switch (lower)
                    {
                        case "boxitem/rare":
                        case "previewgroup/preview":
                        case "time/previewgroup":
                            return true;
                    }
                }
                break;
            case "artseasonconfig":
                {
                    switch (lower)
                    {
                        case "previewgroup/preview":
                        case "item/skin":
                        case "item/rewards":
                            return true;
                    }
                }
                break;
            case "battlepassconfig":
                {
                    switch (lower)
                    {
                        case "seasonfloortex/floor":
                        case "seasonfloortex/vipfloor":
                        case "seasonfloortex/supervipfloor":
                            return true;
                    }
                }
                break;
            case "bestballconfig":
                {
                    switch (lower)
                    {
                        case "guide/phasereward":
                        case "guide/achevement":
                        case "act/rank":
                            return true;
                    }
                }
                break;
            case "customkeyconfig":
                {
                    switch (lower)
                    {
                        case "macro/condition":
                            return true;
                    }
                }
                break;
            case "dbrushconfig":
                {
                    switch (lower)
                    {

                        case "toptag/activity":
                        case "mainbanner/banner":
                            return true;
                    }
                }
                break;
            case "gameplaysettingconfig":
                {
                    switch (lower)
                    {
                        case "item/level":
                            return true;
                    }
                }
                break;
            case "newactivityconfig":
                {
                    switch (lower)
                    {
                        case "activity/tabs":
                            return true;
                    }
                }
                break;
            case "newactivityconfig2":
                {
                    switch (lower)
                    {
                        case "activity/items":
                        case "activity/showitems":
                            return true;
                    }
                }
                break;
            case "pvzartseasonconfig":
                {
                    switch (lower)
                    {
                        case "item/rewards":
                            return true;
                    }
                }
                break;
            case "xml2excelforclient":
                {
                    switch (lower)
                    {
                        case "item/starttime":
                        case "item/endtime":
                            return true;
                    }
                }
                break;

            default:
                {
                    if (Conf.Instance != null && Conf.Instance.nodeAsArrays != null && Conf.Instance.nodeAsArrays.Count > 0)
                    {
                        foreach (var e in Conf.Instance.nodeAsArrays)
                        {
                            if (e.xml.ToLower() != domain)
                                continue;

                            foreach (var nodePath in e.node_paths)
                            {
                                if (nodePath.ToLower() == lower)
                                {
                                    return true;
                                }
                            }

                        }
                    }
                }
                break;
        }


        return false;
    }

    internal static void AddVectorField(string xmlNodeName, string subTableName, FbTypeDecl table)
    {
        string fieldName = FbScalarType.ConsVectorFieldName(xmlNodeName);
        string typeName = FbScalarType.ConsVectorTypeName( subTableName );

        table.WithField(fieldName, typeName);
    }

    /// <summary>
    /// 从Xml自闭节点生成Fbs table
    /// </summary>
    /// <param name="node"></param>
    /// <returns></returns>
    internal static FbTypeDecl FromSelfCloseNode(XmlElement node)
    {
        FbTypeDecl table = new FbTypeDecl
        {
            typeName = node.Name.ToTableName(),
        };
        foreach(XmlAttribute attrib in node.Attributes)
        {
            table.WithField(attrib.Name.ToFieldName(), FbHelper.GetFbTypeFromXmlValue( attrib.Value, attrib.Name ) );
        }
        return table;
    }

}

