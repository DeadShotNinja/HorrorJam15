using System;
using System.Linq;
using HJ.Input;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using HJ.Scriptable;

namespace HJ.Editors
{
    [CustomEditor(typeof(InputSpritesAsset))]
    public class InputSpritesAssetEditor : Editor
    {
        private InputSpritesAsset _asset;
        private SerializedProperty _spriteAsset;
        private SerializedProperty _glyphMap;

        private int _currPage = 0;
        private bool _unassignedFoldout = false;

        private void OnEnable()
        {
            _asset = target as InputSpritesAsset;
            _spriteAsset = serializedObject.FindProperty("SpriteAsset");
            _glyphMap = serializedObject.FindProperty("GlyphMap");
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            {
                EditorGUILayout.PropertyField(_spriteAsset);
                EditorGUILayout.Space();

                using (new EditorGUI.DisabledGroupScope(_spriteAsset.objectReferenceValue == null))
                {
                    if (GUILayout.Button("Refresh Glyph Map", GUILayout.Height(25)))
                    {
                        if (_glyphMap.arraySize > 0)
                        {
                            if (!EditorUtility.DisplayDialog("Refresh Glyph Map", $"Are you sure you want to refresh the glyph map?", "Yes", "No"))
                            {
                                return;
                            }
                        }

                        _glyphMap.arraySize = _asset.SpriteAsset.spriteGlyphTable.Count;
                        serializedObject.ApplyModifiedProperties();

                        for (int i = 0; i < _glyphMap.arraySize; i++)
                        {
                            _asset.GlyphMap[i].Glyph = _asset.SpriteAsset.spriteGlyphTable[i];
                            _asset.GlyphMap[i].Scale = Vector2.one;
                        }
                    }
                }

                if (GUILayout.Button("Clear Glyph Map", GUILayout.Height(25)))
                {
                    if (EditorUtility.DisplayDialog("Clear Glyph Maps", $"Are you sure you want to clear the glyph map?", "Yes", "No"))
                    {
                        _glyphMap.ClearArray();
                    }
                }

                if (GUILayout.Button("Save Asset", GUILayout.Height(25)))
                {
                    EditorUtility.SetDirty(_asset);
                    AssetDatabase.SaveAssetIfDirty(_asset);
                }

                EditorGUILayout.Space();

                int itemsPerPage = 10;
                int arraySize = _glyphMap.arraySize;
                int totalPages = (int)(arraySize / (float)itemsPerPage + 0.999f);

                using (new EditorDrawing.BorderBoxScope(new GUIContent("Glyph Map")))
                {
                    if(totalPages > 0)
                    {
                        for (int i = 0; i < 10; i++)
                        {
                            int index = (_currPage * itemsPerPage) + i;
                            if (index >= arraySize)
                                break;

                            SerializedProperty glyph = _glyphMap.GetArrayElementAtIndex(index);
                            DrawGlyphElement(glyph, index);
                        }

                        Rect pagePos = EditorGUILayout.GetControlRect(false, 20);
                        pagePos.width /= 3;

                        if (GUI.Button(pagePos, "Previous Page"))
                        {
                            _currPage = _currPage > 0 ? _currPage - 1 : 0;
                        }

                        GUIStyle centeredLabel = new GUIStyle(EditorStyles.label) { alignment = TextAnchor.MiddleCenter };
                        pagePos.x += pagePos.width;
                        GUI.Label(pagePos, "Page " + (_currPage + 1) + " / " + totalPages, centeredLabel);

                        pagePos.x += pagePos.width;
                        if (GUI.Button(pagePos, "Next Page"))
                        {
                            _currPage = _currPage < (totalPages - 1) ? _currPage + 1 : (totalPages - 1);
                        }
                    }
                    else
                    {
                        EditorGUILayout.LabelField("Glyph Map is empty!", EditorDrawing.CenterStyle(EditorStyles.label));
                    }
                }

                EditorGUILayout.Space();
                if(totalPages > 0 && EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Unassigned Keys"), ref _unassignedFoldout))
                {
                    string[] controlKeys = InputSpritesAsset.AllKeys.Except(from map in _asset.GlyphMap
                                                                               from key in map.MappedKeys
                                                                               select key).ToArray();

                    EditorGUILayout.HelpBox(string.Join(", ", controlKeys
                        .Select(x => InputControlPath.ToHumanReadableString(x, InputControlPath.HumanReadableStringOptions.OmitDevice))
                        .Select(x => char.ToUpper(x[0]) + x[1..])), MessageType.None);
                    EditorDrawing.EndBorderHeaderLayout();
                }
            }
            serializedObject.ApplyModifiedProperties();
        }

        private void DrawGlyphElement(SerializedProperty glyphProperty, int index)
        {
            GlyphKeysPair glyphKeyPair = _asset.GlyphMap[index];

            SerializedProperty glyph = glyphProperty.FindPropertyRelative("Glyph");
            SerializedProperty keys = glyphProperty.FindPropertyRelative("MappedKeys");
            SerializedProperty scale = glyphProperty.FindPropertyRelative("Scale");
            SerializedProperty sprite = glyph.FindPropertyRelative("sprite");
            Texture2D texture = AssetPreview.GetAssetPreview(sprite.objectReferenceValue);
            int glyphIndex = glyph.FindPropertyRelative("m_Index").intValue;

            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            {
                Rect glyphRect = GUILayoutUtility.GetRect(50, 50);
                GUI.Label(glyphRect, texture);

                Rect popupRect = glyphRect;
                popupRect.height = EditorGUIUtility.singleLineHeight;
                popupRect.xMin += 54f;
                popupRect.y += 8f;

                GUIContent popupTitle = new GUIContent("None");
                if(glyphKeyPair.MappedKeys.Length > 0)
                {
                    string[] selectedTitles = glyphKeyPair.MappedKeys.Select(x =>
                    {
                        string displayName = InputControlPath.ToHumanReadableString(x, InputControlPath.HumanReadableStringOptions.OmitDevice);
                        return char.ToUpper(displayName[0]) + displayName[1..];
                    }).ToArray();

                    popupTitle.text = string.Join(", ", selectedTitles.Take(5));
                    if (selectedTitles.Length > 5)
                        popupTitle.text += "...";
                }

                GlyphKeyPicker glyphKeyPicker = new(new AdvancedDropdownState());
                glyphKeyPicker.Selected = glyphKeyPair.MappedKeys;
                glyphKeyPicker.OnItemPressed += path =>
                {
                    if (path == "none") keys.ClearArray();
                    else
                    {
                        string[] selected = glyphKeyPair.MappedKeys;
                        _asset.GlyphMap[index].MappedKeys = selected.Contains(path)
                            ? selected.Except(new string[] { path }).ToArray()
                            : selected.Concat(new string[] { path }).ToArray();
                    }

                    serializedObject.ApplyModifiedProperties();
                };

                if (GUI.Button(popupRect, popupTitle, EditorStyles.popup))
                {
                    Rect glyphPickerRect = popupRect;
                    glyphPickerRect.width = 250;
                    glyphKeyPicker.Show(glyphPickerRect, 370);
                }

                Rect glyphScaleRect = popupRect;
                glyphScaleRect.y += EditorGUIUtility.singleLineHeight + 2f;
                glyphScaleRect.xMax = glyphScaleRect.xMax / 2f + 100f;
                EditorGUI.PropertyField(glyphScaleRect, scale, GUIContent.none);

                GUIContent indexLabel = new("ID: " + glyphIndex);
                float labelWidth = EditorStyles.boldLabel.CalcSize(indexLabel).x;

                Rect glyphIndexRect = popupRect;
                glyphIndexRect.y += EditorGUIUtility.singleLineHeight + 2f;
                glyphIndexRect.xMin = glyphIndexRect.xMax - labelWidth - 2f;
                EditorGUI.LabelField(glyphIndexRect, indexLabel, EditorStyles.boldLabel);
            }
            EditorGUILayout.EndVertical();
        }

        private class GlyphKeyPicker : AdvancedDropdown
        {
            private class GlyphKeyElement : AdvancedDropdownItem
            {
                public string controlPath;
                public string displayName;

                public GlyphKeyElement(string controlPath, string displayName) : base(displayName)
                {
                    this.controlPath = controlPath;
                    this.displayName = displayName;
                }
            }

            public string[] Selected;
            public event Action<string> OnItemPressed;

            public GlyphKeyPicker(AdvancedDropdownState state) : base(state)
            {

                minimumSize = new Vector2(200f, 250f);
            }

            protected override AdvancedDropdownItem BuildRoot()
            {
                var root = new AdvancedDropdownItem("Input Controls");
                root.AddChild(new GlyphKeyElement("none", "None [Deselect All]"));

                var invalidElement = new GlyphKeyElement("invalid", "Invalid [Null Key]");
                if(Selected.Contains("invalid")) 
                    invalidElement.icon = (Texture2D)EditorGUIUtility.TrIconContent("FilterSelectedOnly").image;
                root.AddChild(invalidElement);

                foreach (var path in InputSpritesAsset.AllKeys)
                {
                    string displayName = InputControlPath.ToHumanReadableString(path);
                    displayName = char.ToUpper(displayName[0]) + displayName[1..];
                    var dropdownItem = new GlyphKeyElement(path, displayName);

                    if(Selected.Contains(path))
                        dropdownItem.icon = (Texture2D)EditorGUIUtility.TrIconContent("FilterSelectedOnly").image;

                    root.AddChild(dropdownItem);
                }

                return root;
            }

            protected override void ItemSelected(AdvancedDropdownItem item)
            {
                OnItemPressed?.Invoke((item as GlyphKeyElement).controlPath);
            }
        }
    }
}