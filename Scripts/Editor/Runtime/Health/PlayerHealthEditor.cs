using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(PlayerHealth))]
    public class PlayerHealthEditor : InspectorEditor<PlayerHealth>
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    Properties.Draw("_maxHealth");
                    Properties.Draw("StartHealth");

                    EditorGUILayout.Space();
                    Rect healthRect = EditorGUILayout.GetControlRect();
                    float health = Application.isPlaying ? Target.EntityHealth : Target.StartHealth;
                    float healthPercent = health / Target.MaxHealth;
                    EditorGUI.ProgressBar(healthRect, healthPercent, $"Health ({health} HP)");
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Health Settings")))
                {
                    Properties.Draw("_useHearthbeat");
                    if (Properties["_useHearthbeat"].boolValue)
                        Properties.Draw("_lowHealthPulse");
                    Properties.Draw("_healthFadeTime");
                }

                EditorGUILayout.Space();
                using(new EditorDrawing.BorderBoxScope(new GUIContent("Blood Overlay")))
                {
                    Properties.Draw("_minHealthFade");
                    Properties.Draw("_bloodDuration");
                    Properties.Draw("_bloodFadeInSpeed");
                    Properties.Draw("_bloodFadeOutSpeed");
                }
                
                EditorGUILayout.Space();
                using(new EditorDrawing.BorderBoxScope(new GUIContent("Death Settings")))
                {
                    Properties.Draw("_closeEyesTime");
                    Properties.Draw("_closeEyesSpeed");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Additional Settings")))
                {
                    Properties.Draw("_isInvisibleToEnemies");
                    Properties.Draw("_isInvisibleToAllies");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
