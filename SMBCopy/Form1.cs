using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;

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

            // 配置BackgroundWorker
            BackgroundWorker worker = new BackgroundWorker
            {
                WorkerReportsProgress = true,
                WorkerSupportsCancellation = false
            };

            worker.DoWork += (s, args) =>
            {
                try
                {
                    SMBFileCopy.CopyFilesFromSMB(srcdir, dstdir, exclude, progress => worker.ReportProgress(progress));
                }
                catch (Exception ex)
                {
                    MessageBox.Show("发生错误: " + ex.Message);
                }
            };

            worker.ProgressChanged += (s, args) =>
            {
                progressBar1.Value = args.ProgressPercentage;
            };

            worker.RunWorkerCompleted += (s, args) =>
            {
                MessageBox.Show("文件复制完成！");
            };

            worker.RunWorkerAsync();
        }
    }
}
