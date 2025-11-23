using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace xmlGraphHelper
{
    internal class Program
    {
        static void Main()
        {
            // 示例XML内容
            string xmlContent = File.ReadAllText("ld2.XML");

            // 使用工具类
            var xmlHelper = new XmlGraphHelper(XDocument.Parse(xmlContent));

            Console.WriteLine("原始XML:");
            Console.WriteLine(xmlHelper.GetXmlString());

            // 删除节点1
            xmlHelper.DeleteNode("1");
            Console.WriteLine("\n删除节点1后:");
            Console.WriteLine(xmlHelper.GetXmlString());

        //    // 删除边 (2->1)
        //    xmlHelper.DeleteEdge("2", "1");
        //    Console.WriteLine("\n删除边2->1后:");
        //    Console.WriteLine(xmlHelper.GetXmlString());

        //    // 使用高级工具类
        //    var advancedHelper = new AdvancedXmlGraphHelper(xmlContent);

        //    // 批量删除边
        //    var edgesToDelete = new List<Tuple<string, string>>
        //{
        //    Tuple.Create("3", "1"),
        //    Tuple.Create("1", "4")
        //};
        //    advancedHelper.DeleteEdges(edgesToDelete);

        //    Console.WriteLine("\n批量删除边后:");
        //    Console.WriteLine(advancedHelper.GetXmlDocument());
        }
    }
}
