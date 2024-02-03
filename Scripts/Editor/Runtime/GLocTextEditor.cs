using HJ.Runtime;
using UnityEngine;
using UnityEditor;
using HJ.Scriptable;

namespace HJ.Editors
{
    [CustomEditor(typeof(GLocText)), CanEditMultipleObjects]
    public class GLocTextEditor : InspectorEditor<GLocText>
    {
        private GameLocalizationAsset _localizationAsset;

        public override void OnEnable()
        {
            base.OnEnable();

            if (GameLocalization.HasReference)
                _localizationAsset = GameLocalization.Instance.LocalizationAsset;
        }

        public override void OnInspectorGUI()
        {
            EditorDrawing.DrawInspectorHeader(new GUIContent("Gloc Text (Localization)"), Target);
            EditorGUILayout.Space();

            serializedObject.Update();
            {
                Properties.Draw("_glocKey");
                Properties.Draw("_observeMany");

                EditorGUILayout.Space();
                Properties.Draw("_onUpdateText");

                EditorGUILayout.Space();
                using (new EditorGUI.DisabledGroupScope(_localizationAsset == null))
                {
                    if (GUILayout.Button("Ping Localization Asset", GUILayout.Height(25)))
                    {
                        EditorGUIUtility.PingObject(_localizationAsset);
                    }
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}