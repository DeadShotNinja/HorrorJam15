using System.Linq;
using UnityEngine;
using UnityEditor;
using HJ.Scriptable;

namespace HJ.Editors
{
    [CustomEditor(typeof(GameLocalizationAsset))]
    public class GameLocalizationAssetEditor : Editor
    {
        private const string LOCALIZATION_SYMBOL = "HJ_LOCALIZATION";
        private SerializedProperty localizations;

        private void OnEnable()
        {
            localizations = serializedObject.FindProperty("Localizations");
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox("To enable or disable HJ localization, click the button below. " +
                                    "A scripting symbol should be automatically added in the player settings to allow " +
                                    "the use of GLoc Localization. (Note: This has not been implemented yet...)", MessageType.Info);
            EditorGUILayout.Space(1f);

            string toggleText = CheckActivation() ? "Disable" : "Enable";
            if (GUILayout.Button($"{toggleText} GLoc Localization", GUILayout.Height(25f)))
            {
                ToggleScriptingSymbol();
            }

            EditorGUILayout.Space();

            serializedObject.Update();
            {
                EditorGUI.indentLevel++;
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.PropertyField(localizations);
                EditorGUILayout.EndVertical();
                EditorGUI.indentLevel--;
            }
            serializedObject.ApplyModifiedProperties();
        }

        private bool CheckActivation()
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            return defines.Contains(LOCALIZATION_SYMBOL);
        }

        private void ToggleScriptingSymbol()
        {
            BuildTargetGroup buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            string defines = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);
            string[] definesParts = defines.Split(';');

            if (defines.Contains(LOCALIZATION_SYMBOL))
                definesParts = definesParts.Except(new[] { LOCALIZATION_SYMBOL }).ToArray();
            else
                definesParts = definesParts.Concat(new[] { LOCALIZATION_SYMBOL }).ToArray();

            defines = string.Join(";", definesParts);
            PlayerSettings.SetScriptingDefineSymbolsForGroup(buildTargetGroup, defines);
        }
    }
}