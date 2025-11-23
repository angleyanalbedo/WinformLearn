using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace xmlGraphHelper
{
 

    /// <summary>
    /// XML图结构操作工具类
    /// </summary>
    public class XmlGraphHelper
    {
        private XDocument _xmlDoc;
        private string _filePath;

        public XmlGraphHelper(string xmlFilePath)
        {
            _filePath = xmlFilePath;
            _xmlDoc = XDocument.Load(xmlFilePath);
        }

        public XmlGraphHelper(XDocument xmlDoc)
        {
            _xmlDoc = xmlDoc;
        }

        /// <summary>
        /// 删除节点及其相关的边
        /// </summary>
        /// <param name="nodeId">要删除的节点ID</param>
        public void DeleteNode(string nodeId)
        {
            var objectElement = _xmlDoc.Descendants("Object")
                                     .FirstOrDefault(o => o.Element("ID")?.Value == nodeId);

            if (objectElement != null)
            {
                // 删除所有指向该节点的入边
                RemoveIncomingEdges(nodeId);

                // 删除所有从该节点指出的出边
                RemoveOutgoingEdges(nodeId);

                // 删除节点本身
                objectElement.Remove();
            }
        }

        /// <summary>
        /// 删除指定节点之间的边
        /// </summary>
        /// <param name="fromNodeId">起始节点ID</param>
        /// <param name="toNodeId">目标节点ID</param>
        public void DeleteEdge(string fromNodeId, string toNodeId)
        {
            // 从起始节点的OUT中删除对目标节点的引用
            var fromObject = _xmlDoc.Descendants("Object")
                                   .FirstOrDefault(o => o.Element("ID")?.Value == fromNodeId);

            if (fromObject != null)
            {
                var outElement = fromObject.Element("OUT");
                if (outElement != null)
                {
                    var refIds = outElement.Elements("RefID")
                                         .Where(r => r.Value == toNodeId)
                                         .ToList();

                    foreach (var refId in refIds)
                    {
                        refId.Remove();
                    }

                    // 如果OUT元素为空，可以选择保留或删除空元素
                    // 这里选择保留空元素以保持结构完整性
                }
            }

            // 从目标节点的IN中删除对起始节点的引用
            var toObject = _xmlDoc.Descendants("Object")
                                 .FirstOrDefault(o => o.Element("ID")?.Value == toNodeId);

            if (toObject != null)
            {
                var inElement = toObject.Element("IN");
                if (inElement != null)
                {
                    var refIds = inElement.Elements("RefID")
                                        .Where(r => r.Value == fromNodeId)
                                        .ToList();

                    foreach (var refId in refIds)
                    {
                        refId.Remove();
                    }
                }
            }
        }

        /// <summary>
        /// 删除所有指向指定节点的入边
        /// </summary>
        private void RemoveIncomingEdges(string nodeId)
        {
            // 找到所有OUT中包含该节点引用的对象
            var objectsWithOutgoingEdges = _xmlDoc.Descendants("object")
                .Where(o => o.Element("OUT")?.Elements("RefID")
                           .Any(r => r.Value == nodeId) == true);

            foreach (var obj in objectsWithOutgoingEdges)
            {
                var outElement = obj.Element("OUT");
                var refIdsToRemove = outElement.Elements("RefID")
                                             .Where(r => r.Value == nodeId)
                                             .ToList();

                foreach (var refId in refIdsToRemove)
                {
                    refId.Remove();
                }
            }
        }

        /// <summary>
        /// 删除所有从指定节点指出的出边
        /// </summary>
        private void RemoveOutgoingEdges(string nodeId)
        {
            var objectElement = _xmlDoc.Descendants("object")
                                     .FirstOrDefault(o => o.Element("ID")?.Value == nodeId);

            if (objectElement != null)
            {
                var outElement = objectElement.Element("OUT");
                if (outElement != null)
                {
                    // 获取所有出边指向的目标节点
                    var targetNodeIds = outElement.Elements("RefID")
                                                .Select(r => r.Value)
                                                .ToList();

                    // 从目标节点的IN中删除对应的引用
                    foreach (var targetId in targetNodeIds)
                    {
                        var targetObject = _xmlDoc.Descendants("object")
                                                .FirstOrDefault(o => o.Element("ID")?.Value == targetId);

                        if (targetObject != null)
                        {
                            var inElement = targetObject.Element("IN");
                            if (inElement != null)
                            {
                                var refIdsToRemove = inElement.Elements("RefID")
                                                            .Where(r => r.Value == nodeId)
                                                            .ToList();

                                foreach (var refId in refIdsToRemove)
                                {
                                    refId.Remove();
                                }
                            }
                        }
                    }

                    // 清空当前节点的OUT
                    outElement.Elements("RefID").Remove();
                }
            }
        }

        /// <summary>
        /// 保存修改后的XML
        /// </summary>
        public void Save()
        {
            if (!string.IsNullOrEmpty(_filePath))
            {
                _xmlDoc.Save(_filePath);
            }
        }

        /// <summary>
        /// 保存到指定路径
        /// </summary>
        public void Save(string filePath)
        {
            _xmlDoc.Save(filePath);
        }

        /// <summary>
        /// 获取XML文档内容
        /// </summary>
        public XDocument GetXmlDocument()
        {
            return _xmlDoc;
        }

        /// <summary>
        /// 获取XML字符串
        /// </summary>
        public string GetXmlString()
        {
            return _xmlDoc.ToString();
        }
    }

    /// <summary>
    /// 高级XML图操作工具类
    /// </summary>
    public class AdvancedXmlGraphHelper
        {
            private XDocument _xmlDoc;

            public AdvancedXmlGraphHelper(XDocument xmlDoc)
            {
                _xmlDoc = xmlDoc;
            }

            public AdvancedXmlGraphHelper(string xmlContent)
            {
                _xmlDoc = XDocument.Parse(xmlContent);
            }

            /// <summary>
            /// 批量删除节点
            /// </summary>
            public void DeleteNodes(List<string> nodeIds)
            {
                foreach (var nodeId in nodeIds)
                {
                    DeleteSingleNode(nodeId);
                }
            }

            /// <summary>
            /// 批量删除边
            /// </summary>
            public void DeleteEdges(List<Tuple<string, string>> edges)
            {
                foreach (var edge in edges)
                {
                    DeleteSingleEdge(edge.Item1, edge.Item2);
                }
            }

            /// <summary>
            /// 删除孤立节点（没有边的节点）
            /// </summary>
            public void DeleteIsolatedNodes()
            {
                var isolatedNodes = _xmlDoc.Descendants("object")
                    .Where(o =>
                        (!o.Element("IN")?.Elements("RefID").Any() ?? true) &&
                        (!o.Element("OUT")?.Elements("RefID").Any() ?? true))
                    .Select(o => o.Element("ID")?.Value)
                    .Where(id => id != null)
                    .ToList();

                foreach (var nodeId in isolatedNodes)
                {
                    DeleteSingleNode(nodeId);
                }
            }

            /// <summary>
            /// 删除所有指向某个节点的边
            /// </summary>
            public void DeleteAllEdgesToNode(string targetNodeId)
            {
                var objectsWithEdgesToTarget = _xmlDoc.Descendants("object")
                    .Where(o => o.Element("OUT")?.Elements("RefID")
                               .Any(r => r.Value == targetNodeId) == true);

                foreach (var obj in objectsWithEdgesToTarget)
                {
                    var outElement = obj.Element("OUT");
                    var sourceNodeId = obj.Element("ID")?.Value;

                    if (sourceNodeId != null && outElement != null)
                    {
                        var refIdsToRemove = outElement.Elements("RefID")
                                                     .Where(r => r.Value == targetNodeId)
                                                     .ToList();

                        foreach (var refId in refIdsToRemove)
                        {
                            refId.Remove();

                            // 同时从目标节点的IN中删除
                            var targetObject = _xmlDoc.Descendants("object")
                                                    .FirstOrDefault(o => o.Element("ID")?.Value == targetNodeId);
                            if (targetObject != null)
                            {
                                var inElement = targetObject.Element("IN");
                                var inRefToRemove = inElement?.Elements("RefID")
                                                            .FirstOrDefault(r => r.Value == sourceNodeId);
                                inRefToRemove?.Remove();
                            }
                        }
                    }
                }
            }

            private void DeleteSingleNode(string nodeId)
            {
                var xmlHelper = new XmlGraphHelper(_xmlDoc);
                xmlHelper.DeleteNode(nodeId);
            }

            private void DeleteSingleEdge(string fromNodeId, string toNodeId)
            {
                var xmlHelper = new XmlGraphHelper(_xmlDoc);
                xmlHelper.DeleteEdge(fromNodeId, toNodeId);
            }

            public XDocument GetXmlDocument()
            {
                return _xmlDoc;
            }
        }
}
