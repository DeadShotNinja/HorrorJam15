using UnityEditor;
using UnityEngine;

namespace HJ.Editors
{
    public class MultiToolbarItem
    {
        public GUIContent Content;
        public SerializedProperty Property;

        public MultiToolbarItem(GUIContent content, SerializedProperty toggleProperty)
        {
            Content = content;
            Property = toggleProperty;
        }
    }
}
