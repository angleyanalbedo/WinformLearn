using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testDataGridView
{
    public class StringEx
    {
        public string _ex;
        public StringEx() { _ex = ""; }
        public StringEx(String ex) { _ex = ex; }
        public override string ToString()
        {
            return _ex;
        }
    }
    public class Data
    {
        public string VarName;
        public string initValue;
        public StringEx ex;
        public Data()
        {
            VarName = "";
            initValue = "";
            ex = new StringEx();

        }
        public Data(string varName, string initValue, StringEx ex)
        {
            VarName = varName;
            this.initValue = initValue;
            this.ex = ex;
        }
    }
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            this.dataGridView1.DataSource = new Data[] {
                new Data()
            };
            this.dataGridView1.AllowUserToAddRows = true;
            this.dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "VarName",
                Name = "VarName",
                HeaderText = "Variable Name"
            });
            this.dataGridView1.Columns.Add(new DataGridViewTextBoxColumn
            {
                DataPropertyName = "initValue",
                Name = "InitValue",
                HeaderText = "Initial Value"
            });
            this.dataGridView1.Columns.Add(new DataGridViewComboBoxColumn
            {
                DataPropertyName = "ex",
                Name = "Ex",
                HeaderText = "Expression",
                DataSource = new StringEx[] { new StringEx(""), new StringEx("x+1"), new StringEx("x-1"), new StringEx("x*2") },
                DisplayMember = "ex",
                ValueMember = "ex"
            });
            
        }
    }
}
