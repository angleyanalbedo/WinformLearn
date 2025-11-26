using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ladderParser
{
    internal static class Program
    {
        public class LadderObject
        {
            public int ID;
            public bool Start;
            public bool End;
            public List<int> OutRefs;
            public List<int> InRefs;

            public LadderObject()
            {
                OutRefs = new List<int>();
                InRefs = new List<int>();
            }
        }
        public static List<LadderObject> ParseObjects(string xmlPath)
        {
            var doc = XDocument.Load(xmlPath);
            var list = new List<LadderObject>();

            foreach (var x in doc.Descendants("Object"))
            {
                var obj = new LadderObject();
                obj.ID = (int)x.Element("ID");
                obj.Start = (bool?)x.Element("Start") == true;
                obj.End = (bool?)x.Element("End") == true;

                var connectNode = x.Element("Connect");
                if (connectNode != null)
                {
                    var outNode = connectNode.Element("OUT"); // 获取 OUT 节点
                    if (outNode != null)
                    {
                        foreach (var refElem in outNode.Elements("RefID")) // 遍历 RefID 子节点
                        {
                            var idAttr = refElem.Attribute("ID");
                            if (idAttr != null)
                            {
                                int id;
                                if (int.TryParse(idAttr.Value, out id))
                                {
                                    obj.OutRefs.Add(id); // 添加到 OutRefs 列表
                                }
                            }
                        }
                    }

                    // 同理，如果你要解析 IN，也可以：
                    var inNode = connectNode.Element("IN");
                    if (inNode != null)
                    {
                        foreach (var refElem in inNode.Elements("RefID"))
                        {
                            var idAttr = refElem.Attribute("ID");
                            if (idAttr != null)
                            {
                                int inId;
                                if (int.TryParse(idAttr.Value, out inId))
                                {
                                    obj.InRefs.Add(inId); // 添加到 InRefs 列表
                                }
                            }
                        }
                    }
                }


                list.Add(obj);
            }

            return list;
        }
        public class ConnectionInfo
        {
            public int InDegree = 0;      // 被多少节点连接
            public int OutDegree = 0;     // 有多少连接出去
        }
        public static void CheckConnectivity(List<LadderObject> objs) 
        {
            var map = objs.ToDictionary(o => o.ID);
            var conn = new Dictionary<int, ConnectionInfo>();

            foreach (var o in objs)
                conn[o.ID] = new ConnectionInfo();

            // 检查所有 outrefs
            foreach (var o in objs)
            {
                foreach (var t in o.OutRefs)
                {
                    if (!map.ContainsKey(t))
                    {
                        Console.WriteLine("错误：节点 {0} 指向不存在的 ID {1}", o.ID, t);
                        continue;
                    }

                    conn[o.ID].OutDegree++;
                    conn[t].InDegree++;
                }
            }

            Console.WriteLine();
            Console.WriteLine("=== 连通性检测结果 ===");

            foreach (var o in objs)
            {
                var ci = conn[o.ID];

                // 1. 完全孤立
                if (!o.Start && !o.End && ci.InDegree == 0 && ci.OutDegree == 0)
                {
                    Console.WriteLine("孤立节点：{0}", o.ID);
                    continue;
                }

                // 2. 没有连接左母线
                if (!o.Start && ci.InDegree == 0)
                    Console.WriteLine("未连接左母线：{0}", o.ID);

                // 3. 没有连接右母线
                if (!o.End && ci.OutDegree == 0)
                    Console.WriteLine("未连接右母线：{0}", o.ID);

                // 4. start=true 但仍然没人连？
                if (o.Start && ci.InDegree > 0)
                    Console.WriteLine("警告：{0} 标记 start，但仍存在入边", o.ID);

                // 5. end=true 但仍然连出？
                if (o.End && ci.OutDegree > 0)
                    Console.WriteLine("警告：{0} 标记 end，但仍存在出边", o.ID);
            }
        }

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Application.EnableVisualStyles();
            //Application.SetCompatibleTextRenderingDefault(false);
            //Application.Run(new Form1());
            var objs = ParseObjects("rung.xml");
            CheckConnectivity(objs);

        }
    }
}
