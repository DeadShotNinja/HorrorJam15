using System.Collections.Generic;
using HJ.Scriptable;
using UnityEditor;
using UnityEngine;

namespace HJ.Editors
{
    [CustomEditor(typeof(AudioAsset))]
    public class AudioAssetEditor : Editor
    {
        public enum WwiseType { Event, State, Switch }

        private class FoldoutState
        {
            public bool IsOpen = true;
        }
        
        private readonly Dictionary<AudioList, FoldoutState> _audioList = new();

        private GUIStyle _boxStyle;
        private GUIStyle _headerStyle;
        
        public GUIStyle BoxStyle
        {
            get
            {
                if (_boxStyle == null)
                {
                    _boxStyle = new GUIStyle(EditorStyles.helpBox)
                    {
                        padding = new RectOffset(10, 10, 10, 10)
                    };
                }
                return _boxStyle;
            }
        }

        public GUIStyle HeaderStyle
        {
            get
            {
                if (_headerStyle == null)
                {
                    _headerStyle = new GUIStyle(EditorStyles.boldLabel)
                    {
                        fontSize = 14,
                        alignment = TextAnchor.MiddleLeft
                    };
                }
                return _headerStyle;
            }
        }
        
        private void OnEnable()
        {
            _audioList.Add(new AudioList(serializedObject, WwiseType.Event, "Ambience", "Ambience", "Audio Collection"), new FoldoutState());
            _audioList.Add(new AudioList(serializedObject, WwiseType.Event, "Enemies", "Enemies", "Audio Collection"), new FoldoutState());
            _audioList.Add(new AudioList(serializedObject, WwiseType.Event, "Environment", "Environment", "Audio Collection"), new FoldoutState());
            _audioList.Add(new AudioList(serializedObject, WwiseType.Event, "Items", "Items", "Audio Collection"), new FoldoutState());
            _audioList.Add(new AudioList(serializedObject, WwiseType.Event, "Music", "Music", "Audio Collection"), new FoldoutState());
            _audioList.Add(new AudioList(serializedObject, WwiseType.Event, "Player", "Player", "Audio Collection"), new FoldoutState());
            _audioList.Add(new AudioList(serializedObject, WwiseType.Event, "UI", "UI", "Audio Collection"), new FoldoutState());
            _audioList.Add(new AudioList(serializedObject, WwiseType.Event, "Dialog", "Dialog", "Audio Collection"), new FoldoutState());
            _audioList.Add(new AudioList(serializedObject, WwiseType.State, "State", "State", "Audio Collection"), new FoldoutState());

            // _boxStyle = new GUIStyle(EditorStyles.helpBox)
            // {
            //     padding = new RectOffset(10, 10, 10, 10)
            // };
            //
            // _headerStyle = new GUIStyle(EditorStyles.boldLabel)
            // {
            //   fontSize = 14,
            //   alignment = TextAnchor.MiddleLeft
            // };
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            foreach (var pair in _audioList)
            {
                DrawList(pair.Key, pair.Value);
            }
            
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawList(AudioList list, FoldoutState state)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField(list.Category, HeaderStyle);
            EditorGUI.indentLevel++;
            state.IsOpen = EditorGUILayout.Foldout(state.IsOpen, list.Title, true);
            if (state.IsOpen)
            {
                EditorGUILayout.BeginVertical(BoxStyle);
                list.DoLayoutList();
                EditorGUILayout.EndVertical();
            }
            EditorGUI.indentLevel--;
        }
    }
}
