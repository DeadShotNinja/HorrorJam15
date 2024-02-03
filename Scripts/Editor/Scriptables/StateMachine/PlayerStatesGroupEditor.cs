using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HJ.Scriptable;

namespace HJ.Editors
{
    [CustomEditor(typeof(PlayerStatesGroup), true)]
    public class PlayerStatesGroupEditor : Editor
    {
        private PropertyCollection Properties;
        private PlayerStatesGroup Target;
        private IEnumerable<Type> AvailableStates;
        
        private void OnEnable()
        {
            Properties = EditorDrawing.GetAllProperties(serializedObject);
            Target = target as PlayerStatesGroup;
            AvailableStates = from type in TypeCache.GetTypesDerivedFrom<PlayerStateAsset>()
                where !type.IsAbstract
                select type;
        }
        
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                if (Properties.Count > 2)
                {
                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["PlayerStates"], new GUIContent("Global Variables")))
                    {
                        foreach (var item in Properties.Skip(2))
                        {
                            EditorGUILayout.PropertyField(item.Value);
                        }
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                    EditorGUILayout.Space();
                }

                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.LabelField("Player States", EditorStyles.miniBoldLabel);
                EditorGUILayout.EndVertical();

                if (Properties["PlayerStates"].arraySize > 0)
                {
                    Vector2 iconSize = EditorGUIUtility.GetIconSize();
                
                    for (int i = 0; i < Properties["PlayerStates"].arraySize; i++)
                    {
                        SerializedProperty state = Properties["PlayerStates"].GetArrayElementAtIndex(i);
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
                                     Properties["PlayerStates"].MoveArrayElement(index, index - 1);
                                     serializedObject.ApplyModifiedProperties();
                                 });
                             }
                             else popup.AddDisabledItem(new GUIContent("Move Up"));
                        
                             if (index < Properties["PlayerStates"].arraySize - 1)
                             {
                                 popup.AddItem(new GUIContent("Move Down"), false, () =>
                                 {
                                     Properties["PlayerStates"].MoveArrayElement(index, index + 1);
                                     serializedObject.ApplyModifiedProperties();
                                 });
                             }
                             else popup.AddDisabledItem(new GUIContent("Move Down"));
                        
                             popup.AddItem(new GUIContent("Delete"), false, () =>
                             {
                                 UnityEngine.Object stateAssetObj = stateAsset.objectReferenceValue;
                                 Properties["PlayerStates"].DeleteArrayElementAtIndex(index);
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

                if (GUILayout.Button("Add State", GUILayout.Height(25)))
                {
                    GenericMenu popup = new GenericMenu();
                    foreach (var state in AvailableStates)
                    {
                        popup.AddItem(new GUIContent(state.Name), false, AddPlayerState, state);
                    }
                    popup.ShowAsContext();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void AddPlayerState(object type)
        {
            ScriptableObject component = CreateInstance((Type)type);
            PlayerStateAsset state = (PlayerStateAsset)component;
            component.name = state.ToString();

            Undo.RegisterCreatedObjectUndo(component, "Add Player State");

            if (EditorUtility.IsPersistent(target))
                AssetDatabase.AddObjectToAsset(component, target);

            Target.PlayerStates.Add(new PlayerStateData()
            {
                StateAsset = state,
                IsEnabled = true
            });

            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssets();
        }
    }
}
