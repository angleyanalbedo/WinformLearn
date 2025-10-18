namespace patchGenTool
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            listBox1 = new ListBox();
            listBox2 = new ListBox();
            listBox3 = new ListBox();
            btnMakePatch = new Button();
            SuspendLayout();
            // 
            // listBox1
            // 
            listBox1.FormattingEnabled = true;
            listBox1.ItemHeight = 24;
            listBox1.Location = new Point(28, 20);
            listBox1.Name = "listBox1";
            listBox1.Size = new Size(514, 244);
            listBox1.TabIndex = 0;
            // 
            // listBox2
            // 
            listBox2.FormattingEnabled = true;
            listBox2.ItemHeight = 24;
            listBox2.Location = new Point(637, 20);
            listBox2.Name = "listBox2";
            listBox2.Size = new Size(514, 244);
            listBox2.TabIndex = 1;
            // 
            // listBox3
            // 
            listBox3.FormattingEnabled = true;
            listBox3.ItemHeight = 24;
            listBox3.Location = new Point(28, 290);
            listBox3.Name = "listBox3";
            listBox3.Size = new Size(1123, 244);
            listBox3.TabIndex = 2;
            // 
            // btnMakePatch
            // 
            btnMakePatch.Location = new Point(953, 550);
            btnMakePatch.Name = "btnMakePatch";
            btnMakePatch.Size = new Size(198, 34);
            btnMakePatch.TabIndex = 3;
            btnMakePatch.Text = "生成补丁文件";
            btnMakePatch.UseVisualStyleBackColor = true;
            btnMakePatch.Click += btnMakePatch_Click;
            // 
            // Form1
            // 
            AutoScaleDimensions = new SizeF(11F, 24F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(1188, 647);
            Controls.Add(btnMakePatch);
            Controls.Add(listBox3);
            Controls.Add(listBox2);
            Controls.Add(listBox1);
            Name = "Form1";
            Text = "Form1";
            ResumeLayout(false);
        }

        #endregion

        private ListBox listBox1;
        private ListBox listBox2;
        private ListBox listBox3;
        private Button btnMakePatch;
    }
}
