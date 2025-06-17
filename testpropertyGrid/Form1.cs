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
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Person
    {
        public string FirstName { get; set; } = "John";

        [DisplayName("姓氏")]
        public string LastName { get; set; } = "Doe";

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Address Address { get; set; } = new Address();

        // 控制属性窗口显示内容
        public override string ToString() => $"{FirstName} {LastName}";
    }

    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Address
    {
        public string Street { get; set; } = "123 Main St";
        public string City { get; set; } = "Anytown";

        [Browsable(false)] // 隐藏此属性
        public string ZipCode { get; set; } = "12345";

        public override string ToString() => $"{Street}, {City}";
    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Xprop
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
    }
    [TypeConverter(typeof(ExpandableObjectConverter))]
    public class Vars : List<Xprop>
    {
        public List<Xprop> List { get; set; }
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Xprop xprop { get; set; }
    }
        public class MyComponent
    {
        [Editor(typeof(VarCollectionsEditor), typeof(UITypeEditor))]
        public VarCollections InputVars { get; set; } = new VarCollections();
        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Vars OutputVars { get; set; } = new Vars();

        [TypeConverter(typeof(ExpandableObjectConverter))]
        public Person Person { get; set; } = new Person();

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
