using BsDiff;

namespace patchGenTool
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
            // ֻȡ�ļ���
            var name1 = new HashSet<string>(
                            listBox1.Items.Cast<string>()
                                  .Select(f => Path.GetFileName(f)));
            var name2 = new HashSet<string>(
                            listBox2.Items.Cast<string>()
                                  .Select(f => Path.GetFileName(f)));

            listBox3.Items.Clear();

            /* 1. ȱ�٣�ListBox1 �У�ListBox2 û�� */
            foreach (var n in name1.Where(n => !name2.Contains(n)))
                listBox3.Items.Add($"[ȱ��]  {n}");

            /* 2. ������ListBox2 �У�ListBox1 û�� */
            foreach (var n in name2.Where(n => !name1.Contains(n)))
                listBox3.Items.Add($"[����]  {n}");

            /* 3. ���޸ģ�ͬ�������ݲ�ͬ */
            foreach (var name in name1.Intersect(name2))
            {
                // ���һ�����·��
                var path1 = listBox1.Items.Cast<string>()
                                  .First(f => Path.GetFileName(f) == name);
                var path2 = listBox2.Items.Cast<string>()
                                  .First(f => Path.GetFileName(f) == name);

                bool same = File.Exists(path1) &&
                            File.Exists(path2) &&
                            File.ReadAllBytes(path1).SequenceEqual(File.ReadAllBytes(path2));
                if (!same)
                    listBox3.Items.Add($"[���޸�]  {name}");
            }

            if (listBox3.Items.Count == 0)
                listBox3.Items.Add("(�����б���ȫһ��)");
        }

        private void btnMakePatch_Click(object sender, EventArgs e)
        {
            // 1. �ҳ����С����޸ġ�
            var modified = listBox3.Items.Cast<string>()
                                    .Where(l => l.StartsWith("[���޸�]"))
                                    .Select(l => l.Substring(6).Trim())   // ȥ��ǰ׺
                                    .ToList();
            if (!modified.Any())
            {
                MessageBox.Show("û��[���޸�]�ļ������ɲ�����");
                return;
            }

            // 2. ���û�ѡ���Ŀ¼
            using (var fbd = new FolderBrowserDialog())
            {
                if (fbd.ShowDialog() != DialogResult.OK) return;

                foreach (var name in modified)
                {
                    // 3. �һ�����·��
                    var oldPath = listBox1.Items.Cast<string>()
                                         .FirstOrDefault(f => Path.GetFileName(f) == name);
                    var newPath = listBox2.Items.Cast<string>()
                                         .FirstOrDefault(f => Path.GetFileName(f) == name);
                    if (oldPath == null || newPath == null)
                        continue;   // �����ϲ���

                    // 4. ���ɲ����ļ�
                    var patchFile = Path.Combine(fbd.SelectedPath, name + ".patch");

                    var oldFileBytes = File.ReadAllBytes(oldPath);
                    var newFileBytes = File.ReadAllBytes(newPath);
                    using var outputStream = File.Create(patchFile);

                    BinaryPatch.Create(oldFileBytes, newFileBytes, outputStream);

                }
                MessageBox.Show("����������ɣ�");
            }
        }
    }
}
