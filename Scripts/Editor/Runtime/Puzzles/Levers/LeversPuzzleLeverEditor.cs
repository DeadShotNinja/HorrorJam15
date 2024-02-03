using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(LeversPuzzleLever))]
    public class LeversPuzzleLeverEditor : InspectorEditor<LeversPuzzleLever>
    {
        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("Determines whether the object is a lever object that you can interact with, " +
                                    "and determines what lever is pressed. The LeversPuzzle script should be added in the any " +
                                    "parent object of this object.", MessageType.Info);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                using(new EditorGUI.DisabledGroupScope(true))
                {
                    Properties.Draw("_leverState");
                }
                EditorGUILayout.Space();

                using(new EditorDrawing.BorderBoxScope(new GUIContent("Limits")))
                {
                    Properties.Draw("_target");
                    Properties.Draw("_limitsObject");
                    Properties.Draw("_switchLimits");
                    Properties.Draw("_limitsForward");
                    Properties.Draw("_limitsNormal");
                }

                EditorGUILayout.Space(2f);
                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Light"), Properties["_useLight"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("_useLight")))
                    {
                        Properties.Draw("_leverLight");
                        Properties.Draw("_emissionKeyword");
                        Properties.Draw("_lightRenderer");
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}