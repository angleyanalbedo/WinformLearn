using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace StructViewer
{
   

    public partial class newStructViewer : Form
    {
        /* ===== 数据模型 ===== */
        public class StructInfo
        {
            public string Name { get; set; }
            public string Namespace { get; set; }
            public string FileLocation { get; set; }
            public int Size { get; set; }
            public List<MemberInfo> Members { get; } = new List<MemberInfo>();
        }

        public class MemberInfo
        {
            public string Name { get; set; }
            public string Type { get; set; }
            public int Offset { get; set; }
            public int Size { get; set; }
            public string Comment { get; set; }
        }

        /* ===== 控件 ===== */
        private TreeView tree;
        private ListView list;
        private ToolStrip tool;
        private StatusStrip status;
        private ToolStripTextBox searchBox;

        public newStructViewer()
        {
            InitializeComponent();
            BuildUI();
            LoadSample();
        }

        private void BuildUI()
        {
            Text = "结构体浏览器（文件管理器风格）";
            Size = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;

            /* 工具栏 */
            tool = new ToolStrip
            {
                GripStyle = ToolStripGripStyle.Hidden,
                RenderMode = ToolStripRenderMode.System
            };
            searchBox = new ToolStripTextBox { Width = 180 };
            searchBox.TextChanged += (s, e) => ApplySearch(searchBox.Text);
            tool.Items.AddRange(new ToolStripItem[]
            {
                new ToolStripLabel("搜索:"),
                searchBox,
                new ToolStripSeparator(),
                new ToolStripButton("刷新", null, (s,e)=>LoadSample()){ DisplayStyle = ToolStripItemDisplayStyle.Text },
                new ToolStripButton("复制名称", null, CopySelectedName){ DisplayStyle = ToolStripItemDisplayStyle.Text }
            });
            

            /* 主分割容器 */
            var split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                SplitterDistance = 50
            };

            /* 把工具栏和 split 都放到一个 Panel 里 */
            var mainPanel = new Panel { Dock = DockStyle.Fill };
            mainPanel.Controls.Add(split);   // split 填充满 Panel
            Controls.Add(mainPanel);         // 先加 Panel
            Controls.Add(tool);              // 再加工具栏，工具栏会自然停靠在顶部

            /* 左侧树 */
            tree = new TreeView
            {
                Dock = DockStyle.Fill,
                HideSelection = false,
                ShowLines = true,
                ShowRootLines = false
            };
            tree.AfterSelect += Tree_AfterSelect;
            split.Panel1.Controls.Add(tree);

            /* 右侧列表 */
            list = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                MultiSelect = false
            };
            list.Columns.Add("名称", 120);
            list.Columns.Add("类型", 120);
            list.Columns.Add("偏移", 60, HorizontalAlignment.Right);
            list.Columns.Add("大小", 60, HorizontalAlignment.Right);
            list.Columns.Add("备注", 200);
            list.DoubleClick += List_DoubleClick;
            split.Panel2.Controls.Add(list);

            /* 状态栏 */
            status = new StatusStrip();
            status.Items.Add(new ToolStripStatusLabel("就绪"));
            Controls.Add(status);
        }

        /* ===== 示例数据 ===== */
        private List<StructInfo> sampleData = new List<StructInfo>();

        private void LoadSample()
        {
            sampleData.Clear();
            sampleData.Add(new StructInfo
            {
                Name = "Person",
                Namespace = "System.Models",
                FileLocation = "Models/Person.cs",
                Size = 64,
                Members =
                {
                    new MemberInfo { Name = "Name", Type = "string", Offset = 0, Size = 32 },
                    new MemberInfo { Name = "Age", Type = "int", Offset = 32, Size = 4 },
                    new MemberInfo { Name = "Height", Type = "float", Offset = 36, Size = 4 }
                }
            });
            sampleData.Add(new StructInfo
            {
                Name = "Config",
                Namespace = "System.Configuration",
                FileLocation = "Config/SystemConfig.cs",
                Size = 24,
                Members =
                {
                    new MemberInfo { Name = "Timeout", Type = "uint", Offset = 0, Size = 4 },
                    new MemberInfo { Name = "MaxConnections", Type = "int", Offset = 4, Size = 4 }
                }
            });

            FillTree();
        }

        /* ===== 填充树 ===== */
        private void FillTree()
        {
            tree.BeginUpdate();
            tree.Nodes.Clear();

            foreach (var g in sampleData.GroupBy(s => s.Namespace))
            {
                var nsNode = new TreeNode(g.Key) { Tag = g.Key };
                foreach (var st in g)
                {
                    var stNode = new TreeNode(st.Name) { Tag = st };
                    nsNode.Nodes.Add(stNode);
                }
                tree.Nodes.Add(nsNode);
            }

            tree.ExpandAll();
            if (tree.Nodes.Count > 0)
                tree.SelectedNode = tree.Nodes[0].Nodes[0];
            tree.EndUpdate();
        }

        /* ===== 选中结构体后刷新列表 ===== */
        private void Tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is StructInfo st)
            {
                list.BeginUpdate();
                list.Items.Clear();

                foreach (var m in st.Members)
                {
                    var lvi = new ListViewItem(m.Name);
                    lvi.SubItems.Add(m.Type);
                    lvi.SubItems.Add(m.Offset.ToString());
                    lvi.SubItems.Add(m.Size.ToString());
                    lvi.SubItems.Add(m.Comment ?? "");
                    lvi.Tag = m;
                    list.Items.Add(lvi);
                }

                /* 更新状态栏 */
                status.Items[0].Text = $"结构体: {st.Name}, 大小: {st.Size} bytes, 成员: {st.Members.Count}";
                list.EndUpdate();
            }
        }

        /* ===== 搜索高亮 ===== */
        private void ApplySearch(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                foreach (TreeNode ns in tree.Nodes)
                    foreach (TreeNode st in ns.Nodes)
                        st.BackColor = SystemColors.Window;
                return;
            }

            var low = text.ToLower();
            foreach (TreeNode ns in tree.Nodes)
            {
                foreach (TreeNode st in ns.Nodes)
                {
                    st.BackColor =
                        st.Text.ToLower().Contains(low) ||
                        ((StructInfo)st.Tag).Members.Any(m =>
                            m.Name.ToLower().Contains(low) ||
                            m.Type.ToLower().Contains(low))
                        ? Color.Yellow : SystemColors.Window;
                }
            }
        }

        /* ===== 双击成员复制名称 ===== */
        private void List_DoubleClick(object sender, EventArgs e)
        {
            if (list.SelectedItems.Count > 0)
                Clipboard.SetText(list.SelectedItems[0].Text);
        }

        /* ===== 工具栏复制 ===== */
        private void CopySelectedName(object sender, EventArgs e)
        {
            if (tree.SelectedNode?.Tag is StructInfo st)
                Clipboard.SetText(st.Name);
            else if (list.SelectedItems.Count > 0)
                Clipboard.SetText(list.SelectedItems[0].Text);
        }
    }
}
