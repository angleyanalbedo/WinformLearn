using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace drawGrapha
{

     
    public class Element
    {
        public string ID { get; set; }          // 元素的唯一标识符
        public string TypeText { get; set; }    // 元素的类型描述
        public string ORefID { get; set; }      // 输出引用的元素 ID
        public string IRefID { get; set; }      // 输入引用的元素 ID
        public Element(string id, string typeText, string oRefID = null, string iRefID = null)
        {
            ID = id;
            TypeText = typeText;
            ORefID = oRefID;
            IRefID = iRefID;
        }
        // 无参构造
        public Element() { }
    }
    internal static class Program
    {
        /// <summary>
        /// 把 List<List<Element>> 转成 Graphviz 的 DOT 代码，
        /// 一键粘贴到 https://dreampuf.github.io/GraphvizOnline 即可看图。
        /// </summary>
        public static string GenerateDotGraph(List<List<Element>> elementarray)
        {
            var sb = new System.Text.StringBuilder();

            //---------- 1. 文件头 ----------
            sb.AppendLine("digraph G {");
            sb.AppendLine("  rankdir=LR;          // 从左到右画，喜欢上下就改成 TB");
            sb.AppendLine("  node [shape=box, style=rounded];");

            // 用来去重：同一个 ID 只画一次节点
            var allNodes = new Dictionary<string, string>();

            //---------- 2. 收集所有节点 ----------
            foreach (var row in elementarray)
            {
                foreach (var e in row)
                {
                    // 用 TypeText + ID 当标签，一眼看出这是啥控件
                    allNodes[e.ID] = $"{e.TypeText}\\n{e.ID}";
                }
            }

            //---------- 3. 画节点 ----------
            foreach (var kv in allNodes)
            {
                sb.AppendLine($"  \"{kv.Key}\" [label=\"{kv.Value}\"];");
            }

            //---------- 4. 画边（连线） ----------
            foreach (var row in elementarray)
            {
                foreach (var e in row)
                {
                    // 输出连出去的方向
                    if (!string.IsNullOrEmpty(e.ORefID))
                    {
                        sb.AppendLine($"  \"{e.ID}\" -> \"{e.ORefID}\";   // {e.ID} 指向 {e.ORefID}");
                    }

                    // 输入连进来的方向（如果只想看“出边”可以删掉这块）
                    if (!string.IsNullOrEmpty(e.IRefID))
                    {
                        sb.AppendLine($"  \"{e.IRefID}\" -> \"{e.ID}\";   // {e.IRefID} 指向 {e.ID}");
                    }
                }
            }

            //---------- 5. 文件尾 ----------
            sb.AppendLine("}");
            return sb.ToString();
        }
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {

            // 1. 手动造一张小图：2 行 1 列，共 3 个节点
            //    结构：Start -> Box -> End
            var elementarray = new List<List<Element>>
            {
                new List<Element>   // 第 0 行
                {
                    new Element
                    {
                        ID       = "Start",
                        TypeText = "Button",
                        ORefID   = "Box1"
                    },
                    new Element
                    {
                        ID       = "Box1",
                        TypeText = "TextBox",
                        IRefID   = "Start",
                        ORefID   = "End"
                    }
                },
                new List<Element>   // 第 1 行
                {
                    new Element
                    {
                        ID       = "End",
                        TypeText = "Label",
                        IRefID   = "Box1"
                    }
                }
            };

            // 2. 生成 DOT
            string dot = GenerateDotGraph(elementarray);

            // 3. 打印到控制台
            Console.WriteLine("=== 复制下面全部内容到 GraphvizOnline 即可看图 ===");
            Console.WriteLine(dot);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
