using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Script.Serialization;
using System.Xml;

namespace xml2json
{

    internal class Program
    {
        /// <summary>
        /// XML ↔ JSON 互转（保留根节点）
        /// </summary>
        public static class XmlJsonConverter
        {
            #region XML → JSON（保留根节点名）
            public static string XmlToJson(string xml)
            {
                XmlDocument doc = new XmlDocument();
                doc.LoadXml(xml);

                var dict = ToDictionary(doc.DocumentElement);
                /* ① 把根节点名一起塞进字典 */
                dict["#root"] = doc.DocumentElement.Name;

                return new JavaScriptSerializer().Serialize(dict);
            }

            private static Dictionary<string, object> ToDictionary(XmlNode node)
            {
                var dict = new Dictionary<string, object>();

                if (node.Attributes != null)
                    foreach (XmlAttribute attr in node.Attributes)
                        dict["@" + attr.Name] = attr.Value;

                foreach (XmlNode child in node.ChildNodes)
                {
                    if (child.NodeType == XmlNodeType.Element)
                    {
                        var childDict = ToDictionary(child);
                        if (dict.ContainsKey(child.Name))
                        {
                            if (!(dict[child.Name] is ArrayList list))
                            {
                                list = new ArrayList { dict[child.Name] };
                                dict[child.Name] = list;
                            }
                            list.Add(childDict);
                        }
                        else
                        {
                            dict[child.Name] = childDict;
                        }
                    }
                    else if (child.NodeType == XmlNodeType.Text && !string.IsNullOrWhiteSpace(child.Value))
                    {
                        dict["#text"] = child.Value;
                    }
                }
                return dict;
            }
            #endregion

            #region JSON → XML（用回原来根节点名）
            public static string JsonToXml(string json)
            {
                var ser = new JavaScriptSerializer();
                var root = (Dictionary<string, object>)ser.DeserializeObject(json);

                /* ② 取出原根节点名 */
                string rootName = root.ContainsKey("#root") ? root["#root"].ToString() : "root";
                if (rootName == string.Empty) rootName = "root";

                var sb = new StringBuilder();
                using (var xw = XmlWriter.Create(sb, new XmlWriterSettings { OmitXmlDeclaration = true, Indent = true }))
                {
                    /* 把 "#root" 字段拿掉，再写 XML */
                    var data = new Dictionary<string, object>(root);
                    data.Remove("#root");
                    WriteDict(xw, data, rootName);
                }
                return sb.ToString();
            }

            private static void WriteDict(XmlWriter xw, Dictionary<string, object> dict, string nodeName)
            {
                xw.WriteStartElement(nodeName);
                foreach (var kv in dict)
                {
                    string key = kv.Key;
                    object val = kv.Value;
                    if (key.StartsWith("@"))
                    {
                        xw.WriteAttributeString(key.Substring(1), val.ToString());
                    }
                    else if (key == "#text")
                    {
                        xw.WriteString(val.ToString());
                    }
                    else if (val is Dictionary<string, object> childDict)
                    {
                        WriteDict(xw, childDict, key);
                    }
                    else if (val is ArrayList list)
                    {
                        foreach (var item in list)
                        {
                            if (item is Dictionary<string, object> d)
                                WriteDict(xw, d, key);
                            else
                            {
                                xw.WriteStartElement(key);
                                xw.WriteString(item?.ToString() ?? "");
                                xw.WriteEndElement();
                            }
                        }
                    }
                    else
                    {
                        xw.WriteStartElement(key);
                        xw.WriteString(val?.ToString() ?? "");
                        xw.WriteEndElement();
                    }
                }
                xw.WriteEndElement();
            }
            #endregion
        }
        static void Main(string[] args)
        {
            string xml = File.ReadAllText("sample.xml");
            Console.WriteLine(XmlJsonConverter.XmlToJson(xml));
            Console.WriteLine(XmlJsonConverter.JsonToXml(XmlJsonConverter.XmlToJson(xml)));
        }
    }
}
