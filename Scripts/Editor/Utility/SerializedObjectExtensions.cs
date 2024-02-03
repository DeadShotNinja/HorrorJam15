using UnityEditor;

namespace HJ.Editors
{
    public static class SerializedObjectExtensions
    {
        /// <summary>
        /// Finds a <see cref="SerializedProperty"/> C# property with a [field: SerializedField] attribute,
        /// because Unity's <see cref="SerializedObject.FindProperty"/> does not work with C# properties.
        /// </summary>
        public static SerializedProperty FindRealProperty(this SerializedObject serializedObject, string name)
        {
            // This is the serialized name of a C# property.
            var realName = $"<{name}>k__BackingField";
            return serializedObject.FindProperty(realName);
        }
        
        /// <summary>
        /// Finds a relative <see cref="SerializedProperty"/> of a C# property with a [field: SerializedField] attribute.
        /// This method extends upon the standard FindPropertyRelative by accounting for C# properties.
        /// </summary>
        public static SerializedProperty FindRealPropertyRelative(this SerializedProperty property, string name)
        {
            // This is the serialized name of a C# property.
            var realName = $"<{name}>k__BackingField";
            return property.FindPropertyRelative(realName);
        }
    }
}
