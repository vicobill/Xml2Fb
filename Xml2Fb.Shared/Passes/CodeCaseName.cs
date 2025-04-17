using System;
using System.Collections.Generic;
using System.Text;

namespace Xml2Fb.Shared.Passes
{
    /// <summary>
    /// 将名称转成C#中
    /// </summary>
    public class CodeCaseName : IXml2FbPass
    {

        public string Name { get; set; }
        public bool ForFieldName { get; set; }
        public bool ForTableName { get; set; }

        public string Result { get; set; }

        public void Pass()
        {
            Result = Name;
            if (ForFieldName)
            {
                Result = Name.Codize().LowerCamelCase();
            }
            else if (ForTableName)
            {
                Result = Name.Codize().Capitalize();
            }
        }
    }
}
