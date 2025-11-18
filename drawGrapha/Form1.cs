using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace drawGrapha
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            var elementarray = new List<List<Element>>
            {
                new List<Element>   // 第 0 行
                {
                    new Element
                    {
                        ID       = "Start",
                        TypeText = "Button",
                        ORefID   = "Box1"
                    },
                    new Element
                    {
                        ID       = "Box1",
                        TypeText = "TextBox",
                        IRefID   = "Start",
                        ORefID   = "End"
                    }
                },
                new List<Element>   // 第 1 行
                {
                    new Element
                    {
                        ID       = "End",
                        TypeText = "Label",
                        IRefID   = "Box1"
                    }
                }
            };
            GraphViewer.ShowGraph(elementarray);
        }
    }
}
