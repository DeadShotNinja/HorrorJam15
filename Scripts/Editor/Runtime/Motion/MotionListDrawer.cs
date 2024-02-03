using System;
using System.Linq;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using HJ.Runtime;

namespace HJ.Editors
{
    public class MotionListDrawer
    {
        private const string Default = MotionBlender.Default;
        public static Texture2D MotionIcon => Resources.Load<Texture2D>("EditorIcons/motion2");

        private readonly List<ModulePair> modules;
        private Vector2 defaultIconSize;

        public Action<Type, int> OnAddModule;
        public Action OnAddState;

        public MotionListDrawer()
        {
            defaultIconSize = EditorGUIUtility.GetIconSize();
            modules = new();

            foreach (var type in TypeCache.GetTypesDerivedFrom<MotionModule>().Where(x => !x.IsAbstract))
            {
                MotionModule instance = Activator.CreateInstance(type) as MotionModule;
                modules.Add(new ModulePair()
                {
                     ModuleType = type,
                     ModuleName = instance.Name
                });
                instance = null;
            }
        }

        public void DrawMotionsList(SerializedProperty stateMotions, GUIContent title)
        {
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                Rect labelRect = EditorGUILayout.GetControlRect();
                EditorGUI.LabelField(labelRect, title, EditorStyles.boldLabel);

                Rect countLabelRect = labelRect;
                countLabelRect.xMin = countLabelRect.xMax - 25f;

                GUI.enabled = false;
                EditorGUI.IntField(countLabelRect, stateMotions.arraySize);
                GUI.enabled = true;

                EditorGUILayout.Space(2f);

                if (stateMotions.arraySize <= 0)
                {
                    EditorGUILayout.HelpBox("There are currently no states. To create a new state, click the 'Add State' button.", MessageType.Info);
                }

                for (int i = 0; i < stateMotions.arraySize; i++)
                {
                    SerializedProperty stateProperty = stateMotions.GetArrayElementAtIndex(i);
                    SerializedProperty stateID = stateProperty.FindPropertyRelative("StateID");
                    SerializedProperty motions = stateProperty.FindPropertyRelative("Motions");

                    string name = stateID.stringValue;
                    string iconName = name == Default ? "sv_icon_dot0_pix16_gizmo" : "AnimatorController On Icon";
                    GUIContent header = EditorGUIUtility.TrTextContentWithIcon($" {name} (State)", iconName);

                    EditorGUIUtility.SetIconSize(new Vector2(14, 14));
                    if (EditorDrawing.BeginFoldoutBorderLayout(stateProperty, header, out Rect foldoutRect, roundedBox: false))
                    {
                        EditorGUILayout.BeginVertical(GUI.skin.box);
                        EditorGUILayout.PropertyField(stateID, new GUIContent("State"));
                        EditorGUILayout.EndVertical();

                        EditorGUILayout.Space(3f);
                        Rect motionDataLabel = EditorGUILayout.GetControlRect();
                        EditorGUI.LabelField(motionDataLabel, new GUIContent("Motion Data"));

                        Rect addMotionRect = motionDataLabel;
                        addMotionRect.xMin = addMotionRect.xMax - EditorGUIUtility.singleLineHeight;

                        using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                        {
                            if (GUI.Button(addMotionRect, EditorUtils.Styles.PlusIcon))
                            {
                                int currentIndex = i;

                                Rect dropdownRect = addMotionRect;
                                dropdownRect.width = 250f;
                                dropdownRect.height = 0f;
                                dropdownRect.y += 21f;
                                dropdownRect.x = addMotionRect.x - (250f - EditorGUIUtility.singleLineHeight);

                                ModulesDropdown modulesDropdown = new(new(), modules);
                                modulesDropdown.OnItemPressed = (type) => OnAddModule(type, currentIndex);
                                modulesDropdown.Show(dropdownRect);
                            }
                        }

                        if (motions != null && motions.arraySize > 0)
                        {
                            EditorGUILayout.Space(1f);
                            for (int j = 0; j < motions.arraySize; j++)
                            {
                                SerializedProperty moduleProperty = motions.GetArrayElementAtIndex(j);
                                PropertyCollection moduleProperties = EditorDrawing.GetAllProperties(moduleProperty);

                                string motionName = moduleProperty.managedReferenceFullTypename.Split('.').Last();
                                GUIContent motionHeader = EditorGUIUtility.TrTextContentWithIcon($" {motionName} (Module)", MotionIcon);

                                if (EditorDrawing.BeginFoldoutBorderLayout(moduleProperty, motionHeader, out Rect moduleFoldoutRect))
                                {
                                    int skip = 1;
                                    EditorGUILayout.BeginVertical(GUI.skin.box);
                                    {
                                        moduleProperties.Draw("Weight");
                                        using (new EditorGUI.IndentLevelScope())
                                        {
                                            var positionSpring = moduleProperties.GetRelative("PositionSpringSettings");
                                            if (positionSpring != null)
                                            {
                                                EditorGUILayout.PropertyField(positionSpring, new GUIContent("Position Spring"));
                                                skip++;
                                            }

                                            var rotationSpring = moduleProperties.GetRelative("RotationSpringSettings");
                                            if (rotationSpring != null)
                                            {
                                                EditorGUILayout.PropertyField(rotationSpring, new GUIContent("Rotation Spring"));
                                                skip++;
                                            }
                                        }
                                    }
                                    EditorGUILayout.EndVertical();

                                    moduleProperties.DrawAll(true, skip);
                                    EditorDrawing.EndBorderHeaderLayout();
                                }

                                Rect moduleRemoveButton = moduleFoldoutRect;
                                moduleRemoveButton.xMin = moduleRemoveButton.xMax - EditorGUIUtility.singleLineHeight;
                                moduleRemoveButton.y += 3f;

                                using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                                {
                                    if (GUI.Button(moduleRemoveButton, EditorUtils.Styles.MinusIcon, EditorStyles.iconButton))
                                    {
                                        motions.DeleteArrayElementAtIndex(j);
                                    }
                                }
                            }
                        }
                        else
                        {
                            EditorGUIUtility.SetIconSize(defaultIconSize);
                            EditorGUILayout.HelpBox("There are currently no state motions. To create a new state motion, click the 'plus (+)' button.", MessageType.Info);
                        }

                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    Rect removeButton = foldoutRect;
                    removeButton.xMin = removeButton.xMax - EditorGUIUtility.singleLineHeight;
                    removeButton.y += 3f;

                    using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                    {
                        if (GUI.Button(removeButton, EditorUtils.Styles.MinusIcon, EditorStyles.iconButton))
                        {
                            stateMotions.DeleteArrayElementAtIndex(i);
                        }
                    }

                    if (i + 1 < stateMotions.arraySize) EditorGUILayout.Space(1f);
                }

                EditorGUIUtility.SetIconSize(defaultIconSize);

                EditorGUILayout.Space(2f);
                EditorDrawing.Separator();
                EditorGUILayout.Space(2f);

                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();
                    Rect moduleButtonRect = EditorGUILayout.GetControlRect(GUILayout.Width(100f));

                    using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                    {
                        if (GUI.Button(moduleButtonRect, "Add State"))
                        {
                            OnAddState?.Invoke();
                        }
                    }
                    GUILayout.FlexibleSpace();
                }
                EditorGUILayout.EndHorizontal();
            }
            EditorGUILayout.EndHorizontal();
        }
    }
}
