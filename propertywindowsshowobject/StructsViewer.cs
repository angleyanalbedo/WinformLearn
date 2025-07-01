using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace propertywindowsshowobject
{
    // 包装Decl列表为可展开属性
    public class DeclListWrapper : ICustomTypeDescriptor
    {
        private List<XmlOperation.Decl> _decls;

        public DeclListWrapper(List<XmlOperation.Decl> decls)
        {
            _decls = decls;
        }

        public PropertyDescriptorCollection GetProperties()
        {
            var props = new List<PropertyDescriptor>();
            for (int i = 0; i < _decls.Count; i++)
            {
                props.Add(new DeclPropertyDescriptor(_decls, i));
            }
            return new PropertyDescriptorCollection(props.ToArray());
        }

        // 其余ICustomTypeDescriptor接口成员实现（可用默认实现）
        public AttributeCollection GetAttributes() => AttributeCollection.Empty;
        public string GetClassName() => null;
        public string GetComponentName() => null;
        public TypeConverter GetConverter() => null;
        public EventDescriptor GetDefaultEvent() => null;
        public PropertyDescriptor GetDefaultProperty() => null;
        public object GetEditor(Type editorBaseType) => null;
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => EventDescriptorCollection.Empty;
        public EventDescriptorCollection GetEvents() => EventDescriptorCollection.Empty;
        public object GetPropertyOwner(PropertyDescriptor pd) => this;
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes) => GetProperties();
    }

    // 每个Decl作为一个属性
    public class DeclPropertyDescriptor : PropertyDescriptor
    {
        private List<XmlOperation.Decl> _decls;
        private int _index;

        public DeclPropertyDescriptor(List<XmlOperation.Decl> decls, int index)
            : base($"{index}_{decls[index].Name}", null)
        {
            _decls = decls;
            _index = index;
        }

        public override Type PropertyType => typeof(XmlOperation.Decl);
        public override void SetValue(object component, object value)
        {
            _decls[_index] = (XmlOperation.Decl)value;
        }
        public override object GetValue(object component) => _decls[_index];
        public override bool IsReadOnly => false;
        public override Type ComponentType => typeof(DeclListWrapper);
        public override bool CanResetValue(object component) => false;
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) => false;
    }

    // Struct包装器，Members属性返回DeclListWrapper
    public class StructWrapper : ICustomTypeDescriptor
    {
        private XmlOperation.Struct _struct;

        public StructWrapper(XmlOperation.Struct s)
        {
            _struct = s;
        }

        public PropertyDescriptorCollection GetProperties()
        {
            var props = new List<PropertyDescriptor>
            {
                TypeDescriptor.CreateProperty(typeof(StructWrapper), "Name", typeof(string)),
                new MembersPropertyDescriptor(_struct)
            };
            return new PropertyDescriptorCollection(props.ToArray());
        }

        // 其余ICustomTypeDescriptor接口成员实现（可用默认实现）
        public AttributeCollection GetAttributes() => AttributeCollection.Empty;
        public string GetClassName() => null;
        public string GetComponentName() => null;
        public TypeConverter GetConverter() => null;
        public EventDescriptor GetDefaultEvent() => null;
        public PropertyDescriptor GetDefaultProperty() => null;
        public object GetEditor(Type editorBaseType) => null;
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => EventDescriptorCollection.Empty;
        public EventDescriptorCollection GetEvents() => EventDescriptorCollection.Empty;
        public object GetPropertyOwner(PropertyDescriptor pd) => this;
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes) => GetProperties();

        // 供PropertyGrid使用
        public string Name
        {
            get => _struct.Name;
            set => _struct.Name = value;
        }
    }

    // Members属性描述符
    public class MembersPropertyDescriptor : PropertyDescriptor
    {
        private XmlOperation.Struct _struct;

        public MembersPropertyDescriptor(XmlOperation.Struct s)
            : base("Members", null)
        {
            _struct = s;
        }

        public override Type PropertyType => typeof(DeclListWrapper);
        public override void SetValue(object component, object value) { }
        public override object GetValue(object component) => new DeclListWrapper(_struct.Members);
        public override bool IsReadOnly => true;
        public override Type ComponentType => typeof(StructWrapper);
        public override bool CanResetValue(object component) => false;
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) => false;
    }

    // StructList包装器
    public class StructListWrapper : ICustomTypeDescriptor
    {
        private List<XmlOperation.Struct> _structs;

        public StructListWrapper(List<XmlOperation.Struct> structs)
        {
            _structs = structs;
        }

        public PropertyDescriptorCollection GetProperties()
        {
            var props = new List<PropertyDescriptor>();
            for (int i = 0; i < _structs.Count; i++)
            {
                props.Add(new StructPropertyDescriptor(_structs, i));
            }
            return new PropertyDescriptorCollection(props.ToArray());
        }

        // 其余ICustomTypeDescriptor接口成员实现（可用默认实现）
        public AttributeCollection GetAttributes() => AttributeCollection.Empty;
        public string GetClassName() => null;
        public string GetComponentName() => null;
        public TypeConverter GetConverter() => null;
        public EventDescriptor GetDefaultEvent() => null;
        public PropertyDescriptor GetDefaultProperty() => null;
        public object GetEditor(Type editorBaseType) => null;
        public EventDescriptorCollection GetEvents(Attribute[] attributes) => EventDescriptorCollection.Empty;
        public EventDescriptorCollection GetEvents() => EventDescriptorCollection.Empty;
        public object GetPropertyOwner(PropertyDescriptor pd) => this;
        public PropertyDescriptorCollection GetProperties(Attribute[] attributes) => GetProperties();
    }

    // 每个Struct作为一个属性，返回StructWrapper
    public class StructPropertyDescriptor : PropertyDescriptor
    {
        private List<XmlOperation.Struct> _structs;
        private int _index;

        public StructPropertyDescriptor(List<XmlOperation.Struct> structs, int index)
            : base(structs[index].Name, null)
        {
            _structs = structs;
            _index = index;
        }

        public override Type PropertyType => typeof(StructWrapper);
        public override void SetValue(object component, object value)
        {
            if (value is StructWrapper wrapper)
                _structs[_index] = (XmlOperation.Struct)wrapper.GetPropertyOwner(null);
        }
        public override object GetValue(object component) => new StructWrapper(_structs[_index]);
        public override bool IsReadOnly => false;
        public override Type ComponentType => typeof(StructListWrapper);
        public override bool CanResetValue(object component) => false;
        public override void ResetValue(object component) { }
        public override bool ShouldSerializeValue(object component) => false;
    }

    public partial class StructsViewer : Form
    {
        public StructsViewer()
        {
            InitializeComponent();

            GlobalValue.StructList = new List<XmlOperation.Struct>
            {
                new XmlOperation.Struct { Name = "Struct1",Members = new List<XmlOperation.Decl>{ new XmlOperation.Decl { Name="a",Type="INT"} } },
                new XmlOperation.Struct { Name = "Struct2",Members = new List<XmlOperation.Decl>{ new XmlOperation.Decl { Name="a",Type="INT"} } },
                new XmlOperation.Struct { Name = "Struct3",Members = new List<XmlOperation.Decl>{ new XmlOperation.Decl { Name="a",Type="INT"} } }
            };
            
            // 使用StructListWrapper包装StructList
            propertyGrid1.SelectedObject = new StructListWrapper(GlobalValue.StructList);
        }
    }
}