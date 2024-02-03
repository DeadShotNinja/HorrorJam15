using System;
using HJ.Runtime;
using HJ.Scriptable;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

namespace HJ.Editors
{
    public class MotionListHelper
    {
        private readonly MotionListDrawer _motionListDrawer;
        private readonly MotionPreset _motionPreset;

        private SerializedObject _motionPresetObject;
        private SerializedProperty _stateMotions;
        private bool _isInstance;

        public MotionListHelper(MotionPreset preset)
        {
            _motionPreset = preset;
            _motionListDrawer = new();
            _motionListDrawer.OnAddState = AddState;
            _motionListDrawer.OnAddModule = AddModule;

            if(preset != null)
            {
                _motionPresetObject = new SerializedObject(preset);
                _stateMotions = _motionPresetObject.FindProperty("StateMotions");
                _isInstance = false;
            }
        }

        public void DrawMotionPresetField(SerializedProperty property)
        {
            EditorGUI.BeginChangeCheck();
            EditorGUILayout.PropertyField(property);
            if (EditorGUI.EndChangeCheck())
            {
                property.serializedObject.ApplyModifiedProperties();
                Object obj = property.objectReferenceValue;
                UpdatePreset((MotionPreset)obj);
            }
        }

        public void UpdatePreset(MotionPreset preset)
        {
            _motionPresetObject = preset != null ? new SerializedObject(preset) : null;
            _stateMotions = _motionPresetObject.FindProperty("StateMotions");
        }

        public void DrawMotionsList(MotionPreset presetInstance, bool showHelp = true, bool showSave = true)
        {
            if (_motionPreset != null)
            {
                GUIContent stateMotionsTitle = new GUIContent("State Motions");
                if (Application.isPlaying && presetInstance != null)
                {
                    stateMotionsTitle = new GUIContent("State Motions (Instance)");
                    if (!_isInstance)
                    {
                        _motionPresetObject = new SerializedObject(presetInstance);
                        _stateMotions = _motionPresetObject.FindProperty("StateMotions");
                        _isInstance = true;
                    }
                }
                else
                {
                    if (_isInstance)
                    {
                        _motionPresetObject = new SerializedObject(_motionPreset);
                        _stateMotions = _motionPresetObject.FindProperty("StateMotions");
                        _isInstance = false;
                    }
                }

                if (_motionPresetObject != null)
                {
                    _motionPresetObject.Update();
                    _motionListDrawer.DrawMotionsList(_stateMotions, stateMotionsTitle);
                    _motionPresetObject.ApplyModifiedProperties();
                }
            }
            else
            {
                if (showHelp) EditorGUILayout.HelpBox("The motion preset is not selected. To manage the motion " +
                                                      "states, please pick a motion preset.", MessageType.Info);
                _motionPresetObject = null;
            }

            if (showSave && Application.isPlaying && presetInstance != null)
            {
                EditorGUILayout.Space();
                if (GUILayout.Button("Save Preset Settings", GUILayout.Height(25f)))
                {
                    _motionPreset.StateMotions = presetInstance.StateMotions;
                    new SerializedObject(_motionPreset).ApplyModifiedProperties();
                }

                EditorGUILayout.Space(1f);
                EditorGUILayout.HelpBox("During runtime, it's not possible to add or remove motion states. " +
                                        "Any modifications made to values will not be retained once you stop playing the game. " +
                                        "To ensure that these changes are preserved, click Save Preset Settings button.", MessageType.Info);
            }
        }

        private void AddState()
        {
            if (_motionPresetObject == null)
                return;

            _motionPreset.StateMotions.Add(new());
            _motionPresetObject.ApplyModifiedProperties();
            _motionPresetObject.Update();
        }

        private void AddModule(Type moduleType, int state)
        {
            if (_motionPresetObject == null)
                return;

            MotionModule motionModule = (MotionModule)Activator.CreateInstance(moduleType);
            var stateRef = _motionPreset.StateMotions[state];

            stateRef.Motions.Add(motionModule);
            _motionPresetObject.ApplyModifiedProperties();
            _motionPresetObject.Update();
        }
    }
}
