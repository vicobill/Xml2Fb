//using Newtonsoft.Json.Utilities;
//using System.Text;
//using System.Xml;
//using Xml2Fb.FbGrammar;
//using Xml2Fb.Shared;
//using Xml2Fb.Shared.FbDef;
//using Xml2Fb.Shared.Passes;

//namespace Xml2Fb;
 
 

//public enum FieldOption
//{
//    None,
//    Key,
//    Packed
//}
 

//public class TableDefinition
//{
//    /// <summary>
//    /// 表格名称
//    /// </summary>
//    public string Name { get; }
//    /// <summary>
//    /// 各个字段名称和字段定义
//    /// </summary>
//    public Dictionary<string, FieldDefinition> Fields { get; } = new();

//    public TableDefinition( string name )
//    {
//        Name = name.Codize().Capitalize();
//    }

//    public void AddField( string name, string type, FieldOption option = FieldOption.None )
//    {
//        //Fields[name.LowerSnakeCase()] = new FieldDefinition( name, type, option );
//        string key = name.ToFieldName().ToLower();
//        // 已经存在字段，则合并
//        if (Fields.TryGetValue( key, out FieldDefinition? field ))
//        {
//            if (field.Type == "string" ||
//                ( FieldDefinition.IsVectorType( field.Type ) && !FieldDefinition.IsVectorType( type ) ))
//                return;

//            if (type == "string" && field.Type != "string")
//            {
//                field.Type = type;
//                return;
//            }

//            if (FieldDefinition.IsVectorType(type) && !FieldDefinition.IsVectorType( field.Type ))
//            {
//                field.Type = type;
//                return;
//            }

//            if (type == "float" && field.Type != "float")
//            {
//                field.Type = type;
//                return;
//            }

//            if (type == "int" && field.Type == "uint")
//            {
//                field.Type = type;
//                return;
//            }


//        }
//        else
//        {
//            Fields[key] = new FieldDefinition( name, type, option );
//        }
//    }
     

 


     

      
//}

///// <summary>
///// FlatBuffers中字段定义
///// </summary>
//public class FieldDefinition
//{
//    public FieldDefinition( string name, string type )
//    {
//        Name = name;
//        Type = type;
//    }
//    public FieldDefinition( string name, string type, FieldOption opt )
//    {
//        Name = name;
//        Type = type;
//        Option = opt;
//    }
//    /// <summary>
//    /// 字段选项
//    /// </summary>
//    public FieldOption Option { get; set; }
//    /// <summary>
//    /// 字段名
//    /// </summary>
//    public string Name { get; set; }
//    /// <summary>
//    /// 字段简单类型
//    /// </summary>
//    public string Type { get; set; }
//    public bool IsVector { get; set; }
//    public string DefaultValue { get; set; }



//    public override string ToString()
//    {
//        string name = Name.ToFieldName();
//        StringBuilder sb = new StringBuilder();
//        if (name.StartsWith( "attr_" ))
//        {
//            string field = name.Substring(5, name.Length - 5);
//            sb.Append( "attr_" ).Append( field.ToFieldName().ToLower() );
//            name = sb.ToString();
//        }
//        var pass2 = new CsharpAliasName { Name = name };
//        pass2.Pass();
//        return $"{pass2.Result} : {Type.Codize()};";
//    }

 


//}
