using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(DraggableItem))]
    public class DraggableItemEditor : InspectorEditor<DraggableItem>
    {
        private Rigidbody Rigidbody;

        public override void OnEnable()
        {
            base.OnEnable();
            Rigidbody = Target.GetComponent<Rigidbody>();
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("This object will be defined as draggable, so the player can move it. To define it's weight, change the mass value of the rigidbody component.", MessageType.Info);
            EditorGUILayout.Space(2f);

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Weight: " + Rigidbody.mass + "kg", EditorStyles.boldLabel);
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();
            serializedObject.Update();
            {
                Properties.Draw("_zoomDistance");
                Properties.Draw("_maxHoldDistance");

                EditorGUILayout.Space();
                using(new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Impact Detection"), Properties["_enableImpactSound"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("_enableImpactSound")))
                    {
                        Properties.Draw("_impactVolume");
                        Properties.Draw("_volumeModifier");
                        Properties.Draw("_nextImpact");
                    }
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.ToggleBorderBoxScope(new GUIContent("Sliding Detection"), Properties["_enableSlidingSound"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("_enableSlidingSound")))
                    {
                        Properties.Draw("_minSlidingFactor");
                        Properties.Draw("_slidingVelocityRange");
                        Properties.Draw("_slidingVolumeModifier");
                        Properties.Draw("_volumeFadeOffSpeed");
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}