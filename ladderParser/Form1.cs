using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;

namespace ladderParser
{
    public class Point2D
    {
        public int X { get; set; }
        public int Y { get; set; }
    }

    public class Size2D
    {
        public int W { get; set; }
        public int H { get; set; }
    }
    public class LadderObject
    {
        public int ID { get; set; }

        public string Type { get; set; }
        public string SubType { get; set; }
        public string VarName { get; set; }
        public string Address { get; set; }

        public bool Start { get; set; }
        public bool End { get; set; }

        public Point2D Location { get; set; }
        public Size2D Size { get; set; }

        public List<int> InRefs { get; set; }
        public List<int> OutRefs { get; set; }

        public LadderObject()
        {
            InRefs = new List<int>();
            OutRefs = new List<int>();
        }
    }
    public class LadderParser
    {
        private readonly List<LadderObject> _nodes = new List<LadderObject>();

        public LadderParser(string xmlPath)
        {
            Load(xmlPath);
        }

        private Point2D ParsePoint(string s)
        {
            var parts = s.Split(',');
            return new Point2D
            {
                X = int.Parse(parts[0]),
                Y = int.Parse(parts[1])
            };
        }

        private Size2D ParseSize(string s)
        {
            var parts = s.Split(',');
            return new Size2D
            {
                W = int.Parse(parts[0]),
                H = int.Parse(parts[1])
            };
        }

        private void Load(string xmlPath)
        {
            XDocument doc = XDocument.Load(xmlPath);

            foreach (var obj in doc.Descendants("Object"))
            {
                var node = new LadderObject();

                node.ID = (int)obj.Element("ID");
                node.Type = (string)obj.Element("TypeText") ?? (string)obj.Element("ToolBox") ?? "";
                node.SubType = (string)obj.Element("subtype") ?? "";
                node.VarName = (string)obj.Element("VarName") ?? "";
                node.Address = (string)obj.Element("Address") ?? "";

                node.Start = (bool?)obj.Element("Start") ?? false;
                node.End = (bool?)obj.Element("End") ?? false;

                if (obj.Element("Location") != null)
                    node.Location = ParsePoint((string)obj.Element("Location"));

                if (obj.Element("Size") != null)
                    node.Size = ParseSize((string)obj.Element("Size"));

                // OUT references
                foreach (var rid in obj.Descendants("OUT").Descendants("RefID"))
                {
                    int id = (int)rid.Attribute("ID");
                    node.OutRefs.Add(id);
                }

                // IN references
                foreach (var rid in obj.Descendants("IN").Descendants("RefID"))
                {
                    int id = (int)rid.Attribute("ID");
                    node.InRefs.Add(id);
                }

                _nodes.Add(node);
            }
        }

        public List<LadderObject> GetAllObjects()
        {
            return _nodes;
        }

        public List<LadderObject> GetLeftUnconnected()
        {
            HashSet<int> pointedIds = new HashSet<int>();
            foreach (var n in _nodes)
            {
                foreach (var id in n.OutRefs)
                    pointedIds.Add(id);
            }

            List<LadderObject> result = new List<LadderObject>();
            foreach (var n in _nodes)
            {
                if (!pointedIds.Contains(n.ID))
                    result.Add(n);
            }
            return result;
        }

        public List<LadderObject> GetRightUnconnected()
        {
            List<LadderObject> result = new List<LadderObject>();
            foreach (var n in _nodes)
            {
                if (n.OutRefs.Count == 0)
                    result.Add(n);
            }
            return result;
        }
    }

    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
    }
}
