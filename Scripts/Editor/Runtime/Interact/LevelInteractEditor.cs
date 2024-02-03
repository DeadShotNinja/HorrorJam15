using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(LevelInteract))]
    public class LevelInteractEditor : Editor
    {
        private SerializedProperty _levelLoadType;
        private SerializedProperty _nextLevelName;

        private SerializedProperty _customTransform;
        private SerializedProperty _targetTransform;
        private SerializedProperty _lookUpDown;

        private SerializedProperty _nextWwiseState;

        private SerializedProperty _onInteractEvent;

        private void OnEnable()
        {
            _levelLoadType = serializedObject.FindProperty("_levelLoadType");
            _nextLevelName = serializedObject.FindProperty("_nextLevelName");

            _customTransform = serializedObject.FindProperty("_customTransform");
            _targetTransform = serializedObject.FindProperty("_targetTransform");
            _lookUpDown = serializedObject.FindProperty("_lookUpDown");
            
            _nextWwiseState = serializedObject.FindProperty("_nextWwiseState");
            _onInteractEvent = serializedObject.FindProperty("_onInteract");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            LevelInteract.LevelType levelType = (LevelInteract.LevelType)_levelLoadType.enumValueIndex;

            using(new EditorDrawing.BorderBoxScope(new GUIContent("Next Level"), 18f, true))
            {
                EditorGUILayout.PropertyField(_levelLoadType);
                EditorGUILayout.PropertyField(_nextLevelName);
                EditorGUILayout.Space();

                if(levelType == LevelInteract.LevelType.NextLevel)
                    EditorGUILayout.HelpBox("The current world state will be saved and the player data will be saved and transferred to the next level.", MessageType.Info);
                else if (levelType == LevelInteract.LevelType.WorldState)
                    EditorGUILayout.HelpBox("The current world state will be saved, the world state of the next level will be loaded and the player data will be transferred. (Previous Scene Persistency must be enabled!)", MessageType.Info);
                else if (levelType == LevelInteract.LevelType.PlayerData)
                    EditorGUILayout.HelpBox("Only the player data will be saved and transferred to the next level.", MessageType.Info);

            }

            EditorGUILayout.Space();

            _customTransform.boolValue = EditorDrawing.BeginToggleBorderLayout(new GUIContent("Custom Transform"), _customTransform.boolValue);
            using (new EditorGUI.DisabledGroupScope(!_customTransform.boolValue))
            {
                EditorGUILayout.PropertyField(_targetTransform);
                EditorGUILayout.PropertyField(_lookUpDown);
                EditorGUILayout.Space();
                EditorGUILayout.HelpBox("The player position and rotation will be replaced by custom position and rotation specified by the target transform.", MessageType.Info);
            }
            EditorDrawing.EndBorderHeaderLayout();
            
            EditorGUILayout.Space();
            
            using(new EditorDrawing.BorderBoxScope(new GUIContent("New Wwise State"), 18f, true))
            {
                EditorGUILayout.PropertyField(_nextWwiseState);
                EditorGUILayout.Space();

                EditorGUILayout.HelpBox("The Wwise state that will be changed (if any) during level transition.", MessageType.Info);
            }
            
            EditorGUILayout.Space();
            
            using(new EditorDrawing.BorderBoxScope(new GUIContent("Events"), 18f, true))
            {
                EditorGUILayout.PropertyField(_onInteractEvent);
            }

            serializedObject.ApplyModifiedProperties();
        }
    }
}