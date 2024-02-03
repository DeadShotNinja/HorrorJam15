using UnityEngine;
using UnityEditor;
using HJ.Editors;

namespace HJ.Runtime
{
    [CustomEditor(typeof(DragRigidbody))]
    public class DragRigidbodyEditor : InspectorEditor<DragRigidbody>
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                Properties.Draw("_holdType");
                Properties.Draw("_dragType");

                EditorGUILayout.Space();
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_dragType"], new GUIContent("Controls Settings")))
                {
                    EditorGUI.indentLevel++;
                    Properties.Draw("_controlsContexts");
                    EditorGUI.indentLevel--;
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_showGrabReticle"], new GUIContent("Reticle Settings")))
                {
                    Properties.Draw("_showGrabReticle");
                    EditorGUI.indentLevel++;
                    Properties.Draw("_grabHand");
                    Properties.Draw("_holdHand");
                    EditorGUI.indentLevel--;
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_interpolate"], new GUIContent("Rigidbody Settings")))
                {
                    Properties.Draw("_interpolate");
                    Properties.Draw("_collisionDetection");
                    Properties.Draw("_freezeRotation");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_dragStrength"], new GUIContent("General Settings")))
                {
                    Properties.Draw("_dragStrength");
                    Properties.Draw("_throwStrength");
                    Properties.Draw("_rotateSpeed");
                    Properties.Draw("_zoomSpeed");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_hitpointOffset"], new GUIContent("Features")))
                {
                    Properties.Draw("_hitpointOffset");
                    Properties.Draw("_playerCollision");
                    Properties.Draw("_objectZooming");
                    Properties.Draw("_objectRotating");
                    Properties.Draw("_objectThrowing");
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}