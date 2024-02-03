using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(FloatingIconObject))]
    public class FloatingIconObjectEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("An object with this script attached will be marked as a floating icon object, so a floating icon will appear following the object.", MessageType.Info);
        }
    }
}