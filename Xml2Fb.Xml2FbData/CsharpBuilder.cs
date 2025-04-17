using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using Xml2Fb.Shared.FbDef;
using Xml2Fb.Shared;
using System.Diagnostics;

namespace Xml2Fb.Xml2FbData
{

    /*
using Google.FlatBuffers; // The runtime library for C#
using MyGame.Sample; // The generated files from `flatc`
public static class MyConfigSerializer {
    public static string Serialize() {
        // Construct a Builder with 1024 byte backing array.
        FlatBufferBuilder builder = new FlatBufferBuilder(1024)

        MyConfig.StartMyConfig(builder);
        MyConfig.AddId(builder,10);
        MyConfig.AddName(builder,"NAME");
        MyConfig.AddWeapons(builder,weapons);
        Offset<MyConfig> my_config = MyConfig.EndMyConfig(builder)
        builder.Finish(my_config.Value);

        var bytes = builder.DataBuffer.ToSizedArray();
        System.IO.File.WriteAllBytes("MyConfig.bin", bytes);
    }
}
     */
    // 序列化一个XXXConfig.xml的数据到bin文件中。
    public class CsharpBuilder
    {
        private StringBuilder _stringBuilder = new();
        
        public string RootTableName { get; private set; }

        public CsharpBuilder( string tableName )
        {
            RootTableName = tableName;

        }
        string _curTableName;
        Stack<string> _tableNodeNameStack = new();
        public void BeginRootTable()
        {
            GenCsharpHeader();
            GenSerializerHeader();

            _curTableName = RootTableName;
            _tableNodeNameStack.Push( _curTableName );
        }

        public void EndRootTable()
        {
            _tableNodeNameStack.TryPop(out var _);
            GenSerializerFooter();
            GenWriteToFile();
            GenCsharpFooter();
        }
        Stack<string> _tableVarNameStack = new();
        
        public void BeginTable( string xmlNodeName,string tableName )
        {
            _curTableName = tableName;
            _tableNodeNameStack.Push( _curTableName );
            string type = $"{FbHelper.ObjectiveTableName( _curTableName )}";
            string varname = xmlNodeName.ToFieldName();
            _tableVarNameStack.Push( varname );
            _stringBuilder.AppendLine( $"{Indent(1)}{type} {varname} = new();" );
        }

        public void EndTable(string xmlNodeName,string tableName)
        {
            _tableNodeNameStack.TryPop( out var _ );
            if (_tableNodeNameStack.Count > 0)
            {
                _curTableName = _tableNodeNameStack.Peek();
            }
            else
            {
                _curTableName = string.Empty;
            }
            
        }

        void GenCsharpHeader()
        {
            _stringBuilder.Clear();
            _stringBuilder
                .AppendLine( "using Google.FlatBuffers;\nusing Config.ConfigObj;\n" )
                .AppendLine( "using System.IO;" );
        }


        void GenSerializerHeader()
        {
            _stringBuilder
                .AppendLine( $"public static class {RootTableName}Serializer {{" )
                .AppendLine( $"public static void Serialize() {{ " )
                .AppendLine( $"{Indent( 1 )}var config = new {RootTableName}T();" );

        }

        public void GenSerializerFooter()
        {
            //_stringBuilder
            //    .AppendLine( $"{Indent( 1 )}Offset<{RootTableName}> {RootTableName.LowerSnakeCase()} = {RootTableName}.End{RootTableName}(builder);" )
            //    .AppendLine( $"{Indent( 1 )}builder.Finish({RootTableName.LowerSnakeCase()}.Value);" )
            //    ;
        }

        public void GenWriteToFile()
        {
            _stringBuilder
                .AppendLine("\n\t/* 保存数据为二进制文件 */\n")
                .AppendLine( $"{Indent( 1 )}var bytes = config.SerializeToBinary();" )
                .AppendLine( $"{Indent(1)}File.WriteAllBytes(\"{RootTableName}.cfb\",bytes);" );
        }

        public void GenCsharpFooter()
        {

            _stringBuilder.AppendLine( $"{Indent( 1 )}}}" )
                .AppendLine( "}" );
        }

        public void BeginAddFields(string tableName)
        {

        }
        public void EndAddFields( string tableName )
        {
        }

        List<string> strings = new List<string>();
        public void AddField(string xmlAttrName, string xmlAttrVal )
        {
            string type = FbHelper.GetFbTypeFromXmlValue( xmlAttrVal,xmlAttrName );
            _tableVarNameStack.TryPeek(out var popvar);
            string varname = _curTableName == RootTableName ? "config" : $"{popvar.ToFieldName()}";
            string fieldname = xmlAttrName.UpperCamelCase();
            string fieldvalue = xmlAttrVal;
            if(type == "string")
            {
                string val = string.IsNullOrEmpty( fieldvalue ) ? "\"\"" : $"\"\"\"\n{fieldvalue}\n\"\"\"";
                _stringBuilder.AppendLine( $"{Indent(1)}{varname}.{fieldname} = {val};" );
            }
            else
            {
                _stringBuilder.AppendLine( $"{Indent(1)}{varname}.{fieldname} = {fieldvalue};" );
            }
            
        }

        Stack<string> _vectorNodeNameStack = new();
        
        public void VectorItem(string xmlNodeName, string subTableName)
        {
            /*
             {
             */
            string varname = _curTableName == RootTableName ? "config" : $"{_curTableName.ToFieldName()}";
            string fieldname = $"{xmlNodeName.UpperCamelCase()}s";
            _vectorNodeNameStack.Push( fieldname );

            _stringBuilder
                .AppendLine( $"{Indent( 1 )}{{" );
        }

        public void BeginVector( string xmlNodeName,string subTableName )
        {
            /*
              config.Items = new();// 如果
            {
             */
            string varname = _curTableName == RootTableName ? "config" : $"{_curTableName.ToFieldName()}";
            string fieldname = $"{xmlNodeName.UpperCamelCase()}s";

            _stringBuilder
                .AppendLine( $"{Indent(1)}{varname}.{fieldname} = new();" )
                ;

            // 将字段名入栈，如 Items
            _vectorNodeNameStack.Push( fieldname );
        }

        
        public void EndVector(string xmlNodeName, string subTableName)
        {
            /*
             config.Items.Add(item);
             }
             */
            string fieldname = _vectorNodeNameStack.Pop();
            string varname = _curTableName == RootTableName ? "config" : $"{_curTableName.ToFieldName()}";
            string type = $"{FbHelper.ObjectiveTableName( _curTableName )}";
            _stringBuilder.AppendLine( $"{Indent( 1 )}{varname}.{fieldname}.Add({xmlNodeName});" );
            _stringBuilder.AppendLine( $"{Indent(1)}}}" );
        }

        string Indent( int n )
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < n; ++i)
            {
                sb.Append( '\t' );
            }
            return sb.ToString();
        }

        
        public string GenerateSerializerCode()
        {
            //GenCsharpHeader();
            //GenSerializerHeader();

            //int n = 0;
            //// 子表序列化
            //while(_tableStack.TryDequeue(out FbTableDef result ))
            //{
            //    result.GenerateSerializerCode(_stringBuilder,n);
            //    n++;
            //}

            //GenSerializerFooter();
            //GenWriteToFile();
            //GenCsharpFooter();

            return _stringBuilder.ToString();
        }
    }
}
