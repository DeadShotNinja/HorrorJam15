using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(LeversPuzzleOrderLights))]
    public class LeversPuzzleOrderLightsEditor : InspectorEditor<LeversPuzzleOrderLights>
    {
        public override void OnEnable()
        {
            base.OnEnable();

            if(Properties["_leversPuzzle"].objectReferenceValue != null)
                Properties["_orderLights"].arraySize = Target.LeversPuzzle.Levers.Count;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("A helper script that is used to display the number of lever interactions you need to validate the levers order.", MessageType.Info);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUI.BeginChangeCheck();
                Properties.Draw("_leversPuzzle");
                if (EditorGUI.EndChangeCheck())
                {
                    LeversPuzzle leversPuzzle = (LeversPuzzle)Properties["_leversPuzzle"].objectReferenceValue;
                    if (leversPuzzle != null) Properties["_orderLights"].arraySize = leversPuzzle.Levers.Count;
                    else Properties["_orderLights"].arraySize = 0;
                }

                if (Properties["_leversPuzzle"].objectReferenceValue != null)
                {
                    EditorGUILayout.Space(1f);
                    DrawOrderLeverList();
                }

                EditorGUILayout.Space();
                Properties.Draw("_emissionKeyword");
                using(new EditorGUI.DisabledGroupScope(true))
                {
                    Properties.Draw("_orderIndex");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawOrderLeverList()
        {
            if(EditorDrawing.BeginFoldoutBorderLayout(Properties["_orderLights"], new GUIContent("Order Lights")))
            {
                for (int i = 0; i < Properties["_orderLights"].arraySize; i++)
                {
                    SerializedProperty property = Properties["_orderLights"].GetArrayElementAtIndex(i);
                    if (EditorDrawing.BeginFoldoutBorderLayout(property, new GUIContent("Light " + i)))
                    {
                        EditorDrawing.GetAllProperties(property).DrawAll();
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                    EditorGUILayout.Space(1f);
                }

                EditorDrawing.EndBorderHeaderLayout();
            }
        }
    }
}