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
            var set1 = new HashSet<string>(listBox1.Items.Cast<string>());
            var onlyIn2 = listBox2.Items.Cast<string>()
                                .Where(f => !set1.Contains(f))
                                .ToArray();

            listBox3.Items.Clear();
            if (onlyIn2.Length == 0)
                listBox3.Items.Add("(没有不同的文件)");
            else
                listBox3.Items.AddRange(onlyIn2);
        }
    }
}
