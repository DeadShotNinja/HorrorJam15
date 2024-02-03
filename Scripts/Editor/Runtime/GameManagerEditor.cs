using System.Linq;
using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(GameManager))]
    public class GameManagerEditor : InspectorEditor<GameManager>
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                Properties.Draw("_modules");
                Properties.Draw("_globalPPVolume");
                Properties.Draw("_healthPPVolume");
                Properties.Draw("_backgroundFade");

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
                EditorGUILayout.Space(2f);

                if(EditorDrawing.BeginFoldoutBorderLayout(Properties["_gamePanel"], new GUIContent("Game Panels")))
                {
                    EditorGUILayout.LabelField("Main Panels", EditorStyles.boldLabel);
                    Properties.Draw("_gamePanel");
                    Properties.Draw("_pausePanel");
                    Properties.Draw("_deadPanel");

                    EditorGUILayout.Space(2f);
                    EditorGUILayout.LabelField("Sub Panels", EditorStyles.boldLabel);
                    Properties.Draw("_hudPanel");
                    Properties.Draw("_tabPanel");

                    EditorGUILayout.Space(2f);
                    EditorGUILayout.LabelField("Feature Panels", EditorStyles.boldLabel);
                    Properties.Draw("_inventoryPanel");
                    Properties.Draw("_floatingIcons");

                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_reticleImage"], new GUIContent("HUD References")))
                {
                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_controlsInfoPanel"], new GUIContent("Reticle/Stamina")))
                    {
                        Properties.Draw("_reticleImage");
                        Properties.Draw("_interactProgress");
                        Properties.Draw("_staminaSlider");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_interactInfoPanel"], new GUIContent("Interact/Controls Info")))
                    {
                        Properties.Draw("_interactInfoPanel");
                        Properties.Draw("_controlsInfoPanel");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_pointerImage"], new GUIContent("Interact Pointer")))
                    {
                        Properties.Draw("_pointerImage");
                        Properties.Draw("_normalPointer");
                        Properties.Draw("_hoverPointer");
                        Properties.Draw("_normalPointerSize");
                        Properties.Draw("_hoverPointerSize");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_itemPickupLayout"], new GUIContent("Pickup Message")))
                    {
                        Properties.Draw("_itemPickupLayout");
                        Properties.Draw("_itemPickup");
                        Properties.Draw("_pickupMessageTime");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_hintMessageGroup"], new GUIContent("Hint Message")))
                    {
                        Properties.Draw("_hintMessageGroup");
                        Properties.Draw("_hintMessageFadeSpeed");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_healthBar"], new GUIContent("Health Panel")))
                    {
                        Properties.Draw("_healthBar");
                        Properties.Draw("_hearthbeat");
                        Properties.Draw("_healthPercent");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_paperPanel"], new GUIContent("Paper Panel")))
                    {
                        Properties.Draw("_paperPanel");
                        Properties.Draw("_paperText");
                        Properties.Draw("_paperFadeSpeed");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_examineInfoPanel"], new GUIContent("Examine Panel")))
                    {
                        Properties.Draw("_examineInfoPanel");
                        Properties.Draw("_examineHotspots");
                        Properties.Draw("_examineText");
                        Properties.Draw("_examineFadeSpeed");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_overlaysParent"], new GUIContent("Overlays")))
                    {
                        Properties.Draw("_overlaysParent");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_enableBlur"], new GUIContent("Blur Settings")))
                {
                    Properties.Draw("_enableBlur");
                    Properties.Draw("_blurRadius");
                    Properties.Draw("_blurDuration");

                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);

                EditorDrawing.DrawList(Properties["_graphicReferencesRaw"], new GUIContent("Custom UI References"));

                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Manager Modules", EditorStyles.boldLabel);
                EditorGUILayout.Space(2f);

                if (Target.Modules != null)
                {
                    SerializedObject modulesAsset = new SerializedObject(Target.Modules);
                    SerializedProperty modules = modulesAsset.FindProperty("ManagerModules");

                    if (Target.Modules.ManagerModules.Any(x => x == null))
                    {
                        EditorGUILayout.HelpBox("There are elements that have an empty module reference!", MessageType.Warning);
                    }

                    for (int i = 0; i < modules.arraySize; i++)
                    {
                        SerializedProperty moduleProperty = modules.GetArrayElementAtIndex(i);
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
                    }

                    EditorGUILayout.Space(EditorGUIUtility.standardVerticalSpacing);
                    EditorGUILayout.HelpBox("To add new modules, open the Manager Modules asset.", MessageType.Info);
                }
                else
                {
                    EditorGUILayout.HelpBox("Assign the Manager Modules asset to view all modules.", MessageType.Info);
                }

                EditorGUILayout.Space();
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}