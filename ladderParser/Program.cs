using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ladderParser
{
    internal static class Program
    {
        public class Point{
            public int X;
            public int Y;
        }
        public class LadderObject
        {
            public int ID;
            public bool Start;
            public bool End;
            public List<int> OutRefs;
            public List<int> InRefs;

            public Point Index;
            public override string ToString()
            {
                return $"ID: {ID}, Start: {Start}, End: {End}, OutRefs: [{string.Join(",", OutRefs)}], InRefs: [{string.Join(",", InRefs)}]";
            }
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
                var indexNode = x.Element("Index");
                if (indexNode != null)
                {
                    string[] parts = indexNode.Value.Split(',');
                    if (parts.Length == 2)
                    {
                        int xIndex, yIndex;
                        if (int.TryParse(parts[1], out xIndex) && int.TryParse(parts[0], out yIndex))
                        {
                            obj.Index = new Point { X = xIndex, Y = yIndex };
                        }
                    }
                }

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
        public static List<Point> CheckConnectivity(List<LadderObject> objs) 
        {
            var map = objs.ToDictionary(o => o.ID);
            var conn = new Dictionary<int, ConnectionInfo>();
            var results = new List<Point>();

            foreach (var o in objs)
                conn[o.ID] = new ConnectionInfo();

            // 检查所有 OutRefs，构建入度和出度信息
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
                    results.Add(o.Index);
                    Console.WriteLine("孤立节点：{0}", o.ID);
                    continue;
                }

                // 2. 没有连接左母线
                if (!o.Start && ci.InDegree == 0)
                {
                    results.Add(o.Index);
                    Console.WriteLine("未连接左母线：{0}", o.ID);
                }
                    

                // 3. 没有连接右母线
                if (!o.End && ci.OutDegree == 0)
                {
                    results.Add(o.Index);
                    Console.WriteLine("未连接右母线：{0}", o.ID);
                }
                    

                // 4. start=true 但仍然没人连？
                if (o.Start && ci.InDegree > 0)
                    Console.WriteLine("警告：{0} 标记 start，但仍存在入边", o.ID);

                // 5. end=true 但仍然连出？
                if (o.End && ci.OutDegree > 0)
                    Console.WriteLine("警告：{0} 标记 end，但仍存在出边", o.ID);
            }
            return results;
        }

        public static void RemoveFbdAndUpdateLines(XDocument doc)
        {
            //int FIXHEIGHT = 70;
            int FIXWIDTH = 40;

            var objects = doc.Descendants("Object").ToList();
            var lineSegments = doc.Descendants("LineSegments").FirstOrDefault();
            

            if (lineSegments == null) return;

            var lines = lineSegments.Elements("Line").ToList();

            foreach (var fbd in objects)
            {
                var typeAttr = fbd.Attribute("Type");
                if (typeAttr == null || typeAttr.Value != "FBD")
                    continue;

                // 1. 找 FBD 的 index 和 size
                var indexElem = fbd.Element("Index");
                var sizeElem = fbd.Element("Size");
                var idElement = fbd.Element("ID");
                if (idElement == null) continue;
                if (sizeElem == null) continue;
                if (indexElem == null) continue;

                string[] idxParts = indexElem.Value.Split(',');
                if (idxParts.Length != 2) continue;

                string[] sizePart = sizeElem.Value.Split(',');
                if (sizePart.Length != 2) continue;

                int fx = int.Parse(idxParts[1]);
                int fy = int.Parse(idxParts[0]);

                int fwidth = int.Parse(sizePart[0]);
                int fheight = int.Parse(sizePart[1]);


                string fbdId = idElement.Value;

                // 1. 找 FBD 的上游（谁指向它）
                List<XElement> upstreamObjects = new List<XElement>();
                foreach (var obj in objects)
                {
                    var outs = obj.Descendants("OUT").FirstOrDefault();
                    if (outs == null) continue;

                    foreach (var refid in outs.Elements("RefID"))
                    {
                        var idAttr = refid.Attribute("ID");
                        if (idAttr != null && idAttr.Value == fbdId)
                        {
                            upstreamObjects.Add(obj);
                            break;
                        }
                    }
                }

                // 2. 找 FBD 的下游（它指向谁）
                List<string> downstreamIds = new List<string>();
                var fbdOut = fbd.Descendants("OUT").FirstOrDefault();
                if (fbdOut != null)
                {
                    foreach (var refid in fbdOut.Elements("RefID"))
                    {
                        var idAttr = refid.Attribute("ID");
                        if (idAttr != null)
                            downstreamIds.Add(idAttr.Value);
                    }
                }
                // 3. 重建连接：上游 → 下游
                foreach (var upObj in upstreamObjects)
                {
                    var upOut = upObj.Descendants("OUT").FirstOrDefault();
                    if (upOut == null) continue;

                    // 删除旧的 FBD 指向
                    upOut.Elements("RefID")
                         .Where(r => r.Attribute("ID") != null &&
                                     r.Attribute("ID").Value == fbdId)
                         .Remove();

                    // 增加新的目标
                    foreach (var dId in downstreamIds)
                    {
                        upOut.Add(
                            new XElement("RefID",
                                new XAttribute("ID", dId),
                                "IN"
                            )
                        );
                    }
                }

                // ========== 4. 更新 LineSegments ============

                // 直接计算控件所占据的WIDTH Index 宽度添加一条新线

                int controlIndexwith = (fwidth - 1) / FIXWIDTH;

                lineSegments.Add(
                    new XElement("Line",
                            new XAttribute("StartIndex",$"{fx},{fy}"),
                            new XAttribute("EndIndex",$"{fx+controlIndexwith},{fy}")
                        )
                    );

                // ========== 5. 删除 FBD Object ============
                fbd.Remove();
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

        }
    }
}
