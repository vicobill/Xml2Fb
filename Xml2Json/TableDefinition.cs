using Newtonsoft.Json.Utilities;
using System.Text;
using System.Xml;
using Xml2Fb.Shared;
using Xml2Fb.Shared.FbDef;
using Xml2Fb.Shared.Passes;

namespace Xml2Fb;

public enum EFbScalarType
{
    None,
    FBT_Byte, FBT_UByte,
    FBT_Bool,
    FBT_Short, FBT_UShort,
    FBT_Int, FBT_UInt,
    FBT_Long, FBT_ULong,
    FBT_Float, FBT_Double,
    FBT_String,
}



public class FbTypeAndTypeNameHelper
{
    public record FbScalarTypeAndName( EFbScalarType type, string typeName, string? aliaTypeName );

    public static readonly FbScalarTypeAndName[] s_fbScalarTypeAndNames = new FbScalarTypeAndName[]
    {
        new FbScalarTypeAndName(EFbScalarType.FBT_Byte,"byte","int8"),
        new FbScalarTypeAndName(EFbScalarType.FBT_UByte,"ubyte","uint8"),
        new FbScalarTypeAndName(EFbScalarType.FBT_Bool,"bool",null),
        new FbScalarTypeAndName(EFbScalarType.FBT_Short,"short","int16"),
        new FbScalarTypeAndName(EFbScalarType.FBT_UShort,"ushort","uint16"),
        new FbScalarTypeAndName(EFbScalarType.FBT_Int,"int","int32"),
        new FbScalarTypeAndName(EFbScalarType.FBT_UInt,"uint","uint32"),
        new FbScalarTypeAndName(EFbScalarType.FBT_Long,"long","int64"),
        new FbScalarTypeAndName(EFbScalarType.FBT_ULong,"ulong","uint64"),
        new FbScalarTypeAndName(EFbScalarType.FBT_Float,"float","float32"),
        new FbScalarTypeAndName(EFbScalarType.FBT_Double,"double","float32"),
        new FbScalarTypeAndName(EFbScalarType.FBT_String,"string",null)
    };

    public static string GetTypeName( EFbScalarType scalarType )
    {
        return s_fbScalarTypeAndNames.Where( x => x.type == scalarType )
            .Select( x => x.typeName )
            .FirstOrDefault();
    }
}


internal class VectorFieldDefinition
{
    public string name;
    public EFbScalarType scalarType;
    public int? numElements;
    public string ToString()
    {
        string type = FbTypeAndTypeNameHelper.GetTypeName( scalarType );
        if (numElements.HasValue)
        {
            return $"{name} : [{type} : {numElements}];";
        }
        else
        {
            return $"{name} : [{type}];";
        }
    }
}


public class StructDefinition
{
    public string Name { get; private set; }
    public StructDefinition( string name )
    {
        Name = name;
    }
    // 结构体定义实现
    public Dictionary<string, string> fields = new Dictionary<string, string>();
    public StructDefinition AddField( string fieldName, string fieldType )
    {
        fields[fieldName] = fieldType;
        return this;
    }

    public override string ToString()
    {
        StringBuilder sb = new StringBuilder();
        sb.AppendLine( $"struct {Name} {{" );
        foreach (var kv in fields)
        {
            sb.AppendLine( $"\t{kv.Key} : {kv.Value};" );
        }
        sb.AppendLine( "}" );

        return sb.ToString();

    }
}

public enum FieldOption
{
    None,
    Key,
    Packed
}

class SchemaDefinition
{
    public string pkgName; // namespace {{pkgName}}
    public List<string> attributes = new();
    public Dictionary<string, TableDefinition> tableDefinitions = new();
    public string rootType;
}

public class TableDefinition
{
    /// <summary>
    /// 表格名称
    /// </summary>
    public string Name { get; }
    /// <summary>
    /// 各个字段
    /// </summary>
    public Dictionary<string, FieldDefinition> Fields { get; } = new();

    public TableDefinition( string name )
    {
        Name = name.Codize().Capitalize();
    }

    public void AddField( string name, string type, FieldOption option = FieldOption.None )
    {
        //Fields[name.LowerSnakeCase()] = new FieldDefinition( name, type, option );
        string key = name.ToFieldName();
        if (Fields.TryGetValue( key, out FieldDefinition? field ))
        {
            if (field.Type == "string" ||
                ( FieldDefinition.IsVectorType( field.Type ) && !FieldDefinition.IsVectorType( type ) ))
                return;

            if (type == "string" && field.Type != "string")
            {
                field.Type = type;
                return;
            }
            
            if (FieldDefinition.IsVectorType(type) && !FieldDefinition.IsVectorType( field.Type ))
            {
                field.Type = type;
                return;
            }

            if (type == "float" && field.Type != "float")
            {
                field.Type = type;
                return;
            }

            if (type == "int" && field.Type == "uint")
            {
                field.Type = type;
                return;
            }

            
        }
        else
        {
            Fields[key] = new FieldDefinition( name, type, option );
        }
    }

    internal void Define( SchemaBuilder builder )
    {
        builder.AppendLine( $"table {Name.Codize()} {{" );
        foreach (var kv in Fields)
        {
            builder.AppendLine( kv.Value.ToString() );
        }
        builder.AppendLine( "}" );
    }

    public static TableDefinition FromSelfCloseNode( XmlElement node )
    {
        TableDefinition table = new TableDefinition( node.Name );
        foreach (XmlAttribute attrib in node.Attributes)
        {
            table.AddField( attrib.Name, FbHelper.GetFbTypeFromXmlValue( attrib.Value , attrib.Name ) );
        }
        return table;
    }



    /// <summary>
    /// 添加数组/列表字段
    /// </summary>
    /// <param name="xmlNodeName"></param>
    public void AddVectorField( string xmlNodeName, string subTableName )
    {
        string fieldName = GetVectorFieldName( xmlNodeName );
        string typeName = GetVectorTypeName( subTableName );
        if (Fields.TryGetValue( fieldName, out var field ))
        {
            //DebugHelper.PrintWarning( $"已经存在属性:{fieldName},将作为数组" );
            field.Type = typeName;
        }
        else
        {
            Fields[fieldName] = new FieldDefinition( fieldName, typeName );
        }
    }

    internal static string GetVectorTypeName( string scalarType )
    {
        if (FbHelper.IsFbScalarType( scalarType ))
            return $"[{scalarType}]";
        return $"[{scalarType.Capitalize()}]";
    }
    internal static string GetVectorFieldName( string tableName ) => $"{tableName}s";
}


public class FieldDefinition
{
    public FieldDefinition( string name, string type )
    {
        Name = name;
        Type = type;
    }
    public FieldDefinition( string name, string type, FieldOption opt )
    {
        Name = name;
        Type = type;
        Option = opt;
    }

    public FieldOption Option { get; set; }
    /// <summary>
    /// 字段名
    /// </summary>
    public string Name { get; set; }
    /// <summary>
    /// 字段简单类型
    /// </summary>
    public string Type { get; set; }


    public override string ToString()
    {
        string name = Name.ToFieldName();
        StringBuilder sb = new StringBuilder();
        if (name.StartsWith( "attr_" ))
        {
            string field = name.Substring(5, name.Length - 5);
            sb.Append( "attr_" ).Append( field.ToFieldName() );
            name = sb.ToString();
        }
        var pass2 = new CsharpAliasName { Name = name };
        pass2.Pass();
        return $"{pass2.Result} : {Type.Codize()};";
    }

    public static bool IsVectorType(string type)
    {
        var span = type.AsSpan();
        return span[0] == '[' && span[^1] == ']';
    }


}
