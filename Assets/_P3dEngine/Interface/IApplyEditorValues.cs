using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets._P3dEngine.Interface
{
    internal interface IApplyEditorValues
    {
        void ApplyEditorValues();

        void ApplyEditorValue(string editorFieldName, object editorValue)
        {
            this.GetType().GetProperty(editorFieldName[2..]).SetValue(this, editorValue);
        }
    }
}
