using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(ObjectiveEvent))]
    public class ObjectiveEventEditor : Editor
    {
        private SerializedProperty _objective;

        private SerializedProperty _onObjectiveAdded;
        private SerializedProperty _onObjectiveCompleted;

        private SerializedProperty _onSubObjectiveAdded;
        private SerializedProperty _onSubObjectiveCompleted;
        private SerializedProperty _onSubObjectiveCountChanged;

        bool[] expanded = new bool[2];

        private void OnEnable()
        {
            _objective = serializedObject.FindProperty("Objective");

            _onObjectiveAdded = serializedObject.FindProperty("OnObjectiveAdded");
            _onObjectiveCompleted = serializedObject.FindProperty("OnObjectiveCompleted");

            _onSubObjectiveAdded = serializedObject.FindProperty("OnSubObjectiveAdded");
            _onSubObjectiveCompleted = serializedObject.FindProperty("OnSubObjectiveCompleted");
            _onSubObjectiveCountChanged = serializedObject.FindProperty("OnSubObjectiveCountChanged");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            EditorGUILayout.PropertyField(_objective);
            EditorGUILayout.Space();

            if(EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Objective Events"), ref expanded[0]))
            {
                EditorGUILayout.PropertyField(_onObjectiveAdded);
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(_onObjectiveCompleted);
                EditorDrawing.EndBorderHeaderLayout();
            }

            EditorGUILayout.Space(2f);

            if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("SubObjective Events"), ref expanded[1]))
            {
                EditorGUILayout.PropertyField(_onSubObjectiveAdded);
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(_onSubObjectiveCompleted);
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(_onSubObjectiveCountChanged);
                EditorDrawing.EndBorderHeaderLayout();
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}