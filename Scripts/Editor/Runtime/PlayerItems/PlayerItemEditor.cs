using UnityEngine;
using UnityEditor;
using HJ.Runtime;
using HJ.Scriptable;

namespace HJ.Editors
{
    public class PlayerItemEditor<T> : Editor where T : PlayerItemBehaviour
    {
        public T Target { get; private set; }
        public PlayerItemBehaviour Behaviour { get; private set; }
        public PropertyCollection Properties { get; private set; }

        private bool _settingsFoldout;
        private MotionListHelper _motionListHelper;

        public virtual void OnEnable()
        {
            Target = target as T;
            Behaviour = Target;
            Properties = EditorDrawing.GetAllProperties(serializedObject);

            MotionPreset preset = Behaviour.MotionPreset;
            _motionListHelper = new(preset);
        }

        public override void OnInspectorGUI()
        {
            GUIContent playerItemSettingsContent = EditorGUIUtility.TrTextContentWithIcon(" PlayerItem Base Settings", "Settings");
            if (EditorDrawing.BeginFoldoutBorderLayout(playerItemSettingsContent, ref _settingsFoldout))
            {
                if(EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Wall Detection"), Properties["_enableWallDetection"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("_enableWallDetection")))
                    {
                        Properties.Draw("_showRayGizmos");
                        Properties.Draw("_wallHitMask");
                        Properties.Draw("_wallHitRayDistance");
                        Properties.Draw("_wallHitRayRadius");
                        Properties.Draw("_wallHitAmount");
                        Properties.Draw("_wallHitTime");
                        Properties.Draw("_wallHitRayOffset");
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                if (EditorDrawing.BeginFoldoutToggleBorderLayout(new GUIContent("Motion Preset"), Properties["_enableMotionPreset"]))
                {
                    using (new EditorGUI.DisabledGroupScope(!Properties.BoolValue("_enableMotionPreset")))
                    {
                        _motionListHelper.DrawMotionPresetField(Properties["_motionPreset"]);
                        Properties.Draw("<MotionPivot>k__BackingField");

                        if (_motionListHelper != null)
                        {
                            EditorGUILayout.Space();
                            MotionPreset presetInstance = Behaviour.MotionBlender.Instance;
                            _motionListHelper.DrawMotionsList(presetInstance);
                        }
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorDrawing.EndBorderHeaderLayout();
            }
        }
    }
}