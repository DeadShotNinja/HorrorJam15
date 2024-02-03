using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(MovableObject))]
    public class MovableObjectEditor : InspectorEditor<MovableObject>
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                Properties.Draw("_rigidbody");
                Properties.Draw("_forwardAxis");
                Properties.Draw("_drawGizmos");
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Object Properties")))
                {
                    Properties.Draw("_moveDirection");
                    Properties.Draw("_collisionMask");
                    Properties.Draw("_holdOffset");
                    Properties.Draw("_allowRotation");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Settings")))
                {
                    Properties.Draw("_holdDistance");
                    Properties.Draw("_objectWeight");
                    Properties.Draw("_playerRadius");
                    Properties.Draw("_playerHeight");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Multipliers")))
                {
                    Properties.Draw("_walkMultiplier");
                    Properties.Draw("_lookMultiplier");
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Sounds")))
                {
                    Properties.Draw("_slideVolume");
                    Properties.Draw("_volumeFadeSpeed");
                }

                EditorGUILayout.Space();

                if (EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Use Mouse Limits"), Properties["_useMouseLimits"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties["_useMouseLimits"].boolValue))
                    {
                        Properties.Draw("_mouseVerticalLimits");
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}