using UnityEngine;
using UnityEditor;
using HJ.Scriptable;

namespace HJ.Editors
{
    [CustomEditor(typeof(ObjectivesAsset))]
    public class ObjectivesAssetEditor : Editor
    {
        private SerializedProperty Objectives;

        private void OnEnable()
        {
            Objectives = serializedObject.FindProperty("Objectives");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.PropertyField(Objectives);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}