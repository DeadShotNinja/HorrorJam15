using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;
using UnityEditor.Events;
using TMPro;
using HJ.Runtime;
using HJ.Scriptable;

namespace HJ.Editors
{
    public class GStringExtension : EditorWindow
    {
        private const string LASTKEY = "GStringExtension.LastKey";

        private TextMeshProUGUI _textMesh;
        private GameLocalizationAsset _localizationAsset;

        private WindowProperties _propertiesWindow;
        private SerializedObject _serializedObject;
        private PropertyCollection _properties;

        [MenuItem("CONTEXT/TextMeshProUGUI/GString Localize")]
        static void GStringLocalize(MenuCommand command)
        {
            if (command.context is not TextMeshProUGUI textMesh)
                return;

            EditorWindow window = GetWindow<GStringExtension>(false, "TextMeshPro to GString", true);
            window.minSize = new Vector2(500, 150);
            window.maxSize = new Vector2(500, 150);
            (window as GStringExtension).Show(textMesh);
        }

        public void Show(TextMeshProUGUI textMesh)
        {
            this._textMesh = textMesh;
            _localizationAsset = GameLocalization.Instance.LocalizationAsset;

            _propertiesWindow = CreateInstance<WindowProperties>();
            _serializedObject = new SerializedObject(_propertiesWindow);
            _properties = EditorDrawing.GetAllProperties(_serializedObject);

            if (EditorPrefs.HasKey(LASTKEY))
            {
                string lastKey = EditorPrefs.GetString(LASTKEY);
                string section = lastKey.Split('.')[0];
                string newKey = textMesh.text.ToLower().Replace(" ", ".");

                _propertiesWindow.LocalizationKey.GlocText = section + "." + newKey;
                _serializedObject.ApplyModifiedProperties();
                _serializedObject.Update();
            }
        }

        private void OnDestroy()
        {
            EditorPrefs.SetString(LASTKEY, _propertiesWindow.LocalizationKey.GlocText);

            _properties = null;
            _serializedObject = null;
            DestroyImmediate(_propertiesWindow);
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
                EditorGUILayout.BeginVertical(GUI.skin.box);
                {
                    _localizationAsset = (GameLocalizationAsset)EditorGUILayout.ObjectField(new GUIContent("GameLocalization Asset"), _localizationAsset, typeof(GameLocalizationAsset), false);
                    _properties.Draw("LocalizationKey");
                }
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space();

                string gLocKey = _propertiesWindow.LocalizationKey.GlocText;
                EditorGUILayout.BeginHorizontal();
                {
                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Assign Key", GUILayout.Height(30f), GUILayout.Width(240)))
                    {
                        AssignKey(false);
                        Debug.Log($"The localization key '{gLocKey}' has been assigned and linked to the Text Component.");
                    }

                    GUILayout.FlexibleSpace();

                    if (GUILayout.Button("Create & Assign Key", GUILayout.Height(30f), GUILayout.Width(240)))
                    {
                        AssignKey(true);
                        Debug.Log($"The localization key '{gLocKey}' has been added to the localization asset and linked to the Text Component.");
                    }
                }
                EditorGUILayout.EndHorizontal();
            }
            GUILayout.EndArea();
        }

        private async void AssignKey(bool create)
        {
            string gLocKey = _propertiesWindow.LocalizationKey.GlocText;

            if (string.IsNullOrEmpty(gLocKey))
                return;

            if (!_textMesh.gameObject.TryGetComponent(out GLocText gLoc))
                gLoc = _textMesh.gameObject.AddComponent<GLocText>();

            await WaitForGLocNull(gLoc);

            gLoc.GlocKey.GlocText = gLocKey;

            if (create)
            {
                string text = _textMesh.text;
                _localizationAsset.AddSectionKey(gLocKey, text);
            }

            var setStringMethod = _textMesh.GetType().GetProperty("text").GetSetMethod();
            var methodDelegate = Delegate.CreateDelegate(typeof(UnityAction<string>), _textMesh, setStringMethod) as UnityAction<string>;
            UnityEventTools.AddPersistentListener(gLoc.OnUpdateText, methodDelegate);
            gLoc.OnUpdateText.SetPersistentListenerState(0, UnityEventCallState.RuntimeOnly);
        }

        private async Task WaitForGLocNull(GLocText gloc)
        {
            while (gloc.GlocKey == null)
            {
                await Task.Yield();
            }
        }

        public sealed class WindowProperties : ScriptableObject
        {
            public GString LocalizationKey;
        }
    }
}