using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace testpropertyGrid
{
    public class MyComponent
    {
        [Editor(typeof(VarCollectionsEditor), typeof(UITypeEditor))]
        public VarCollections InputVars { get; set; } = new VarCollections();
    }
    public partial class Form1 : Form
    {
        private MyComponent _p = new MyComponent();
        public Form1()
        {
           
            InitializeComponent();
            this.propertyGrid1.SelectedObject = _p;
        }
    }
}
