using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEditor;
using HJ.Runtime;

namespace HJ.Editors
{
    [CustomEditor(typeof(SaveGameManager))]
    public class SaveGameManagerEditor : Editor
    {
        private SaveGameManager _manager;
        private string _saveFolderName;
        private bool _debugExpanded;

        private void OnEnable()
        {
            _manager = (SaveGameManager)target;
            _saveFolderName = "Save";
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();

            DrawPropertiesExcluding(serializedObject, "m_Script", "_worldSaveables", "_runtimeSaveables");

            GUIStyle headerStyle = new(EditorStyles.boldLabel)
            {
                fontSize = 11,
                alignment = TextAnchor.MiddleLeft,
                padding = new(5, 0, 0, 0)
            };

            headerStyle.normal.textColor = Color.white;

            EditorGUILayout.Space();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.BeginVertical(GUI.skin.box);
            {
                Rect searcherHeader = EditorGUILayout.GetControlRect(false, 25f);
                ColorUtility.TryParseHtmlString("#272727", out Color color);

                EditorGUI.DrawRect(searcherHeader, color);
                EditorGUI.LabelField(searcherHeader, "Saveables Searcher".ToUpper(), headerStyle);

                using (new EditorDrawing.IconSizeScope(15))
                {
                    EditorUtils.TrIconText($"World Saveables: {_manager.WorldSaveables.Count}", MessageType.Info, EditorStyles.miniBoldLabel);
                    EditorUtils.TrIconText($"Runtime Saveables: {_manager.RuntimeSaveables.Count}", MessageType.Info, EditorStyles.miniBoldLabel);
                }
                EditorGUILayout.Space();

                using (new EditorDrawing.BackgroundColorScope("#F7E987"))
                {
                    if (GUILayout.Button("Find Saveables", GUILayout.Height(25)))
                    {
                        System.Diagnostics.Stopwatch stopwatch = new System.Diagnostics.Stopwatch();
                        stopwatch.Start();

                        MonoBehaviour[] monos = FindObjectsOfType<MonoBehaviour>(true);
                        var saveables = from mono in monos
                                        let type = mono.GetType()
                                        where typeof(ISaveable).IsAssignableFrom(type) && !type.IsInterface && !type.IsAbstract
                                        let token = $"{type.Name}{SaveGameManager.TOKEN_SEPARATOR}{GUID.Generate()}"
                                        select new SaveGameManager.SaveablePair(token, mono);

                        _manager.WorldSaveables = saveables.ToList();
                        stopwatch.Stop();

                        EditorUtility.SetDirty(target);
                        Debug.Log($"<color=yellow>[Saveables Searcher]</color> Found {saveables.Count()} world saveables in {stopwatch.ElapsedMilliseconds}ms. <color=red>SAVE YOUR SCENE!</color>");
                    }
                }

                Rect resetButtonRect = searcherHeader;
                resetButtonRect.xMin = resetButtonRect.xMax - EditorGUIUtility.singleLineHeight;
                resetButtonRect.height = EditorGUIUtility.singleLineHeight;
                resetButtonRect.y += 4f;
                resetButtonRect.x -= 5f;

                GUIContent resetButtonIcon = EditorGUIUtility.TrIconContent("Refresh", "Reset Saveables");
                if (GUI.Button(resetButtonRect, resetButtonIcon, EditorStyles.iconButton))
                {
                    _manager.WorldSaveables.Clear();
                }
            }
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndVertical();

            EditorGUILayout.Space();

            if(EditorDrawing.BeginFoldoutBorderLayout(new GUIContent("Functions Debug (Runtime Only)"), ref _debugExpanded))
            {
                using (new EditorGUI.DisabledGroupScope(!Application.isPlaying))
                {
                    Rect saveGameRect = EditorGUILayout.GetControlRect();
                    Rect saveGameBtn = EditorGUI.PrefixLabel(saveGameRect, new GUIContent("Save Game"));
                    if (GUI.Button(saveGameBtn, new GUIContent("Save")))
                    {
                        SaveGameManager.SaveGame(false);
                    }

                    Rect loadGameRect = EditorGUILayout.GetControlRect();
                    Rect loadGameBtn = EditorGUI.PrefixLabel(loadGameRect, new GUIContent("Load Game"));

                    Rect loadGameText = loadGameBtn;
                    loadGameText.xMax *= 0.8f;
                    loadGameBtn.xMin = loadGameText.xMax + 2f;

                    _saveFolderName = EditorGUI.TextField(loadGameText, _saveFolderName);
                    if (GUI.Button(loadGameBtn, new GUIContent("Load")))
                    {
                        if (!string.IsNullOrEmpty(_saveFolderName))
                        {
                            SaveGameManager.GameLoadType = SaveGameManager.LoadType.LoadGameState;
                            SaveGameManager.LoadFolderName = _saveFolderName;

                            string sceneName = SceneManager.GetActiveScene().name;
                            SaveGameManager.LoadSceneName = sceneName;
                            SceneManager.LoadScene(SaveGameManager.LMS);
                        }
                    }
                }

                EditorDrawing.EndBorderHeaderLayout();
            }

            serializedObject.ApplyModifiedProperties();
        }

        private Texture2D MakeBackgroundTexture(int width, int height, Color color)
        {
            Color[] pixels = new Color[width * height];

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = color;
            }

            Texture2D backgroundTexture = new Texture2D(width, height);

            backgroundTexture.SetPixels(pixels);
            backgroundTexture.Apply();

            return backgroundTexture;
        }
    }
}