using System;
using UnityEngine;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using HJ.Runtime;
using HJ.Input;

namespace HJ.Editors
{
    [CustomPropertyDrawer(typeof(InputReference))]
    public class InputReferenceDrawer : PropertyDrawer
    {
        public static Texture2D InputActionIcon => Resources.Load<Texture2D>("EditorIcons/InputAction");

        private readonly Lazy<InputManager> inputManager = new(() =>
        {
            if (InputManager.HasReference)
                return InputManager.Instance;

            return null;
        });

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            SerializedProperty actionNameProp = property.FindPropertyRelative("ActionName");
            SerializedProperty bindingIndexProp = property.FindPropertyRelative("BindingIndex");

            EditorGUI.BeginProperty(position, label, property);
            {
                position = EditorGUI.PrefixLabel(position, label);

                Rect dropdownRect = position;
                dropdownRect.width = 250f;
                dropdownRect.height = 0f;
                dropdownRect.y += 21f;
                dropdownRect.x += position.xMax - dropdownRect.width - EditorGUIUtility.singleLineHeight;

                InputPicker inputPicker = new(new AdvancedDropdownState(), inputManager.Value.InputActions);
                inputPicker.OnItemPressed = (name, index) =>
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        actionNameProp.stringValue = string.Empty;
                        bindingIndexProp.intValue = -1;
                    }
                    else
                    {
                        actionNameProp.stringValue = name;
                        bindingIndexProp.intValue = index;
                    }

                    property.serializedObject.ApplyModifiedProperties();
                };

                GUIContent fieldText = new GUIContent("None (InputReference)");
                if (!string.IsNullOrEmpty(actionNameProp.stringValue))
                {
                    fieldText.text = actionNameProp.stringValue + $" [{bindingIndexProp.intValue}]";
                    fieldText.image = InputActionIcon;
                }

                if (EditorDrawing.ObjectField(position, fieldText))
                {
                    inputPicker.Show(dropdownRect, 370);
                }
            }
            EditorGUI.EndProperty();
        }
    }
}
