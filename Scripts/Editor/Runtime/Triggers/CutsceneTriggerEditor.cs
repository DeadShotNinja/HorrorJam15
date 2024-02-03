using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(CutsceneTrigger))]
    public class CutsceneTriggerEditor : InspectorEditor<CutsceneTrigger>
    {
        public override void OnInspectorGUI()
        {
            CutsceneTrigger.CutsceneTypeEnum cutsceneType = (CutsceneTrigger.CutsceneTypeEnum)Properties["CutsceneType"].enumValueIndex;

            serializedObject.Update();
            {
                Properties.Draw("CutsceneType");
                Properties.Draw("Cutscene");
                Properties.Draw("PlayOnStart");

                if(cutsceneType == CutsceneTrigger.CutsceneTypeEnum.CameraCutscene)
                {
                    EditorGUILayout.Space();
                    Properties.Draw("CutsceneCamera");
                    Properties.Draw("CutsceneFadeSpeed");
                }
                else
                {
                    EditorGUILayout.Space();
                    Properties.Draw("InitialPosition");
                    Properties.Draw("InitialLook");
                }

                EditorGUILayout.Space();
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["OnCutsceneStart"], new GUIContent("Events")))
                {
                    Properties.Draw("OnCutsceneStart");
                    Properties.Draw("OnCutsceneEnd");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}