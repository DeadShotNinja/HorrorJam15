using UnityEngine;
using UnityEditor;

namespace HJ.Editors
{
    public class InspectorEditor<T> : Editor where T : Object
    {
        public T Target { get; private set; }
        public PropertyCollection Properties { get; private set; }

        public virtual void OnEnable()
        {
            Target = target as T;
            Properties = EditorDrawing.GetAllProperties(serializedObject);
        }
    }
}
