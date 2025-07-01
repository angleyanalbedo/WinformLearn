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

        public override string ToString()
        {
            return $"{Type} {Name} (偏移: {Offset}, 大小: {Size})";
        }
    }
    public partial class StructViewer : Form
    {
        public StructViewer()
        {
            InitializeComponent();
            InitializeUI();
            LoadSampleData();
        }
        private void InitializeUI()
        {
            this.Text = "结构体成员查看器";
            this.Size = new System.Drawing.Size(800, 600);

            // 创建分割容器
            SplitContainer splitContainer = new SplitContainer();
            splitContainer.Dock = DockStyle.Fill;
            splitContainer.Orientation = Orientation.Horizontal;
            this.Controls.Add(splitContainer);

            // 上部面板 - 树形视图
            TreeView treeView = new TreeView();
            treeView.Dock = DockStyle.Fill;
            treeView.AfterSelect += TreeView_AfterSelect;
            splitContainer.Panel1.Controls.Add(treeView);
            this.StructTreeView = treeView;

            // 下部面板 - 详细信息
            PropertyGrid propertyGrid = new PropertyGrid();
            propertyGrid.Dock = DockStyle.Fill;
            splitContainer.Panel2.Controls.Add(propertyGrid);
            this.DetailPropertyGrid = propertyGrid;

            // 底部工具栏
            StatusStrip statusStrip = new StatusStrip();
            statusStrip.Items.Add(new ToolStripStatusLabel("就绪"));
            this.Controls.Add(statusStrip);
            statusStrip.Dock = DockStyle.Bottom;

            // 添加搜索框
            ToolStrip toolStrip = new ToolStrip();
            var searchBox = new ToolStripTextBox();
            searchBox.TextChanged += SearchBox_TextChanged;
            toolStrip.Items.Add(new ToolStripLabel("搜索:"));
            toolStrip.Items.Add(searchBox);
            this.Controls.Add(toolStrip);
            toolStrip.Dock = DockStyle.Top;

            // 添加上下文菜单
            ContextMenuStrip contextMenu = new ContextMenuStrip();
            contextMenu.Items.Add("复制名称", null, (s, e) =>
            {
                if (StructTreeView.SelectedNode?.Tag != null)
                {
                    string text = StructTreeView.SelectedNode.Tag is StructInfo si ? si.Name :
                                 StructTreeView.SelectedNode.Tag is MemberInfo mi ? mi.Name :
                                 StructTreeView.SelectedNode.Text;
                    Clipboard.SetText(text);
                }
            });

            contextMenu.Items.Add("定位到文件", null, (s, e) =>
            {
                if (StructTreeView.SelectedNode?.Tag is StructInfo si)
                {
                    MessageBox.Show($"尝试打开: {si.FileLocation}", "定位文件");
                    // 实际应用中这里可以调用编辑器打开文件
                }
            });

            StructTreeView.ContextMenuStrip = contextMenu;
        }
        private void SearchBox_TextChanged(object sender, EventArgs e)
        {
            var searchText = ((ToolStripTextBox)sender).Text.ToLower();

            foreach (TreeNode namespaceNode in StructTreeView.Nodes)
            {
                bool namespaceVisible = false;

                foreach (TreeNode structNode in namespaceNode.Nodes)
                {
                    bool structVisible = structNode.Text.ToLower().Contains(searchText);

                    foreach (TreeNode memberNode in structNode.Nodes)
                    {
                        bool memberVisible = memberNode.Text.ToLower().Contains(searchText);
                        memberNode.BackColor = memberVisible ? Color.Yellow : Color.White; // Highlight matching nodes  
                        structVisible = structVisible || memberVisible;
                    }

                    structNode.BackColor = structVisible ? Color.Yellow : Color.White; // Highlight matching nodes  
                    namespaceVisible = namespaceVisible || structVisible;
                }

                namespaceNode.BackColor = namespaceVisible ? Color.Yellow : Color.White; // Highlight matching nodes  
            }
        }

        public TreeView StructTreeView { get; private set; }
        public PropertyGrid DetailPropertyGrid { get; private set; }

        private void LoadSampleData()
        {
            // 创建一些示例结构体
            var structs = new List<StructInfo>
        {
            new StructInfo
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
            },
            new StructInfo
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
            }
        };

            // 构建树形结构
            var namespaceNodes = new Dictionary<string, TreeNode>();

            foreach (var structInfo in structs)
            {
                // 按命名空间分组
                if (!namespaceNodes.TryGetValue(structInfo.Namespace, out var namespaceNode))
                {
                    namespaceNode = new TreeNode(structInfo.Namespace);
                    namespaceNodes[structInfo.Namespace] = namespaceNode;
                    StructTreeView.Nodes.Add(namespaceNode);
                }

                // 添加结构体节点
                var structNode = new TreeNode(structInfo.Name);
                structNode.Tag = structInfo;
                namespaceNode.Nodes.Add(structNode);

                // 添加成员节点
                foreach (var member in structInfo.Members)
                {
                    var memberNode = new TreeNode(member.ToString());
                    memberNode.Tag = member;
                    structNode.Nodes.Add(memberNode);
                }
            }

            // 展开所有节点
            StructTreeView.ExpandAll();
        }
        private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is StructInfo structInfo)
            {
                // 显示结构体详细信息
                DetailPropertyGrid.SelectedObject = new
                {
                    Name = structInfo.Name,
                    Namespace = structInfo.Namespace,
                    Location = structInfo.FileLocation,
                    Size = $"{structInfo.Size} bytes",
                    MemberCount = structInfo.Members.Count
                };
            }
            else if (e.Node.Tag is MemberInfo memberInfo)
            {
                // 显示成员详细信息
                DetailPropertyGrid.SelectedObject = memberInfo;
            }
        }

    }

}
