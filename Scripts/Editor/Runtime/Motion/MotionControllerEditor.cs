using UnityEngine;
using UnityEditor;
using HJ.Runtime;
using Object = UnityEngine.Object;
using HJ.Scriptable;

namespace HJ.Editors
{
    [CustomEditor(typeof(MotionController))]
    public class MotionControllerEditor : InspectorEditor<MotionController>
    {
        private MotionListHelper _motionListHelper;

        public override void OnEnable()
        {
            base.OnEnable();

            MotionPreset preset = Target.MotionPreset;
            _motionListHelper = new (preset);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                using (new EditorGUI.DisabledGroupScope(Application.isPlaying))
                {
                    EditorGUI.BeginChangeCheck();
                    Properties.Draw("MotionPreset");
                    if (EditorGUI.EndChangeCheck())
                    {
                        serializedObject.ApplyModifiedProperties();
                        Object obj = Properties["MotionPreset"].objectReferenceValue;
                        _motionListHelper.UpdatePreset((MotionPreset)obj);
                    }
                }

                Properties.Draw("HandsMotionTransform");
                Properties.Draw("HeadMotionTransform");

                EditorGUILayout.Space();

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Motion Settings")))
                {
                    Properties.Draw("MotionSuppress");
                    Properties.Draw("MotionSuppressSpeed");
                    Properties.Draw("MotionResetSpeed");
                }

                EditorGUILayout.Space();
                MotionPreset presetInstance = Target.MotionBlender.Instance;
                _motionListHelper.DrawMotionsList(presetInstance);
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}
