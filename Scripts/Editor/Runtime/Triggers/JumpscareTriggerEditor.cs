using UnityEngine;
using UnityEditor;
using HJ.Runtime;
using static HJ.Runtime.JumpscareTrigger;

namespace HJ.Editors
{
    [CustomEditor(typeof(JumpscareTrigger))]
    public class JumpscareTriggerEditor : InspectorEditor<JumpscareTrigger>
    {
        private bool eventsExpanded;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                DrawJumpscareTypeGroup();
                EditorGUILayout.Space();

                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    if (Target.JumpscareType == JumpscareTypeEnum.Direct)
                    {
                        Properties.Draw("_directType");
                    }

                    Properties.Draw("_triggerType");
                }
                EditorGUILayout.EndVertical();

                EditorGUILayout.Space();

                if(Target.TriggerType == TriggerTypeEnum.Event)
                {
                    EditorGUILayout.HelpBox("The Jumpscare will be triggered when the TriggerJumpscare() method is called from another script.", MessageType.Info);
                }
                else if (Target.TriggerType == TriggerTypeEnum.TriggerEnter)
                {
                    EditorGUILayout.HelpBox("The Jumpscare will be triggered when the player enters the trigger.", MessageType.Info);
                }
                else if (Target.TriggerType == TriggerTypeEnum.TriggerExit)
                {
                    EditorGUILayout.HelpBox("The Jumpscare will be triggered when the player exits the trigger.", MessageType.Info);
                }

                EditorGUILayout.Space();
                EditorDrawing.Separator();
                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Jumpscare Setup")))
                {
                    if(Target.JumpscareType == JumpscareTypeEnum.Direct)
                    {
                        if(Target.DirectType == DirectTypeEnum.Image)
                        {
                            Properties.Draw("_jumpscareImage");
                        }
                        else if(Target.DirectType == DirectTypeEnum.Model)
                        {
                            Properties.Draw("_jumpscareModelID");
                        }

                        Properties.Draw("_directDuration");
                        EditorGUILayout.Space(1f);
                    }
                    else if(Target.JumpscareType == JumpscareTypeEnum.Indirect)
                    {
                        Properties.Draw("_animator");
                        Properties.Draw("_animatorStateName");
                        Properties.Draw("_animatorTrigger");

                        EditorGUILayout.Space(1f);
                    }
                }

                EditorGUILayout.Space(1f);

                if (Target.JumpscareType == JumpscareTypeEnum.Indirect || Target.JumpscareType == JumpscareTypeEnum.Audio)
                {
                    if (EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Look At Jumpscare"), Properties["_lookAtJumpscare"]))
                    {
                        using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("_lookAtJumpscare")))
                        {
                            EditorGUILayout.HelpBox("Slowly move the rotation of the look towards the jumpscare target.", MessageType.Info);
                            EditorGUILayout.Space(1f);

                            Properties.Draw("_lookAtTarget");
                            Properties.Draw("_lookAtDuration");
                            Properties.Draw("_lockPlayer");
                            Properties.Draw("_endJumpscareWithEvent");
                        }
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                }

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Influence Wobble"), Properties["_influenceWobble"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("_influenceWobble")))
                    {
                        EditorGUILayout.HelpBox("Wobble is a camera effect that causes the screen to shake when the player experiences a jumpscare.", MessageType.Info);
                        EditorGUILayout.Space(1f);

                        Properties.Draw("_wobbleAmplitudeGain");
                        Properties.Draw("_wobbleFrequencyGain");
                        Properties.Draw("_wobbleDuration");
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Influence Fear"), Properties["_influenceFear"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("_influenceFear")))
                    {
                        EditorGUILayout.HelpBox("Display the fear tentacles effect at the player's screen edges when the player experiences a jumpscare.", MessageType.Info);
                        EditorGUILayout.Space(1f);

                        Properties.Draw("_tentaclesIntensity");
                        Properties.Draw("_tentaclesSpeed");
                        Properties.Draw("_vignetteStrength");
                        Properties.Draw("_fearDuration");
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);

                DrawJumpscareEvents();
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawJumpscareTypeGroup()
        {
            GUIContent[] toolbarContent = {
                new GUIContent(Resources.Load<Texture>("EditorIcons/Jumpscare/direct_jumpscare"), "Direct Jumpscare"),
                new GUIContent(Resources.Load<Texture>("EditorIcons/Jumpscare/indirect_jumpscare"), "Indirect Jumpscare"),
                new GUIContent(Resources.Load<Texture>("EditorIcons/Jumpscare/audio_jumpscare"), "Audio Jumpscare"),
            };

            using (new EditorDrawing.IconSizeScope(25))
            {
                GUIStyle toolbarButtons = new GUIStyle(GUI.skin.button);
                toolbarButtons.fixedHeight = 0;
                toolbarButtons.fixedWidth = 50;

                Rect toolbarRect = EditorGUILayout.GetControlRect(false, 30);
                toolbarRect.width = toolbarButtons.fixedWidth * toolbarContent.Length;
                toolbarRect.x = EditorGUIUtility.currentViewWidth / 2 - toolbarRect.width / 2 + 7f;

                SerializedProperty jumpscareType = Properties["_jumpscareType"];
                jumpscareType.enumValueIndex = GUI.Toolbar(toolbarRect, jumpscareType.enumValueIndex, toolbarContent, toolbarButtons);
            }
        }
    
        private void DrawJumpscareEvents()
        {
            if(EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Events"), ref eventsExpanded))
            {
                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_triggerEnter"], new GUIContent("Trigger Events")))
                {
                    Properties.Draw("_triggerEnter");
                    Properties.Draw("_triggerExit");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1f);

                if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_onJumpscareStarted"], new GUIContent("Jumpscare Events")))
                {
                    Properties.Draw("_onJumpscareStarted");
                    Properties.Draw("_onJumpscareEnded");
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorDrawing.EndBorderHeaderLayout();
            }
        }
    }
}