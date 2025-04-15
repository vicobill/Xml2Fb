// See https://aka.ms/new-console-template for more information
using System.Diagnostics;
using System.Runtime.ExceptionServices;
using System.Xml;
using Xml2Fb.Shared.FbDef;
using Xml2Fb.Xml2FbData;

/* 读取xml，生成flat buffers Serialize和Deserialize代码
 * 通过Serialize代码，保存数据为FlatBuffers 二进制文件
 * 通过Deserialize代码，读取保存的FlatBuffers二进制文件
 */

string xmlFile = "E:\\project\\my-tools\\FlatBufferTool-master\\XmlToFlatbuffer\\XML\\LanguageConfig.xml";


string s = new Xml2FbDataGenerator( xmlFile )
    .Generate();
File.WriteAllText( "language_serializer.cs", s );



