using System.Linq;
using UnityEngine;
using UnityEditor;
using UnityEditor.Callbacks;
using HJ.Scriptable;

namespace HJ.Editors
{
    [CustomEditor(typeof(InventoryAsset))]
    public class InventoryAssetEditor : Editor
    {
        private static InventoryAsset _asset;

        public static GUIStyle wordWrappedLabel
        {
            get => new(EditorStyles.wordWrappedMiniLabel)
            {
                richText = true
            };
        }

        private void OnEnable()
        {
            _asset = (InventoryAsset)target;
        }

        [OnOpenAsset]
        public static bool OnOpenAsset(int instanceId, int line)
        {
            var obj = EditorUtility.InstanceIDToObject(instanceId);
            var asset = obj as InventoryAsset;
            if (asset == null) return false;

            OpenDatabaseEditor();
            return true;
        }

        static void OpenDatabaseEditor()
        {
            EditorWindow invBuilder = EditorWindow.GetWindow<InventoryBuilder>(false, "Inventory Builder", true);

            Vector2 windowSize = new Vector2(1000, 500);
            invBuilder.minSize = windowSize;

            (invBuilder as InventoryBuilder).Show(_asset);
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                EditorGUILayout.Space(10);
                EditorGUILayout.HelpBox("Contains a database of inventory items.", MessageType.Info, true);
                EditorGUILayout.Space(2);
                EditorGUILayout.HelpBox("Assign this asset to an Inventory script to activate item selection with this asset.", MessageType.Warning, true);
                EditorGUILayout.Space(10);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                string items = string.Join(",", _asset.Items.Select(x => x.Item.Title).Take(10));
                EditorGUILayout.LabelField("<b>Items:</b> " + items, wordWrappedLabel);
                EditorGUILayout.EndVertical();

                if (GUILayout.Button("Open Inventory Builder", GUILayout.Height(30)))
                {
                    OpenDatabaseEditor();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}