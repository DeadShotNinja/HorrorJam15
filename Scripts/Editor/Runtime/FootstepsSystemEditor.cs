using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(FootstepsSystem))]
    public class FootstepsSystemEditor : InspectorEditor<FootstepsSystem>
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                Properties.Draw("_surfaceDetails");
                Properties.Draw("_footstepStyle");
                Properties.Draw("_surfaceDetection");
                Properties.Draw("_footstepsMask");

                EditorGUILayout.Space();
                using(new EditorDrawing.BorderBoxScope(new GUIContent("Footstep Settings")))
                {
                    Properties.Draw("_stepPlayerVelocity");
                    Properties.Draw("_jumpStepAirTime");
                }

                if (Target.FootstepStyle != FootstepsSystem.FootstepStyleEnum.Animation)
                {
                    EditorGUILayout.Space();
                    using (new EditorDrawing.BorderBoxScope(new GUIContent("Footstep Timing")))
                    {
                        if (Target.FootstepStyle == FootstepsSystem.FootstepStyleEnum.Timed)
                        {
                            Properties.Draw("_walkStepTime");
                            Properties.Draw("_runStepTime");
                        }
                        else if (Target.FootstepStyle == FootstepsSystem.FootstepStyleEnum.HeadBob)
                        {
                            Properties.Draw("_headBobStepWave");
                        }

                        Properties.Draw("_landStepTime");
                    }
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Footstep Volume")))
                {
                    Properties.Draw("_walkingVolume");
                    Properties.Draw("_runningVolume");
                    Properties.Draw("_landVolume");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}