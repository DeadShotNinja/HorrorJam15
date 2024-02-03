using System.Collections.Generic;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;
using static HJ.Editors.AudioAssetEditor;

namespace HJ.Editors
{
    public class AudioList
    {
        public readonly string Category;
        public readonly string Title;
        
        private SerializedObject _serializedObject;
        private SerializedProperty _listProperty;

        private ReorderableList _reorderableList;
        private List<bool> _expandedItems = new List<bool>();
        private WwiseType _wwiseType;

        public AudioList(SerializedObject serializedObject, WwiseType wwiseType, string propertyName, string category, string title)
        {
            _serializedObject = serializedObject;
            _listProperty = serializedObject.FindRealProperty(propertyName);
            _reorderableList = new ReorderableList(serializedObject, _listProperty, true, true, true, true);
            _wwiseType = wwiseType;
            Category = category;
            Title = title;

            InitializeCallbacks();
            InitializeExpandedItems();
        }

        private void InitializeCallbacks()
        {
            _reorderableList.drawHeaderCallback = (Rect rect) => { };

            _reorderableList.drawElementCallback = (Rect rect, int index, bool isActive, bool isFocused) =>
            {
                var listItem = _listProperty.GetArrayElementAtIndex(index);
                var typeProperty = listItem.FindRealPropertyRelative("Type");
                var customName = typeProperty.enumNames[typeProperty.enumValueIndex];

                //_expandedItems[index] = EditorGUI.Foldout(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), _expandedItems[index], customName, true);
                if (index >= 0 && index < _expandedItems.Count)
                {
                    _expandedItems[index] = EditorGUI.Foldout(new Rect(rect.x, rect.y, rect.width, EditorGUIUtility.singleLineHeight), _expandedItems[index], customName, true);
                    
                    if (_expandedItems[index])
                    {
                        var wwiseEventProperty = listItem.FindRealPropertyRelative("Wwise" + _wwiseType.ToString());
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + EditorGUIUtility.singleLineHeight + 2, rect.width, EditorGUIUtility.singleLineHeight), typeProperty);
                        EditorGUI.PropertyField(new Rect(rect.x, rect.y + (EditorGUIUtility.singleLineHeight * 2) + 4, rect.width, EditorGUIUtility.singleLineHeight), wwiseEventProperty);
                    }
                }
                else
                {
                    Debug.LogError("[AudioList] Index out of range: " + index);
                }
            };

            _reorderableList.elementHeightCallback = (int index) =>
            {
                if (index < _expandedItems.Count && _expandedItems[index])
                    return EditorGUIUtility.singleLineHeight * 3 + 6;
                else
                    return EditorGUIUtility.singleLineHeight;
            };

            _reorderableList.onAddCallback = (ReorderableList list) =>
            {
                list.serializedProperty.arraySize++;
                var newItem = list.serializedProperty.GetArrayElementAtIndex(list.serializedProperty.arraySize - 1);
                newItem.FindRealPropertyRelative("Type").enumValueIndex = 0;
                _expandedItems.Add(false);
            };
        }

        private void InitializeExpandedItems()
        {
            for (int i = 0; i < _listProperty.arraySize; i++)
            {
                _expandedItems.Add(false);
            }
        }

        public void DoLayoutList()
        {
            _reorderableList.DoLayoutList();
        }
    }
}
