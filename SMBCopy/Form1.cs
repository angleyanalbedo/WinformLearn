using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace SMBCopy
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var srcdir = this.textBox1.Text;
            var dstdir = this.textBox2.Text;
            var exclude = this.textBox3.Text.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries).ToList();
            try
            {
                SMBFileCopy.CopyFilesFromSMB(srcdir, dstdir, exclude);
                MessageBox.Show("文件复制完成！");
            }
            catch (Exception ex)
            {
                MessageBox.Show("发生错误: " + ex.Message);
            }
        }
    }
}
