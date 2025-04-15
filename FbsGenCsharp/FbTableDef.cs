////using System;
////using System.Collections.Generic;
////using System.Linq;
////using System.Text;
////using System.Threading.Tasks;

//using System.Text;
//using Xml2Fb.Shared;

namespace FbsGenCsharp;

//internal class FbFieldDef
//{
//    internal string FieldType;
//    internal string FieldName;
//    internal string FieldValue;
//    internal bool IsVectorType;
//    internal int FieldIndex;

//    internal void GenCsharp(StringBuilder stringBuilder)
//    {
//        if (FieldType.ToLower() == "string")
//        {
//            GenStringField(stringBuilder); 
//        }
        
//    }



//    internal void GenStringField( StringBuilder stringBuilder ) {
//        stringBuilder.AppendLine( $"Offset<string> {FieldName.LowerSnakeCase()}_{FieldIndex} = builder.CreateString(\"{FieldValue}\");" );
//    }
//}

//internal class FbTableDef
//{
//    internal string TableName;
//    internal List<FbFieldDef> TableFields;

//    internal void Start(StringBuilder stringBuilder)
//    {
//        stringBuilder.AppendLine( $"{TableName}.Start{TableName}(builder);" );
//    }

//    internal void GenCsharp(StringBuilder stringBuilder)
//    {
//        /* @example:
//         *      ArtConfig.StartArtConfig(builder);
//         *      ArtConfig.
//         * 
//         * 
//         */ 
        
//        foreach(var field in TableFields)
//        {
//            stringBuilder.AppendLine( $"{TableName}.Add{field.FieldName}(builder, {field.FieldValue});" );
//        }

//        stringBuilder.AppendLine($"var")
            
//    }
//}


////namespace Xml2FbSchemaGenerator.Fb
////{
////    /* <item level="1" rewardids="1001:5, 1003:3" /> */
////    internal class FbTableDef
////    {
////        public string TableName { get; set; }
////        public Dictionary<string, FbFieldDef> TableFields { get; set; } = new();


////        public void Generate()
////        {
////            $"{TableName}.Start{TableName}(builder);";
////            $"{TableName}.Add{FieldName}(builder,{FieldValue};";
////            $"var {TableName.LowerSnakeCase()} = {TableName}.End{TableName}(builder);";
////            $"builder.Finish({TableName.LowerSnakeCase()}.Value);";

////    }


////    internal class FbFieldDef
////    {
////        public string FieldName { get; set; }
////        public string FieldType { get; set; }
////        public string FieldValue { get; set; }

////        public bool IsVectorType { get; set; }


////    }

////}
