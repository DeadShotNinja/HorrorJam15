using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(LevelManager))]
    public class LevelManagerEditor : InspectorEditor<LevelManager>
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                EditorDrawing.DrawList(Properties["_levelInfos"], new GUIContent("Level Infos"));
                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("UI")))
                {
                    Properties.Draw("_title");
                    Properties.Draw("_description");
                    Properties.Draw("_background");
                    Properties.Draw("_fadingBackground");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Settings")))
                {
                    Properties.Draw("_loadPriority");
                    if (GUILayout.Button("Loading Priority Documentation"))
                    {
                        Application.OpenURL("https://docs.unity3d.com/ScriptReference/Application-backgroundLoadingPriority.html");
                    }

                    EditorGUILayout.Space();
                    Properties.Draw("_fadeSpeed");
                    Properties.Draw("_switchManually");
                    Properties.Draw("_fadeBackground");
                    Properties.Draw("_debugging");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Switch Panels"), Properties["_switchPanels"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("_switchPanels")))
                    {
                        Properties.Draw("_switchFadeSpeed");
                        Properties.Draw("_currentPanel");
                        Properties.Draw("_newPanel");
                    }
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Events")))
                {
                    Properties.Draw("_onProgressUpdate");
                    Properties.Draw("_onLoadingDone");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}