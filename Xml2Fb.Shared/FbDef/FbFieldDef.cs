using System;
using System.Collections.Generic;
using System.Text;
using Xml2Fb.Shared.FbGrammar;

namespace Xml2Fb.Shared.FbDef
{
    // <id name="value" /> 里的 name="value"
    // 对应fbs：
    // var name = value;
    public class FbFieldDef
    {
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string FieldValue { get; set; }
        public bool IsVectorType { get; set; }
        /// <summary>
        /// 所属表
        /// </summary>
        public string ParentTableName { get; set; }

        public FbFieldDef(string fieldName )
        {
            FieldName = fieldName.ToFieldName();
        }

        void GenStringVar( StringBuilder sb, int i )
        {
            string varName = $"{FieldName}_{i}".ToFieldName();
            
            sb.AppendLine( $"var {varName} = builder.CreateString(\"\"\"{FieldValue}\"\"\")" );
        }
        void GenScalarVar( StringBuilder sb, int i )
        {
            string varName = $"{FieldName}_{i}".ToFieldName();
            sb.AppendLine( $"var {varName} = ({FieldType})({FieldValue});" );
        }

        public void GenerateSerializerCode( StringBuilder sb, int index )
        {
            if (FieldType == "string")
            {
                GenStringVar( sb, index );
            }
            else if ( FieldType.IsFbScalarType())
            {
                GenScalarVar( sb, index );
            }
            else
            {
                DebugHelper.PrintWarning( $"未实现的类型：{FieldType} {FieldName} = {FieldValue}" );
            }
        }
    }
}
