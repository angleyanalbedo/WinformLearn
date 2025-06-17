using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing.Design;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.Design;
using System.Windows.Forms;
using System.Drawing;

namespace testpropertyGrid
{
    internal class TableTypeEditor
    {
    }
    [Editor(typeof(VarCollectionsEditor), typeof(UITypeEditor))]
    public class VarCollections
    {
        public List<Variable> Variables { get; set; } = new List<Variable>();

        internal VarCollections Clone()
        {
            var clone = new VarCollections();
            foreach (var variable in this.Variables)
            {
                clone.Variables.Add(new Variable
                {
                    Name = variable.Name,
                    Value = variable.Value, // 注意：如果 Value 是引用类型且需要深拷贝，这里需特殊处理
                    Type = variable.Type
                });
            }
            return clone;
        }
    }

    public class Variable
    {
        public string Name { get; set; }
        public object Value { get; set; }
        public string Type { get; set; }
    }

    public class VarCollectionsEditor : UITypeEditor
    {
        public override UITypeEditorEditStyle GetEditStyle(ITypeDescriptorContext context)
        {
            // 指定为模态对话框编辑方式
            return UITypeEditorEditStyle.Modal;
        }

        public override object EditValue(
        ITypeDescriptorContext context,
        IServiceProvider provider,
        object value)
        {
            var editorService = provider.GetService(typeof(IWindowsFormsEditorService))
                as IWindowsFormsEditorService;

            if (editorService == null || !(value is VarCollections collection))
                return value;

            // 创建编辑器控件
            var editorControl = new TableEditorUserControl(collection);

            // 自动适应内容大小
            editorControl.AutoSize = true;
            editorControl.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            // 计算合适的下拉尺寸（可根据控件内容动态调整）
            Size preferredSize = editorControl.PreferredSize;
            int maxWidth = 800, maxHeight = 600; // 可根据需要设定最大值
            int width = Math.Min(preferredSize.Width, maxWidth);
            int height = Math.Min(preferredSize.Height, maxHeight);
            editorControl.Size = new Size(width, height);

            // 设置保存事件
            editorControl.SaveRequested += (s, e) =>
            {
                editorService.CloseDropDown();
            };

            // 显示编辑器控件
            editorService.DropDownControl(editorControl);

            return editorControl.EditedCollection;
        }
    }
}
