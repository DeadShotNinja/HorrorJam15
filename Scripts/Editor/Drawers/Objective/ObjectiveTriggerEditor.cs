using UnityEngine;
using UnityEditor;
using HJ.Runtime;
using HJ.Scriptable;

namespace HJ.Editors
{
    [CustomEditor(typeof(ObjectiveTrigger)), CanEditMultipleObjects]
    public class ObjectiveTriggerEditor : Editor
    {
        private SerializedProperty _triggerType;
        private SerializedProperty _objectiveType;

        private SerializedProperty _objective;
        private SerializedProperty _completeObjective;

        private ObjectivesAsset _objectivesAsset;

        private void OnEnable()
        {
            _triggerType = serializedObject.FindProperty("_triggerType");
            _objectiveType = serializedObject.FindProperty("_objectiveType");

            _objective = serializedObject.FindProperty("_objectiveToAdd");
            _completeObjective = serializedObject.FindProperty("_objectiveToComplete");

            if (ObjectiveManager.HasReference)
                _objectivesAsset = ObjectiveManager.Instance.ObjectivesAsset;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            ObjectiveTrigger.ObjectiveType objectiveTypeEnum = (ObjectiveTrigger.ObjectiveType)_objectiveType.enumValueIndex;

            using(new EditorDrawing.BorderBoxScope(false))
            {
                EditorGUILayout.PropertyField(_triggerType);
                EditorGUILayout.PropertyField(_objectiveType);
            }

            EditorGUILayout.Space();

            if (objectiveTypeEnum == ObjectiveTrigger.ObjectiveType.New)
            {
                EditorGUILayout.PropertyField(_objective);
            }
            else if (objectiveTypeEnum == ObjectiveTrigger.ObjectiveType.Complete)
            {
                EditorGUILayout.PropertyField(_completeObjective);
            }
            else if (objectiveTypeEnum == ObjectiveTrigger.ObjectiveType.NewAndComplete)
            {
                EditorGUILayout.PropertyField(_objective);
                EditorGUILayout.Space(2f);
                EditorGUILayout.PropertyField(_completeObjective);
            }

            EditorGUILayout.Space();
            using (new EditorGUI.DisabledGroupScope(_objectivesAsset == null))
            {
                if (GUILayout.Button("Ping Objectives Asset", GUILayout.Height(25)))
                {
                    EditorGUIUtility.PingObject(_objectivesAsset);
                }
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}