using UnityEngine;
using UnityEditor;
using HJ.Editors;

namespace HJ.Runtime
{
    [CustomEditor(typeof(ExamineController))]
    public class ExamineControllerEditor : InspectorEditor<ExamineController>
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                Properties.Draw("_focusCullLayes");
                Properties.Draw("_focusLayer");
                Properties.Draw("_focusRenderingLayer");
                Properties.Draw("_hotspotPrefab");

                EditorGUILayout.Space();
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_hotspotPrefab"], new GUIContent("Controls Settings")))
                {
                    EditorGUI.indentLevel++;
                    Properties.Draw("_controlPutBack");
                    Properties.Draw("_controlRead");
                    Properties.Draw("_controlTake");
                    Properties.Draw("_controlRotate");
                    Properties.Draw("_controlZoom");
                    EditorGUI.indentLevel--;
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_rotateTime"], new GUIContent("General Settings")))
                {
                    Properties.Draw("_rotateTime");
                    Properties.Draw("_rotateMultiplier");
                    Properties.Draw("_zoomMultiplier");
                    Properties.Draw("_timeToExamine");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_dropOffset"], new GUIContent("Offset Settings")))
                {
                    Properties.Draw("_dropOffset");
                    Properties.Draw("_inventoryOffset");
                    Properties.Draw("_showLabels");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_pickUpCurve"], new GUIContent("Pickup Settings")))
                {
                    Properties.Draw("_pickUpCurve");
                    Properties.Draw("_pickUpCurveMultiplier");
                    Properties.Draw("_pickUpTime");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_pickUpTime"], new GUIContent("Put Settings")))
                {
                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_putPositionCurve"], new GUIContent("Position Curve")))
                    {
                        Properties.Draw("_putPositionCurve");
                        Properties.Draw("_putPositionCurveMultiplier");
                        Properties.Draw("_putPositionCurveTime");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorGUILayout.Space(1f);

                    if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_putRotationCurve"], new GUIContent("Rotation Curve")))
                    {
                        Properties.Draw("_putRotationCurve");
                        Properties.Draw("_putRotationCurveMultiplier");
                        Properties.Draw("_putRotationCurveTime");
                        EditorDrawing.EndBorderHeaderLayout();
                    }

                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}