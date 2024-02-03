using System;
using UnityEngine;
using UnityEditor;
using HJ.Runtime;
using HJ.Scriptable;
using HJ.Editors;

namespace HJ.editors
{
    [CustomEditor(typeof(MotionPreset)), CanEditMultipleObjects]
    public class MotionPresetEditor : Editor
    {
        private MotionPreset _asset;
        private MotionListDrawer _motionListDrawer;
        private SerializedProperty _stateMotions;

        private void OnEnable()
        {
            _asset = target as MotionPreset;
            _stateMotions = serializedObject.FindProperty("StateMotions");

            _motionListDrawer = new();
            _motionListDrawer.OnAddState = AddState;
            _motionListDrawer.OnAddModule = AddModule;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                GUIContent motionsLabel = new GUIContent("State Motions");
                _motionListDrawer.DrawMotionsList(_stateMotions, motionsLabel);
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void AddState()
        {
            _asset.StateMotions.Add(new());
            serializedObject.ApplyModifiedProperties();
        }

        private void AddModule(Type moduleType, int state)
        {
            MotionModule motionModule = (MotionModule)Activator.CreateInstance(moduleType);
            _asset.StateMotions[state].Motions.Add(motionModule);
            serializedObject.ApplyModifiedProperties();
        }
    }
}
