using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xml2Fb.Shared.FbDef
{
    public class FbTableDef
    {
        //public StringBuilder stringBuilder { get; private set; }
        public string Name { get; set; }
        public string RootType { get; set; }
        public Queue<FbFieldDef> FieldsStack { get; private set; }
        public bool IsVectorElement { get; set; }
        public bool IsRootType => !string.IsNullOrEmpty( RootType );
        public Queue<FbTableDef> SubTables { get; private set; }



        public FbTableDef(string tableName,bool isVectorElement)
        {
            Name = StringUtils.Codize(tableName).Capitalize();
            //this.stringBuilder = new StringBuilder();
            FieldsStack = new Queue<FbFieldDef>();
            IsVectorElement = isVectorElement;
            SubTables = new Queue<FbTableDef>();
        }

        internal static string GetVectorTypeName( string scalarType ) => $"[{scalarType.Capitalize()}]";
        internal static string GetVectorFieldName( string tableName ) => $"{tableName}s";

        public void AddVectorField(string xmlNodeName, string subTableName )
        {
            string fieldName = GetVectorFieldName( xmlNodeName );
            string typeName = GetVectorTypeName( subTableName );
            var field = new FbFieldDef( fieldName );
            field.FieldType = typeName;
            field.IsVectorType = true;

            FieldsStack.Enqueue( field );
        }


        public void AddField(FbFieldDef field )
        {
            FieldsStack.Enqueue(field);
        }

        static void GenTableVarHeader(StringBuilder sb, FbTableDef tableDef, int n )
        {
            string varname = StringUtils.ToSnakeCase( tableDef.Name );//.LowerSnakeCase();
            string name = tableDef.Name;
            sb.AppendLine( $"Offset<{name}> {varname}_{n};" );
        }
        static void GenTableVarFooter(StringBuilder sb, FbTableDef tableDef, int n,params string[] argNames)
        {
            string varname = StringUtils.ToSnakeCase( tableDef.Name );//.LowerSnakeCase();
            string name = tableDef.Name;
            sb.Append( $"{varname}_{n} = {name}.Create{name}(builder," );
            for (int i = 0; i < argNames.Length; i++)
            {
                sb.Append( argNames[i] );
                if(i < argNames.Length - 1) sb.Append(", ");
            }
            sb.AppendLine( ");" );
        }

        public void GenerateSerializerCode(StringBuilder sb,int index)
        {
            if (!IsRootType)
            {
                GenTableVarHeader(sb,this,index);
                int n = 0;
                List<string> argNames = new List<string>();
                sb.AppendLine( "{" );
                while (FieldsStack.TryDequeue(out FbFieldDef result))
                {
                    argNames.Add($"{result.FieldName.ToFieldName()}_{n}");

                    result.GenerateSerializerCode( sb,n );

                    n++;
                }
                GenTableVarFooter(sb,this,index,argNames.ToArray());
                sb.AppendLine( "}" );
            }
            else
            {

            }
        }


    }
}
