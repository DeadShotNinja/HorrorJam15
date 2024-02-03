using System.Linq;
using UnityEngine;
using UnityEditor;
using HJ.Runtime;
using HJ.Scriptable;
using HJ.Attributes;

namespace HJ.Editors
{
    [CustomPropertyDrawer(typeof(PlayerStatePickerAttribute))]
    public class PlayerStateDrawer : PropertyDrawer
    {
        private const string DEFAULT = MotionBlender.Default;
        private readonly string[] _avaiableStates;

        public PlayerStateDrawer()
        {
            var types = TypeCache.GetTypesDerivedFrom<PlayerStateAsset>().Where(x => !x.IsAbstract);
            _avaiableStates = new string[types.Count()];
            int index = 0;

            foreach (var type in types)
            {
                PlayerStateAsset stateAsset = (PlayerStateAsset)ScriptableObject.CreateInstance(type);
                _avaiableStates[index++] = stateAsset.GetStateKey();
                Object.DestroyImmediate(stateAsset);
            }
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            PlayerStatePickerAttribute att = attribute as PlayerStatePickerAttribute;

            if (property.propertyType != SerializedPropertyType.String)
            {
                EditorGUI.LabelField(position, label.text, "PlayerState Attribute can only be used at string type fields!");
                return;
            }

            EditorGUI.BeginProperty(position, label, property);
            {
                string[] states = _avaiableStates;
                if (att.IncludeDefault) states = new string[] { DEFAULT }.Concat(_avaiableStates).ToArray();

                GUIContent title = new(DEFAULT);
                string selected = property.stringValue;

                if (string.IsNullOrEmpty(property.stringValue))
                    title = new("None");
                else if (!states.Contains(selected)) 
                    selected = "Missing State";

                Rect popupRect = EditorGUI.PrefixLabel(position, label);
                EditorDrawing.DrawStringSelectPopup(popupRect, title, states, selected, (state) =>
                {
                    property.stringValue = state;
                    property.serializedObject.ApplyModifiedProperties();
                });
            }
            EditorGUI.EndProperty();
        }
    }
}
