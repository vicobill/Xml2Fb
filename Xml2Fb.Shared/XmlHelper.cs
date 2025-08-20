using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace Xml2Fb.Shared
{
    public static class XmlHelper
    {
        /// <summary>
        /// 是否纯粹的Text节点，形如： <id>1234</id>，而不是 <id name="xxx">123</id>
        /// </summary>
        /// <param name="node"></param>
        /// <param name="child"></param>
        /// <returns></returns>
        public static bool IsTextNode(XmlNode node,out XmlNode child)
        {
            child = null;

            if(node.ChildNodes.Count == 1 && node.FirstChild.NodeType == XmlNodeType.Text
                && node.Attributes.Count == 0)
            {
                child = node.FirstChild;
                return true;
            }
            return false;
        }
        /// <summary>
        /// 对子节点进行分组
        /// </summary>
        /// <param name="node"></param>
        /// <param name="xmlElemGroups"></param>
        public static void GroupSubNodes(XmlNode node, out Dictionary<string, List<XmlNode>> xmlElemGroups )
        {
            xmlElemGroups = new Dictionary<string, List<XmlNode>>();
            foreach (XmlNode child in node.ChildNodes)
            {
                if (!xmlElemGroups.TryGetValue( child.Name, out var xmlElemList ))
                {
                    xmlElemList = new List<XmlNode>();
                    xmlElemList.Add( child );
                    xmlElemGroups[child.Name] = xmlElemList;
                }
                else
                {
                    xmlElemList.Add( child );
                }
            }
        }
    }
}
