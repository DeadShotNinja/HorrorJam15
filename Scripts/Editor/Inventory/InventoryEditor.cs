using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(Inventory))]
    public class InventoryEditor : InspectorEditor<Inventory>
    {
        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Properties.Draw("_inventoryAsset");
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("References", EditorStyles.boldLabel);
            Properties.Draw("_inventoryContainers");
            Properties.Draw("_slotsLayoutGrid");
            Properties.Draw("_itemsTransform");
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Settings", EditorStyles.boldLabel);
            if (EditorDrawing.BeginFoldoutBorderLayout(Properties["_itemsTransform"], new GUIContent("Controls Settings")))
            {
                EditorGUI.indentLevel++;
                Properties.Draw("_controlsContexts");
                EditorGUI.indentLevel--;
                EditorDrawing.EndBorderHeaderLayout();
            }
            EditorGUILayout.Space(1f);
            EditorDrawing.DrawClassBorderFoldout(Properties["_settings"], new GUIContent("Items Settings"));
            EditorGUILayout.Space(1f);
            EditorDrawing.DrawClassBorderFoldout(Properties["_slotSettings"], new GUIContent("Slot Settings"));
            EditorGUILayout.Space(1f);
            EditorDrawing.DrawClassBorderFoldout(Properties["_containerSettings"], new GUIContent("Container Settings"));
            EditorGUILayout.Space(1f);
            EditorDrawing.DrawClassBorderFoldout(Properties["_itemInfo"], new GUIContent("Item Info"));
            EditorGUILayout.Space(1f);
            EditorDrawing.DrawClassBorderFoldout(Properties["_shortcutSettings"], new GUIContent("Shortcut Settings"));
            EditorGUILayout.Space(1f);
            EditorDrawing.DrawClassBorderFoldout(Properties["_promptSettings"], new GUIContent("Prompt Settings"));
            EditorGUILayout.Space(1f);
            EditorDrawing.DrawClassBorderFoldout(Properties["_contextMenu"], new GUIContent("Context Menu"));
            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Features", EditorStyles.boldLabel);
            bool siExpanded = Properties["_startingItems"].isExpanded;
            if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Starting Items"), ref siExpanded))
            {
                for (int i = 0; i < Properties["_startingItems"].arraySize; i++)
                {
                    SerializedProperty property = Properties["_startingItems"].GetArrayElementAtIndex(i);
                    DrawStartingItem(property, i);
                    EditorGUILayout.Space(1f);
                }

                EditorGUILayout.Space(1f);
                if (GUILayout.Button("Add Starting Item"))
                {
                    Target.StartingItems.Add(new StartingItem());
                }

                EditorDrawing.EndBorderHeaderLayout();
            }
            Properties["_startingItems"].isExpanded = siExpanded;

            EditorGUILayout.Space(1f);

            bool esEnabled = Properties.GetRelative("_expandableSlots.Enabled").boolValue;
            if (EditorDrawing.BeginFoldoutToggleBorderLayout(Properties["_expandableSlots"], new GUIContent("Expandable Slots"), ref esEnabled))
            {
                using (new EditorGUI.DisabledScope(!esEnabled))
                {
                    EditorGUILayout.PropertyField(Properties.GetRelative("_expandableSlots.ShowExpandableSlots"));
                    EditorGUILayout.PropertyField(Properties.GetRelative("_expandableSlots.ExpandableRows"));
                }
                EditorDrawing.EndBorderHeaderLayout();
            }
            Properties.GetRelative("_expandableSlots.Enabled").boolValue = esEnabled;

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawStartingItem(SerializedProperty property, int index)
        {
            SerializedProperty guid = property.FindPropertyRelative("GUID");
            SerializedProperty title = property.FindPropertyRelative("Title");
            SerializedProperty quantity = property.FindPropertyRelative("Quantity");
            SerializedProperty data = property.FindPropertyRelative("Data");
            SerializedProperty jsonData = data.FindPropertyRelative("JsonData");

            GUIContent headerTitle = new GUIContent($"[{index}] None");
            string itemTitle = string.Empty;

            if (Target.InventoryAsset != null && !string.IsNullOrEmpty(guid.stringValue))
            {
                foreach (var item in Target.InventoryAsset.Items)
                {
                    if(item.Guid == guid.stringValue)
                    {
                        headerTitle.text = $"[{index}] {item.Item.Title}";
                        itemTitle = item.Item.Title;
                        break;
                    }
                }
            }

            bool isExpanded = property.isExpanded;
            if (EditorDrawing.BeginFoldoutBorderLayout(headerTitle, ref isExpanded, out Rect headerRect, 18f, false))
            {
                DrawItemGUID(guid, itemTitle);
                EditorGUILayout.PropertyField(quantity);
                if (quantity.intValue < 1) quantity.intValue = 1;

                EditorGUI.indentLevel++;
                {
                    Rect foldoutRect = EditorGUILayout.GetControlRect();
                    foldoutRect = EditorGUI.IndentedRect(foldoutRect);

                    if (jsonData.isExpanded = EditorGUI.BeginFoldoutHeaderGroup(foldoutRect, jsonData.isExpanded, "Custom JSON Data"))
                    {
                        EditorGUILayout.Space(-EditorGUIUtility.singleLineHeight);
                        EditorGUILayout.PropertyField(jsonData, GUIContent.none);
                    }
                    EditorGUI.EndFoldoutHeaderGroup();
                }
                EditorGUI.indentLevel--;

                EditorDrawing.EndBorderHeaderLayout();
            }
            property.isExpanded = isExpanded;

            Rect removeRect = headerRect;
            removeRect.xMin = removeRect.xMax - EditorGUIUtility.singleLineHeight;
            removeRect.y += 3f;
            removeRect.x -= 2f;

            if (GUI.Button(removeRect, EditorUtils.Styles.TrashIcon, EditorStyles.iconButton))
            {
                Properties["_startingItems"].DeleteArrayElementAtIndex(index);
            }
        }

        private void DrawItemGUID(SerializedProperty guid, string itemTitle)
        {
            Rect rect = EditorGUILayout.GetControlRect();
            rect = EditorGUI.PrefixLabel(rect, new GUIContent("GUID"));

            GUIContent buttonContent = new GUIContent("Select Item");

            if (Target.InventoryAsset == null)
            {
                buttonContent.text = "<color=#ED213A>Inventory asset not defined!</color>";
            }
            else if(!string.IsNullOrEmpty(guid.stringValue))
            {
                buttonContent = EditorGUIUtility.TrTextContentWithIcon(itemTitle, "Prefab On Icon");
            }

            Rect dropdownRect = rect;
            dropdownRect.width = 250f;
            dropdownRect.height = 0f;
            dropdownRect.y += 21f;
            dropdownRect.x += rect.xMax - dropdownRect.width - EditorGUIUtility.singleLineHeight;

            if (EditorDrawing.ObjectField(rect, buttonContent))
            {
                ItemPropertyDrawer.ItemPicker itemPicker = new (new AdvancedDropdownState(), Target.InventoryAsset);
                itemPicker.OnItemPressed += obj =>
                {
                    guid.stringValue = obj.Guid;
                    serializedObject.ApplyModifiedProperties();
                };

                itemPicker.Show(dropdownRect);
            }
        }
    }
}