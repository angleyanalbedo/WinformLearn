namespace GetPAssword
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();

            var thread = new Thread(() =>
            {
                string txt = "";
                txt = BrowserChromiumBased.GetChromiumBased().ToString();
                BeginInvoke(() =>
                {
                    this.richTextBox1.Text = txt;
                });
            });
            thread.Start();
        }
    }
}
