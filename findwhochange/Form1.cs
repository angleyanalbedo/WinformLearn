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

        /* ---------- �Ϸ�ͨ�� ---------- */
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

        /* ---------- ������������ ---------- */
        private void Lb_DragDrop(object sender, DragEventArgs e)
        {
            var paths = (string[])e.Data.GetData(DataFormats.FileDrop);
            var lb = (ListBox)sender;

            // 1. �ѡ��ļ�ȫ·����д����Ӧ ListBox����ȥ�أ�
            var files = ExpandFiles(paths);          // �ļ��С��ļ�
            foreach (var f in files)
                if (!lb.Items.Contains(f))
                    lb.Items.Add(f);

            // 2. ֻҪ ListBox2 �б仯�������Ƚ�
            if (lb == listBox2)
                CompareNow();
        }

        /* ---------- �ݹ�չ���ļ� ---------- */
        private List<string> ExpandFiles(string[] paths)
        {
            var list = new List<string>();
            foreach (var p in paths)
            {
                if (File.Exists(p))          // �����ļ�
                    list.Add(p);
                else if (Directory.Exists(p)) // �ļ���
                    list.AddRange(Directory.EnumerateFiles(p, "*", SearchOption.AllDirectories));
            }
            return list;
        }

        /* ---------- ���ıȽ� ---------- */
        private void CompareNow()
        {
            var set1 = new HashSet<string>(listBox1.Items.Cast<string>());
            var onlyIn2 = listBox2.Items.Cast<string>()
                                .Where(f => !set1.Contains(f))
                                .ToArray();

            listBox3.Items.Clear();
            if (onlyIn2.Length == 0)
                listBox3.Items.Add("(û�в�ͬ���ļ�)");
            else
                listBox3.Items.AddRange(onlyIn2);
        }
    }
}
