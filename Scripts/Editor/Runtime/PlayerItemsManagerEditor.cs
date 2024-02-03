using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(PlayerItemsManager))]
    public class PlayerItemsManagerEditor : InspectorEditor<PlayerItemsManager>
    {
        private ReorderableList _playerItemsList;

        public override void OnEnable()
        {
            base.OnEnable();
            _playerItemsList = new ReorderableList(serializedObject, Properties["_playerItems"], true, false, true, true);
            _playerItemsList.drawElementCallback += (rect, index, isActive, isFocused) =>
            {
                SerializedProperty element = Properties["_playerItems"].GetArrayElementAtIndex(index);
                string itemName = element.objectReferenceValue != null ? (element.objectReferenceValue as PlayerItemBehaviour).Name : "New Item";
                Rect elementRect = new Rect(rect.x, rect.y + 2f, rect.width, EditorGUIUtility.singleLineHeight);

                Rect labelRect = elementRect;
                labelRect.width = EditorGUIUtility.labelWidth;
                EditorGUI.LabelField(labelRect, new GUIContent($"<b>[{index}]</b> {itemName}"), EditorDrawing.Styles.RichLabel);

                Rect propertyRect = elementRect;
                propertyRect.x += EditorGUIUtility.labelWidth + 2f;
                propertyRect.xMax = rect.xMax;
                EditorGUI.PropertyField(propertyRect, element, GUIContent.none);
            };
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                if(EditorDrawing.BeginFoldoutBorderLayout(Properties["_playerItems"], new GUIContent("Player Items")))
                {
                    _playerItemsList.DoLayoutList();
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space();
                using (new EditorDrawing.BorderBoxScope(new GUIContent("Settings")))
                {
                    Properties.Draw("_antiSpamDelay");
                    Properties.Draw("_isItemsUsable");
                }
            }
            serializedObject.ApplyModifiedProperties();
        }
    }
}