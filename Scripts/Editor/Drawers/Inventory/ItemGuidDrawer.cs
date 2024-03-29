using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using HJ.Runtime;
using HJ.Scriptable;

namespace HJ.Editors
{
    [CustomPropertyDrawer(typeof(ItemGuid))]
    public class ItemGuidDrawer : PropertyDrawer
    {
        readonly InventoryAsset _inventoryAsset;
        readonly bool _hasInvReference;

        public ItemGuidDrawer()
        {
            if (Inventory.HasReference)
            {
                _inventoryAsset = Inventory.Instance.InventoryAsset;
                _hasInvReference = true;
            }
        }

        public Item GetItemRaw(string guid)
        {
            if (!string.IsNullOrEmpty(guid) && _hasInvReference && _inventoryAsset != null)
            {
                foreach (var item in _inventoryAsset.Items)
                {
                    if (item.Guid == guid)
                        return item.Item;
                }
            }

            return null;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position = EditorGUI.PrefixLabel(position, label);

            SerializedProperty guid = property.FindPropertyRelative("GUID");
            GUIContent buttonContent = new GUIContent("Select Item");

            Item item = GetItemRaw(guid.stringValue);

            if (!_hasInvReference)
            {
                buttonContent.text = "<color=#ED213A>Inventory component reference is missing!</color>";
            }
            else if (_inventoryAsset == null)
            {
                buttonContent.text = "<color=#ED213A>Inventory asset not defined!</color>";
            }
            else if(item != null)
            {
                buttonContent = EditorGUIUtility.TrTextContentWithIcon(item?.Title, "Prefab On Icon");
            }

            Rect dropdownRect = position;
            dropdownRect.width = 250f;
            dropdownRect.height = 0f;
            dropdownRect.y += EditorGUIUtility.singleLineHeight;
            dropdownRect.x += position.xMax - dropdownRect.width - EditorGUIUtility.singleLineHeight;

            if (EditorDrawing.ObjectField(position, buttonContent))
            {
                ItemPropertyDrawer.ItemPicker itemPicker = new (new AdvancedDropdownState(), _inventoryAsset);
                itemPicker.OnItemPressed += obj =>
                {
                    guid.stringValue = obj.Guid;
                    property.serializedObject.ApplyModifiedProperties();
                };

                itemPicker.Show(dropdownRect);
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }
    }
}