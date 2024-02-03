using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HJ.Scriptable;

namespace HJ.Editors
{
    [CustomEditor(typeof(AIStatesGroup), true)]
    public class AIStatesGroupEditor : Editor
    {
        private PropertyCollection _properties;
        private AIStatesGroup _target;
        private IEnumerable<Type> _availableStates;

        private void OnEnable()
        {
            _properties = EditorDrawing.GetAllProperties(serializedObject);
            _target = target as AIStatesGroup;
            _availableStates = from type in TypeCache.GetTypesDerivedFrom<AIStateAsset>()
                              where !type.IsAbstract && !_target.AIStates.Any(x => x.StateAsset.GetType() == type)
                              select type;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                if (_properties.Count > 2)
                {
                    if (EditorDrawing.BeginFoldoutBorderLayout(_properties["AIStates"], new GUIContent("Global Variables")))
                    {
                        foreach (var item in _properties.Skip(2))
                        {
                            EditorGUILayout.PropertyField(item.Value);
                        }
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                    EditorGUILayout.Space();
                }

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("AI States", EditorStyles.miniBoldLabel);
                EditorGUILayout.EndVertical();

                if (_properties["AIStates"].arraySize > 0)
                {
                    Vector2 iconSize = EditorGUIUtility.GetIconSize();

                    for (int i = 0; i < _properties["AIStates"].arraySize; i++)
                    {
                        SerializedProperty state = _properties["AIStates"].GetArrayElementAtIndex(i);
                        SerializedProperty stateAsset = state.FindPropertyRelative("StateAsset");
                        SerializedProperty isEnabled = state.FindPropertyRelative("IsEnabled");

                        bool expanded = state.isExpanded;
                        bool toggle = isEnabled.boolValue;

                        string name = stateAsset.objectReferenceValue.ToString();
                        EditorGUIUtility.SetIconSize(new Vector2(14, 14));
                        GUIContent title = EditorGUIUtility.TrTextContentWithIcon(" " + name, "NavMeshAgent Icon");
                        Rect headerRect = EditorDrawing.DrawScriptableBorderFoldoutToggle(stateAsset, title, ref expanded, ref toggle);
                        state.isExpanded = expanded;
                        isEnabled.boolValue = toggle;

                        Rect dropdownRect = headerRect;
                        dropdownRect.xMin = headerRect.xMax - EditorGUIUtility.singleLineHeight;
                        dropdownRect.x -= EditorGUIUtility.standardVerticalSpacing;
                        dropdownRect.y += headerRect.height / 2 - 8f;

                        EditorGUIUtility.SetIconSize(iconSize);
                        GUIContent dropdownIcon = EditorGUIUtility.TrIconContent("_Menu", "State Menu");
                        int index = i;

                        if (GUI.Button(dropdownRect, dropdownIcon, EditorStyles.iconButton))
                        {
                            GenericMenu popup = new GenericMenu();

                            if (index > 0)
                            {
                                popup.AddItem(new GUIContent("Move Up"), false, () =>
                                {
                                    _properties["AIStates"].MoveArrayElement(index, index - 1);
                                    serializedObject.ApplyModifiedProperties();
                                });
                            }
                            else popup.AddDisabledItem(new GUIContent("Move Up"));

                            if (index < _properties["AIStates"].arraySize - 1)
                            {
                                popup.AddItem(new GUIContent("Move Down"), false, () =>
                                {
                                    _properties["PlayerStates"].MoveArrayElement(index, index + 1);
                                    serializedObject.ApplyModifiedProperties();
                                });
                            }
                            else popup.AddDisabledItem(new GUIContent("Move Down"));

                            popup.AddItem(new GUIContent("Delete"), false, () =>
                            {
                                UnityEngine.Object stateAssetObj = stateAsset.objectReferenceValue;
                                _properties["AIStates"].DeleteArrayElementAtIndex(index);
                                serializedObject.ApplyModifiedProperties();
                                AssetDatabase.RemoveObjectFromAsset(stateAssetObj);
                                EditorUtility.SetDirty(target);
                                AssetDatabase.SaveAssets();
                            });

                            popup.ShowAsContext();
                        }
                    }

                    EditorGUILayout.Space();
                }

                using (new EditorGUI.DisabledGroupScope(_availableStates.Count() <= 0))
                {
                    if (GUILayout.Button("Add State", GUILayout.Height(25)))
                    {
                        GenericMenu popup = new GenericMenu();
                        foreach (var state in _availableStates)
                        {
                            popup.AddItem(new GUIContent(state.Name), false, AddAIState, state);
                        }
                        popup.ShowAsContext();
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void AddAIState(object type)
        {
            ScriptableObject component = CreateInstance((Type)type);
            AIStateAsset state = (AIStateAsset)component;
            component.name = state.ToString();

            Undo.RegisterCreatedObjectUndo(component, "Add AI State");

            if (EditorUtility.IsPersistent(target))
                AssetDatabase.AddObjectToAsset(component, target);

            _target.AIStates.Add(new AIStateData()
            {
                StateAsset = state,
                IsEnabled = true
            });

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
}