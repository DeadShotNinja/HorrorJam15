using UnityEditor;
using UnityEngine;
using HJ.Scriptable;

namespace HJ.Editors
{
    public class InventoryItemsExport : EditorWindow
    {
        private InventoryAsset _asset;
        private GameLocalizationAsset _localizationAsset;
        private string _keysSection = "item";

        public void Show(InventoryAsset asset)
        {
            _asset = asset;
        }

        private void OnGUI()
        {
            Rect rect = position;
            rect.xMin += 5f;
            rect.xMax -= 5f;
            rect.yMin += 5f;
            rect.yMax -= 5f;
            rect.x = 5;
            rect.y = 5;

            GUILayout.BeginArea(rect);
            {
                EditorGUILayout.HelpBox("This tool automatically generates keys for the item title and description. These keys will be exported to the GameLocalization asset and assigned to the items. The title and description will be populated with the item's Title and Description text.", MessageType.Info);
                EditorGUILayout.HelpBox((_asset.Items.Count * 2) + " key will be exported.", MessageType.Info);

                EditorGUILayout.Space();
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    _localizationAsset = (GameLocalizationAsset)EditorGUILayout.ObjectField(new GUIContent("GameLocalization Asset"), _localizationAsset, typeof(GameLocalizationAsset), false);
                    _keysSection = EditorGUILayout.TextField(new GUIContent("Keys Section"), _keysSection);

                    EditorGUILayout.Space();
                    using (new EditorGUI.DisabledGroupScope(_localizationAsset == null))
                    {
                        if (GUILayout.Button(new GUIContent("Export Keys"), GUILayout.Height(25f)))
                        {
                            SerializedObject serializedObject = new SerializedObject(_asset);
                            SerializedProperty itemsLits = serializedObject.FindProperty("Items");

                            for (int i = 0; i < _asset.Items.Count; i++)
                            {
                                var rawItem = _asset.Items[i];
                                SerializedProperty itemElement = itemsLits.GetArrayElementAtIndex(i);
                                SerializedProperty item = itemElement.FindPropertyRelative("item");
                                SerializedProperty title = item.FindPropertyRelative("Title");

                                SerializedProperty localization = item.FindPropertyRelative("LocalizationSettings");
                                SerializedProperty titleKeyProp = localization.FindPropertyRelative("titleKey");
                                SerializedProperty descKeyProp = localization.FindPropertyRelative("descriptionKey");

                                string itemTitle = title.stringValue.Replace(" ", "").ToLower();
                                string titleKey = _keysSection + ".title." + itemTitle;
                                string descriptionKey = _keysSection + ".description." + itemTitle;

                                titleKeyProp.stringValue = titleKey;
                                descKeyProp.stringValue = descriptionKey;

                                _localizationAsset.AddSectionKey(titleKey, rawItem.Item.Title);
                                _localizationAsset.AddSectionKey(descriptionKey, rawItem.Item.Description);
                            }

                            serializedObject.ApplyModifiedProperties();
                        }
                    }
                }
                EditorGUILayout.EndVertical();
            }
            GUILayout.EndArea();
        }
    }
}