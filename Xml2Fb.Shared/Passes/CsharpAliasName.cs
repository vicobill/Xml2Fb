using Newtonsoft.Json.Utilities;
using System;
using System.Collections.Generic;
using System.Text;

namespace Xml2Fb.Shared.Passes
{
    /// <summary>
    /// 避免CSharp的关键字
    /// </summary>
    public class CsharpAliasName : IXml2FbPass
    {
        public string Name { get; set; }
        public string Result { get; set; }

        public void Pass()
        {
            Result = Name;
            Result = CSharpKeywordAlias.Alias( Name );
        }
    }
}
