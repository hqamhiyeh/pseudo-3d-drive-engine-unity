using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Assets._P3dEngine.Interface
{
    internal interface IP3dEngineEditor
    {
        enum EditorValuesGroup
        {
            All,
            Settings,
            Camera
        }

        void ApplyEditorValues(EditorValuesGroup editorValuesGroup);

        void ApplyEditorValue(string serializedPropertyPath, object serializedPropertyValue)
        {
            ApplyEditorValue(this, serializedPropertyPath, serializedPropertyValue);
        }

        private void ApplyEditorValue(object context, string path, object value)
        {
            string[] split = path.Split('.');
            if (split.Length == 1)
            {
                ((IApplyEditorValues)context).ApplyEditorValue(split[0], value);
            }
            else
            {
                FieldInfo field = context.GetType().GetField(split[0], BindingFlags.NonPublic | BindingFlags.Instance);
                ApplyEditorValue(field.GetValue(context), string.Join('.', split.Skip(1)), value);
            }
        }
    }
}
