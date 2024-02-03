using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(UIButton)), CanEditMultipleObjects]
    public class UIButtonEditor : InspectorEditor<UIButton>
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                Properties.Draw("_buttonImage");
                Properties.Draw("_buttonText");

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Settings")))
                {
                    Properties.Draw("_interactable");
                    Properties.Draw("_autoDeselectOther");

                    EditorGUILayout.Space();
                    using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Use Fade"), Properties["_useFade"], roundedBox: false))
                    {
                        Properties.Draw("_fadeSpeed");
                    }

                    EditorGUILayout.Space(1f);
                    using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Pulsating"), Properties["_pulsating"], roundedBox: false))
                    {
                        Properties.Draw("_pulseColor");
                        Properties.Draw("_pulseSpeed");
                        Properties.Draw("_pulseBlend");
                    }
                }

                EditorGUILayout.Space();

                Properties.Draw("_useButtonColors");
                
                if (Properties["_useButtonColors"].boolValue)
                {
                    using (new EditorDrawing.BorderBoxScope(new GUIContent("Button Colors")))
                    {
                        Properties.Draw("_buttonNormal");
                        Properties.Draw("_buttonHover");
                        Properties.Draw("_buttonPressed");
                        Properties.Draw("_buttonSelected");
                    }
                }
                else
                {
                    using (new EditorDrawing.BorderBoxScope(new GUIContent("Button Sprites")))
                    {
                        Properties.Draw("_normalSprite");
                        Properties.Draw("_hoverSprite");
                        Properties.Draw("_selectedSprite");
                    }
                }

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Text Colors")))
                {
                    Properties.Draw("_textNormal");
                    Properties.Draw("_textHover");
                    Properties.Draw("_textPressed");
                    Properties.Draw("_textSelected");
                }

                EditorGUILayout.Space();

                Properties.Draw("_onClick");
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}