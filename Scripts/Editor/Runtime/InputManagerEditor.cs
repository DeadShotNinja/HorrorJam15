using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEditor;
using HJ.Input;
using HJ.Tools;
using Binding = HJ.Input.Binding;

namespace HJ.Editors
{
    [CustomEditor(typeof(InputManager))]
    public class InputManagerEditor : InspectorEditor<InputManager>
    {
        private readonly List<bool> _mapFoldouts = new();
        private readonly List<bool> _actionFoldouts = new();
        private readonly List<bool> _rebindFoldouts = new();
        private readonly Dictionary<string, bool> _actionBindings = new();

        private readonly bool[] _foldouts = new bool[3];
        private string _actionName;
        private int _bindingIndex;

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            Properties.Draw("_inputActions");
            Properties.Draw("_inputSpritesAsset");
            Properties.Draw("_debugMode");

            EditorGUILayout.Space();
            if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Functions Debug (Runtime Only)"), ref _foldouts[0]))
            {
                using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
                {
                    _actionName = EditorGUILayout.TextField("Action Name", _actionName);
                    _bindingIndex = EditorGUILayout.IntField("Binding Index", _bindingIndex);
                    if (GUILayout.Button("Start Rebind Operation"))
                    {
                        InputManager.StartRebindOperation(_actionName, _bindingIndex);
                    }
                    if(GUILayout.Button("Apply Prepared Rebinds"))
                    {
                        InputManager.SetInputRebindOverrides();
                    }
                }

                EditorDrawing.EndBorderHeaderLayout();
            }

            if (Application.isPlaying)
            {
                EditorGUILayout.Space(1);
                if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Input Action Map"), ref _foldouts[1]))
                {
                    if (_mapFoldouts.Count < Target.ActionMaps.Count)
                        _mapFoldouts.AddRange(new bool[Target.ActionMaps.Count]);

                    int mapIndex = 0;
                    foreach (var map in Target.ActionMaps)
                    {
                        if (_mapFoldouts[mapIndex] = EditorDrawing.BeginFoldoutBorderLayout(new GUIContent(map.Key), _mapFoldouts[mapIndex++]))
                        {
                            if (_actionFoldouts.Count < map.Value.Actions.Count)
                                _actionFoldouts.AddRange(new bool[map.Value.Actions.Count]);

                            int actionIndex = 0;
                            foreach (var action in map.Value.Actions)
                            {
                                if (_actionFoldouts[actionIndex] = EditorDrawing.BeginFoldoutBorderLayout(new GUIContent(action.Key), _actionFoldouts[actionIndex++]))
                                {
                                    foreach (var binding in action.Value.Bindings)
                                    {
                                        string bindingKey = action.Key + "_" + binding.Value.BindingIndex;
                                        if (!_actionBindings.ContainsKey(bindingKey))
                                            _actionBindings.Add(bindingKey, false);

                                        string name = !string.IsNullOrEmpty(binding.Value.Name)
                                            ? $"Binding Index [{binding.Value.BindingIndex}] [{binding.Value.Name.ToTitleCase()}]"
                                            : $"Binding Index [{binding.Value.BindingIndex}]";

                                        if (_actionBindings[bindingKey] = EditorDrawing.BeginFoldoutBorderLayout(new GUIContent(name), _actionBindings[bindingKey]))
                                        {
                                            DrawBinding(binding.Value);
                                            EditorDrawing.EndBorderHeaderLayout();
                                        }
                                    }
                                    EditorDrawing.EndBorderHeaderLayout();
                                }
                            }
                            EditorDrawing.EndBorderHeaderLayout();
                        }
                    }
                    EditorDrawing.EndBorderHeaderLayout();
                }

                EditorGUILayout.Space(1);
                if (Target.PreparedRebinds.Count > 0)
                {
                    if (_rebindFoldouts.Count != Target.PreparedRebinds.Count)
                    {
                        _rebindFoldouts.Clear();
                        _rebindFoldouts.AddRange(new bool[Target.PreparedRebinds.Count]);
                    }

                    if (EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Prepared Rebinds"), ref _foldouts[2]))
                    {
                        int rebindIndex = 0;
                        foreach (var rebind in Target.PreparedRebinds)
                        {
                            InputBinding inputBinding = rebind.Action.bindings[rebind.BindingIndex];
                            string actionName = rebind.Action.name;
                            string bindingName = inputBinding.name;

                            if (!string.IsNullOrEmpty(bindingName))
                                actionName += "." + bindingName;

                            if (_rebindFoldouts[rebindIndex] = EditorDrawing.BeginFoldoutBorderLayout(new GUIContent(actionName), _rebindFoldouts[rebindIndex++]))
                            {
                                using (new EditorGUI.DisabledGroupScope(true))
                                {
                                    EditorGUILayout.TextField("Binding Index", rebind.BindingIndex.ToString());
                                    EditorGUILayout.TextField("Override Path", rebind.OverridePath);
                                }
                                EditorDrawing.EndBorderHeaderLayout();
                            }
                        }
                        EditorDrawing.EndBorderHeaderLayout();
                    }
                }
                else
                {
                    _rebindFoldouts.Clear();
                }
            }

            serializedObject.ApplyModifiedProperties();
        }

        private void DrawBinding(Binding binding)
        {
            using (new EditorGUI.DisabledGroupScope(true))
            {
                EditorGUILayout.TextField("Binding Path", binding.BindingPath.BindedPath);
                EditorGUILayout.TextField("Override Path", binding.BindingPath.OverridePath);
                EditorGUILayout.TextField("Effective Path", binding.BindingPath.EffectivePath);
                EditorGUILayout.TextField("Glyph Path", binding.BindingPath.InputGlyph.GlyphPath);
            }
        }
    }
}