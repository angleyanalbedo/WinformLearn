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
        private SplitContainer split;
        //气泡提示
        private NotifyIcon _ni;

        public newStructViewer()
        {
            InitializeComponent();
            BuildUI();
            LoadSample();
        }

        protected override void OnFormClosed(FormClosedEventArgs e)
        {
            if (_ni != null)
            {
                _ni.Dispose();
                _ni = null; // Ensure the NotifyIcon is properly disposed and set to null
            }
            base.OnFormClosed(e);
        }

        //protected override void OnLoad(EventArgs e)
        //{
        //    base.OnLoad(e);
        //    split.SplitterDistance = Math.Max(split.Panel1MinSize,
        //                                      split.Width - split.Panel2MinSize);
        //}

        //protected override void OnResize(EventArgs e)
        //{
        //    base.OnResize(e);
        //    split.SplitterDistance = Math.Max(split.Panel1MinSize,
        //                                      split.Width - split.Panel2MinSize);
        //}
        private void BuildUI()
        {
            /* ===== 窗口基础 ===== */
            Text = "结构体浏览器";
            Size = new Size(1000, 650);
            StartPosition = FormStartPosition.CenterScreen;
            MinimumSize = new Size(800, 500);
            BackColor = Color.FromArgb(248, 249, 250);        // 微软浅灰背景

            /* ===== 通知图标 ===== */
            InitNotifyIcon();

            /* ===== 工具栏 ===== */
            tool = new ToolStrip
            {
                BackColor = Color.FromArgb(240, 240, 240),
                GripStyle = ToolStripGripStyle.Hidden,
                RenderMode = ToolStripRenderMode.Professional,
                Font = new Font("Segoe UI", 9F)
            };

            searchBox = new ToolStripTextBox
            {
                Width = 200,
                BorderStyle = BorderStyle.FixedSingle,
                Font = new Font("Segoe UI", 9F)
            };
            searchBox.TextChanged += (s, e) => ApplySearch(searchBox.Text);

            tool.Items.AddRange(new ToolStripItem[]
            {
        new ToolStripLabel("搜索:") { ForeColor = Color.FromArgb(64,64,64)},
        searchBox,
        new ToolStripSeparator(),
        new ToolStripButton("刷新", null, (s,e)=>LoadSample()){ DisplayStyle = ToolStripItemDisplayStyle.ImageAndText, Image = SystemIcons.Application.ToBitmap(), ImageTransparentColor = Color.Magenta },
        new ToolStripButton("复制名称", null, CopySelectedName){ DisplayStyle = ToolStripItemDisplayStyle.ImageAndText, Image = SystemIcons.Shield.ToBitmap(), ImageTransparentColor = Color.Magenta }
            });

            /* ===== 主容器：使用 ToolStripContainer（顶部工具栏 + 内容区） ===== */
            var tsc = new ToolStripContainer { Dock = DockStyle.Fill };
            tsc.TopToolStripPanel.Controls.Add(tool);
            Controls.Add(tsc);

            /* ===== 左侧树 ===== */
            tree = new TreeView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5F),
                ShowLines = true,
                ShowRootLines = false,
                HideSelection = false,
                DrawMode = TreeViewDrawMode.OwnerDrawText   // 下面画高亮
            };
            tree.AfterSelect += Tree_AfterSelect;
            tree.MouseUp += Tree_MouseUp;

            /* ===== 右侧列表 ===== */
            list = new ListView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                BackColor = Color.White,
                Font = new Font("Segoe UI", 9.5F),
                View = View.Details,
                FullRowSelect = true,
                GridLines = false,                // 取消网格线更清爽
                HeaderStyle = ColumnHeaderStyle.Nonclickable
            };
            list.Columns.Add("名称", 150);
            list.Columns.Add("类型", 120);
            list.Columns.Add("偏移", 70, HorizontalAlignment.Right);
            list.Columns.Add("大小", 70, HorizontalAlignment.Right);
            list.Columns.Add("备注", -2);           // -2 = 填满剩余宽度
            list.DoubleClick += List_DoubleClick;
            list.MouseUp += List_MouseUp;

            /* ===== 分割容器 ===== */
            split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                Orientation = Orientation.Vertical,
                //SplitterDistance = 280,          // 合理初始宽度
                //Panel1MinSize = 100,
                //Panel2MinSize = 300,
                BackColor = Color.FromArgb(233, 236, 239)   // 分割线颜色
            };
            split.Panel1.Controls.Add(tree);
            split.Panel2.Controls.Add(list);
            tsc.ContentPanel.Controls.Add(split);

            /* ===== 状态栏 ===== */
            status = new StatusStrip
            {
                BackColor = Color.FromArgb(240, 240, 240),
                Font = new Font("Segoe UI", 9F)
            };
            status.Items.Add(new ToolStripStatusLabel("就绪") { Spring = true });

            tsc.BottomToolStripPanel.Controls.Add(status);

            /* ===== 加载数据 ===== */
            LoadSample();
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
                    // 把成员也挂到结构体节点下
                    foreach (var m in st.Members)
                    {
                        var memberNode = new TreeNode(m.Name) { Tag = m };
                        stNode.Nodes.Add(memberNode);
                    }
                    nsNode.Nodes.Add(stNode);
                }
                tree.Nodes.Add(nsNode);
            }

            tree.ExpandAll();                               // 展开全部
            if (tree.Nodes.Count > 0 && tree.Nodes[0].Nodes.Count > 0)
                tree.SelectedNode = tree.Nodes[0].Nodes[0]; // 默认选中第一个结构体
            tree.EndUpdate();
        }

        /* ===== 选中结构体后刷新列表 ===== */
        private void Tree_AfterSelect(object sender, TreeViewEventArgs e)
        {
            list.BeginUpdate();
            list.Items.Clear();

            switch (e.Node.Tag)
            {
                case StructInfo st:
                    // 显示该结构体的所有成员
                    foreach (var m in st.Members)
                        AddMemberToList(m);
                    status.Items[0].Text = $"结构体: {st.Name}, 大小: {st.Size} bytes, 成员: {st.Members.Count}";
                    break;

                case MemberInfo m:
                    // 只显示这一条成员
                    AddMemberToList(m);
                    status.Items[0].Text = $"成员: {m.Name}  (偏移 {m.Offset}, 大小 {m.Size})";
                    break;

                default:
                    status.Items[0].Text = "就绪";
                    break;
            }

            list.EndUpdate();
        }

        private void AddMemberToList(MemberInfo m)
        {
            var lvi = new ListViewItem(m.Name);
            lvi.SubItems.Add(m.Type);
            lvi.SubItems.Add(m.Offset.ToString());
            lvi.SubItems.Add(m.Size.ToString());
            lvi.SubItems.Add(m.Comment ?? "");
            lvi.Tag = m;
            list.Items.Add(lvi);
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
            if (list.SelectedItems.Count == 0) return;

            string txt = list.SelectedItems[0].Text;
            Clipboard.SetText(txt);

            // 反馈：状态栏 3 秒后自动恢复
            var old = status.Items[0].Text;
            status.Items[0].Text = $"已复制: {txt}";
            var t = new System.Windows.Forms.Timer { Interval = 3000, Enabled = true };
            t.Tick += (_, __) =>
            {
                status.Items[0].Text = old;
                t.Stop();
                t.Dispose();
            };

            // 显示气泡，3 秒后自动消失
            _ni.ShowBalloonTip(
                timeout: 3000,
                tipTitle: "复制成功",
                tipText: $"已复制到剪贴板：{txt}",
                tipIcon: ToolTipIcon.Info);
        }

        /* ===== 工具栏复制 ===== */
        private void CopySelectedName(object sender, EventArgs e)
        {
            if (tree.SelectedNode?.Tag is StructInfo st)
                Clipboard.SetText(st.Name);
            else if (list.SelectedItems.Count > 0)
                Clipboard.SetText(list.SelectedItems[0].Text);
        }

        // ListView右键
        private void List_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var hit = list.HitTest(e.Location);
            if (hit.Item == null) return;          // 没点到行

            // 选中当前行（可选）
            hit.Item.Selected = true;

            // 动态创建菜单
            var cms = new ContextMenuStrip();
            cms.Items.Add("复制名称", null, (_, __) =>
                Clipboard.SetText(hit.Item.Text));
            cms.Items.Add("复制类型", null, (_, __) =>
                Clipboard.SetText(hit.Item.SubItems[1].Text));
            cms.Items.Add("复制偏移", null, (_, __) =>
                Clipboard.SetText(hit.Item.SubItems[2].Text));
            cms.Items.Add("复制大小", null, (_, __) =>
                Clipboard.SetText(hit.Item.SubItems[3].Text));

            // 在鼠标位置弹出
            cms.Show(list, e.Location);
        }

        // TreeView右键
        private void Tree_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button != MouseButtons.Right) return;

            var hit = tree.HitTest(e.Location);
            if (hit.Node == null) return;

            tree.SelectedNode = hit.Node;   // 修复：使用 TreeView 的 SelectedNode 属性来设置选中节点

            // 仅复制名称
            var cms = new ContextMenuStrip();
            cms.Items.Add("复制名称", null, (_, __) =>
            {
                string text;
                if (hit.Node.Tag is StructInfo si)
                    text = si.Name;
                else if (hit.Node.Tag is MemberInfo mi)
                    text = mi.Name;
                else
                    text = hit.Node.Text;

                Clipboard.SetText(text);
                Clipboard.SetText(text);
            });

            cms.Show(tree, e.Location);
        }

        /*===气泡提示初始化==*/
        private void InitNotifyIcon()
        {
            _ni = new NotifyIcon
            {
                Visible = true,
                Icon = SystemIcons.Application,   // 用系统图标即可
                Text = "结构体查看器"
            };
        }

    }
}
