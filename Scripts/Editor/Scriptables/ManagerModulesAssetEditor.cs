using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using HJ.Runtime;
using HJ.Scriptable;

namespace HJ.Editors
{
    [CustomEditor(typeof(ManagerModulesAsset))]
    public class ManagerModulesAssetEditor : Editor
    {
        private SerializedProperty _managerModules;
        private ManagerModulesAsset _target;
        private IEnumerable<Type> _modules;

        private void OnEnable()
        {
            _target = (ManagerModulesAsset)target;
            _managerModules = serializedObject.FindProperty("ManagerModules");
            _modules = from type in TypeCache.GetTypesDerivedFrom<ManagerModule>()
                      where !type.IsAbstract && !_target.ManagerModules.Where(x => x != null).Any(x => x.GetType() == type)
                      select type;
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                EditorGUILayout.LabelField("Manager Modules", EditorStyles.boldLabel);
                EditorGUILayout.Space(2f);

                if (_managerModules.arraySize <= 0)
                {
                    EditorGUILayout.HelpBox("To add new modules to the manager, click the Add Module button and select the module you want to add to the manager.", MessageType.Info);
                }
                else if (_target.ManagerModules.Any(x => x == null))
                {
                    EditorGUILayout.HelpBox("There are elements that have an empty module reference, switch the inspector to debug mode and remove the element that has the missing reference.", MessageType.Warning);
                }

                for (int i = 0; i < _managerModules.arraySize; i++)
                {
                    SerializedProperty moduleProperty = _managerModules.GetArrayElementAtIndex(i);
                    PropertyCollection moduleProperties = EditorDrawing.GetAllProperties(moduleProperty);
                    string moduleName = ((ManagerModule)moduleProperty.boxedValue).Name;

                    Rect headerRect = EditorGUILayout.GetControlRect(false, 22f);
                    Texture2D icon = Resources.Load<Texture2D>("EditorIcons/module");
                    GUIContent header = new GUIContent($" {moduleName} (Module)", icon);

                    using (new EditorDrawing.IconSizeScope(12))
                    {
                        if (moduleProperty.isExpanded = EditorDrawing.DrawFoldoutHeader(headerRect, header, moduleProperty.isExpanded))
                            moduleProperties.DrawAll();
                    }

                    Rect menuRect = headerRect;
                    menuRect.xMin = menuRect.xMax - EditorGUIUtility.singleLineHeight;
                    menuRect.x -= EditorGUIUtility.standardVerticalSpacing;
                    menuRect.y += headerRect.height / 2 - 8f;

                    GUIContent menuIcon = EditorGUIUtility.TrIconContent("_Menu", "Module Menu");
                    int index = i;

                    if (GUI.Button(menuRect, menuIcon, EditorStyles.iconButton))
                    {
                        GenericMenu popup = new GenericMenu();

                        if (index > 0)
                        {
                            popup.AddItem(new GUIContent("Move Up"), false, () =>
                            {
                                _managerModules.MoveArrayElement(index, index - 1);
                                serializedObject.ApplyModifiedProperties();
                            });
                        }
                        else popup.AddDisabledItem(new GUIContent("Move Up"));

                        if (index < _managerModules.arraySize - 1)
                        {
                            popup.AddItem(new GUIContent("Move Down"), false, () =>
                            {
                                _managerModules.MoveArrayElement(index, index + 1);
                                serializedObject.ApplyModifiedProperties();
                            });
                        }
                        else popup.AddDisabledItem(new GUIContent("Move Down"));

                        popup.AddItem(new GUIContent("Delete"), false, () =>
                        {
                            _managerModules.DeleteArrayElementAtIndex(index);
                            serializedObject.ApplyModifiedProperties();
                            serializedObject.Update();
                        });

                        popup.ShowAsContext();
                    }
                }

                EditorGUILayout.Space();
                using (new EditorGUI.DisabledGroupScope(_modules.Count() == 0))
                {
                    if (GUILayout.Button("Add Module", GUILayout.Height(25f)))
                    {
                        GenericMenu popup = new GenericMenu();

                        foreach (var module in _modules)
                        {
                            popup.AddItem(new GUIContent(module.Name), false, AddModule, module);
                        }

                        popup.ShowAsContext();
                    }
                }

            }
            serializedObject.ApplyModifiedProperties();
        }

        private void AddModule(object type)
        {
            ManagerModule module = (ManagerModule)Activator.CreateInstance((Type)type);
            _target.ManagerModules.Add(module);

            serializedObject.ApplyModifiedProperties();
            serializedObject.Update();
            EditorUtility.SetDirty(target);
            AssetDatabase.SaveAssetIfDirty(target);
        }
    }
}