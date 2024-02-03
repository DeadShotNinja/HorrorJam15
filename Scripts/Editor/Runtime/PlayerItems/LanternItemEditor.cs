using UnityEngine;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(LanternItem))]
    public class LanternItemEditor : PlayerItemEditor<LanternItem>
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                base.OnInspectorGUI();
                EditorGUILayout.Space();

                Properties.Draw("<ItemObject>k__BackingField");
                Properties.Draw("_fuelInventoryItem");
                Properties.Draw("_handleBone");
                Properties.Draw("_lanternLight");
                Properties.Draw("_lanternFlame");
                Properties.Draw("_handleLimits");
                Properties.Draw("_handleAxis");

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Lantern Settings")))
                {
                    Properties.Draw("_handleGravityTime");
                    Properties.Draw("_handleForwardAngle");
                    Properties.Draw("_flameChangeSpeed");
                    Properties.Draw("_flameLightIntensity");
                    Properties.Draw("_flameAlphaFadeStart");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Light Intensity", EditorStyles.boldLabel);
                    Properties.Draw("_flameFlickerLimits");
                    Properties.Draw("_flameFlickerSpeed");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Handle Variation")))
                {
                    EditorDrawing.DrawClassBorderFoldout(Properties["_handleIdleVariation"], new GUIContent("Handle Idle Variation"));
                    EditorDrawing.DrawClassBorderFoldout(Properties["_handleWalkVariation"], new GUIContent("Handle Walk Variation"));
                    Properties.Draw("_variationBlendTime");
                    Properties.Draw("_useHandleVariation");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Fuel Settings")))
                {
                    Properties.Draw("_infiniteFuel");
                    Properties.Draw("_fuelReloadTime");
                    Properties.Draw("_fuelLife");
                    Properties.Draw("_fuelPercentage");

                    EditorGUILayout.Space();
                    float fuel = Application.isPlaying ? Target.LanternFuel : Target.FuelPercentage.Ratio();
                    int fuelPercent = Mathf.RoundToInt(fuel * 100);
                    Rect fuelPercentageRect = EditorGUILayout.GetControlRect();
                    EditorGUI.ProgressBar(fuelPercentageRect, fuel, $"Lantern Fuel ({fuelPercent}%)");
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Animation Settings")))
                {
                    Properties.Draw("_lanternDrawState");
                    Properties.Draw("_lanternHideState");
                    Properties.Draw("_lanternReloadStartState");
                    Properties.Draw("_lanternReloadEndState");
                    Properties.Draw("_lanternIdleState");

                    EditorGUILayout.Space();
                    EditorGUILayout.LabelField("Triggers", EditorStyles.boldLabel);
                    Properties.Draw("_lanternHideTrigger");
                    Properties.Draw("_lanternReloadTrigger");
                    Properties.Draw("_lanternReloadEndTrigger");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}