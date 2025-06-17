using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testpropertyGrid
{
    public partial class TableEditorUserControl : UserControl
    {
        public event EventHandler SaveRequested;

        private BindingList<Variable> _variables;
        private VarCollections _originalCollection;

        public VarCollections EditedCollection { get; private set; }
        public TableEditorUserControl(VarCollections collection)
        {
            InitializeComponent();
            _originalCollection = collection;
            EditedCollection = collection.Clone(); // 实现深拷贝
            _variables = new BindingList<Variable>(EditedCollection.Variables);

            SetupDataGridView();
            dataGridView.DataSource = _variables;
        }
        private void SetupDataGridView()
        {
            dataGridView.AutoGenerateColumns = false;
            dataGridView.Columns.Clear();
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;
            // 关闭选择列（如果存在）
            if (dataGridView.Columns.Contains("选择列"))
            {
                dataGridView.Columns.Remove("选择列");
            }
            // 添加列
            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "变量名",
                DataPropertyName = "Name",
                Width = 150
            });

            dataGridView.Columns.Add(new DataGridViewTextBoxColumn
            {
                HeaderText = "值",
                DataPropertyName = "Value",
                Width = 200
            });

            var typeColumn = new DataGridViewComboBoxColumn
            {
                HeaderText = "类型",
                DataPropertyName = "Type",
                DataSource = new List<string> { "string", "int", "bool", "double", "datetime" },
                Width = 100
            };
            dataGridView.Columns.Add(typeColumn);

            //// 添加操作按钮列
            //var actionColumn = new DataGridViewButtonColumn
            //{
            //    HeaderText = "操作",
            //    Text = "删除",
            //    UseColumnTextForButtonValue = true,
            //    Width = 80
            //};
            //dataGridView.Columns.Add(actionColumn);
        }

        //删除按钮处理
        private void dataGridView_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.ColumnIndex == dataGridView.Columns.Cast<DataGridViewColumn>()
                .First(c => c.HeaderText == "操作").Index && e.RowIndex >= 0)
            {
                _variables.RemoveAt(e.RowIndex);
            }
        }

        // 添加新变量
        private void btnAdd_Click(object sender, EventArgs e)
        {
            _variables.Add(new Variable
            {
                Name = $"var_{_variables.Count + 1}",
                Value = "",
                Type = "string"
            });
        }

        // 保存修改
        private void btnSave_Click(object sender, EventArgs e)
        {
            ValidateData();
            SaveRequested?.Invoke(this, EventArgs.Empty);
        }

        // 验证数据
        private void ValidateData()
        {
            // 检查重复名称
            var duplicates = _variables
                .GroupBy(v => v.Name)
                .Where(g => g.Count() > 1)
                .Select(g => g.Key);

            if (duplicates.Any())
            {
                MessageBox.Show($"变量名重复: {string.Join(", ", duplicates)}", "错误");
                return;
            }

            // 类型验证逻辑...
        }
    }
}
