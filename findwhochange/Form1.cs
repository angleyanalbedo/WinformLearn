using System.Text;
using System.Text.RegularExpressions;

namespace findwhochange
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
            InitListBox(listBox1);
            InitListBox(listBox2);
        }

        /* ---------- 拖放通用 ---------- */
        private void InitListBox(ListBox lb)
        {
            lb.AllowDrop = true;
            lb.DragEnter += (s, e) =>
            {
                e.Effect = e.Data.GetDataPresent(DataFormats.FileDrop)
                           ? DragDropEffects.Copy
                           : DragDropEffects.None;
            };
            lb.DragDrop += Lb_DragDrop;
        }

        /* ---------- 拖完立即处理 ---------- */
        private void Lb_DragDrop(object sender, DragEventArgs e)
        {
            var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            var lb = (ListBox)sender;

            // 1. 把“文件全路径”写进对应 ListBox（已去重）
            var files = ExpandFiles(paths);          // 文件夹→文件
            foreach (var f in files)
                if (!lb.Items.Contains(f))
                    lb.Items.Add(f);

            // 2. 只要 ListBox2 有变化，立即比较
            if (lb == listBox2)
                CompareNow();
        }

        /* ---------- 递归展开文件 ---------- */
        private List<string> ExpandFiles(string[] paths)
        {
            var list = new List<string>();
            foreach (var p in paths)
            {
                if (File.Exists(p))          // 单个文件
                    list.Add(p);
                else if (Directory.Exists(p)) // 文件夹
                    list.AddRange(Directory.EnumerateFiles(p, "*", SearchOption.AllDirectories));
            }
            return list;
        }

        /* ---------- 核心比较 ---------- */
        private void CompareNow()
        {
            // 只取文件名
            var name1 = new HashSet<string>(
                            listBox1.Items.Cast<string>()
                                  .Select(f => Path.GetFileName(f)));
            var name2 = new HashSet<string>(
                            listBox2.Items.Cast<string>()
                                  .Select(f => Path.GetFileName(f)));

            listBox3.Items.Clear();

            /* 1. 缺少：ListBox1 有，ListBox2 没有 */
            foreach (var n in name1.Where(n => !name2.Contains(n)))
                listBox3.Items.Add($"[缺少]  {n}");

            /* 2. 新增：ListBox2 有，ListBox1 没有 */
            foreach (var n in name2.Where(n => !name1.Contains(n)))
                listBox3.Items.Add($"[新增]  {n}");

            /* 3. 已修改：同名但内容不同 */
            foreach (var name in name1.Intersect(name2))
            {
                // 先找回完整路径
                var path1 = listBox1.Items.Cast<string>()
                                  .First(f => Path.GetFileName(f) == name);
                var path2 = listBox2.Items.Cast<string>()
                                  .First(f => Path.GetFileName(f) == name);

                bool same = File.Exists(path1) &&
                            File.Exists(path2) &&
                            File.ReadAllBytes(path1).SequenceEqual(File.ReadAllBytes(path2));
                if (!same)
                    listBox3.Items.Add($"[已修改]  {name}");
            }

            if (listBox3.Items.Count == 0)
                listBox3.Items.Add("(两个列表完全一致)");
        }
        private void btnExportCsv_Click(object sender, EventArgs e)
        {
            if (listBox3.Items.Count == 0)
            {
                MessageBox.Show("没有可导出的数据！");
                return;
            }

            using (var sfd = new SaveFileDialog())
            {
                sfd.Filter = "CSV 文件|*.csv";
                sfd.FileName = $"CompareResult_{DateTime.Now:yyyyMMdd_HHmmss}.csv";
                if (sfd.ShowDialog() != DialogResult.OK) return;

                try
                {
                    // 写 UTF-8 带 BOM，Excel 打开不乱码
                    using (var sw = new StreamWriter(sfd.FileName, false, Encoding.UTF8))
                    {
                        sw.WriteLine("状态,文件名");          // 表头
                        foreach (var line in listBox3.Items.Cast<string>())
                        {
                            // 解析前缀 [状态] 文件名
                            var match = Regex.Match(line, @"^\[(.+?)\]\s+(.+)$");
                            if (match.Success)
                                sw.WriteLine($"{match.Groups[1].Value},{match.Groups[2].Value}");
                            else          // 兜底，如“完全一致”
                                sw.WriteLine($",{line}");
                        }
                    }
                    MessageBox.Show("导出完成！");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("导出失败：" + ex.Message);
                }
            }
        }
    }
}
