using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Xml2Fb.Xml2FbData
{
    public static class XmlHelper
    {

        /// <summary>
        /// 按子节点名称，对子节点进行分组
        /// </summary>
        /// <param name="node"></param>
        /// <param name="groups">key:子节点名称 value: 成组的子节点列表</param>
        public static void GroupSubNodes( XmlNode node, out Dictionary<string, List<XmlNode>> groups )
        {
            groups = new Dictionary<string, List<XmlNode>>();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (!groups.TryGetValue( child.Name, out var xmlElemList ))
                {
                    xmlElemList = new List<XmlNode>();
                    xmlElemList.Add( child );
                    groups[child.Name] = xmlElemList;
                }
                else
                {
                    xmlElemList.Add( child );
                }
            }
        }

        // <id title="xxx"> 123 </id>
        // 这种特殊处理，将Value作为单独字段，而不是子节点
        public static bool IsTextNode (XmlNode node,out string value)
        {
            value = string.Empty;
            if (node.ChildNodes.Count != 1)
                return false;
            bool @is = node.ChildNodes.Count == 1 && node.FirstChild.NodeType == XmlNodeType.Text;
            value = node.FirstChild.Value;
            return @is;
        }



    }
}
