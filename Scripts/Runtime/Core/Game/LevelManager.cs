using System;
using System.Collections;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using HJ.Input;
using HJ.Tools;
using TMText = TMPro.TMP_Text;
using static HJ.Runtime.SaveGameManager;

namespace HJ.Runtime
{
    public class LevelManager : MonoBehaviour
    {
        [Serializable]
        public struct LevelInfo
        {
            public string SceneName;
            public GString Title;
            public GString Description;
            public Sprite Background;
        }

        [SerializeField] private LevelInfo[] _levelInfos;

        [SerializeField] private TMText _title;
        [SerializeField] private TMText _description;
        [SerializeField] private Image _background;
        [SerializeField] private BackgroundFader _fadingBackground;

        /// <summary>
        /// Priority of background loading thread.
        /// </summary>
        [SerializeField] private ThreadPriority _loadPriority = ThreadPriority.High;

        [SerializeField] private float _fadeSpeed;
        [SerializeField] private bool _switchManually;
        [SerializeField] private bool _fadeBackground;
        [SerializeField] private bool _debugging;

        [SerializeField] private bool _switchPanels;
        [SerializeField] private float _switchFadeSpeed;
        [SerializeField] private CanvasGroup _currentPanel;
        [SerializeField] private CanvasGroup _newPanel;

        [SerializeField] private UnityEvent<float> _onProgressUpdate;
        [SerializeField] private UnityEvent _onLoadingDone;

        private void Start()
        {
            Time.timeScale = 1f;
            Application.backgroundLoadingPriority = _loadPriority;

            string sceneName = LoadSceneName;
            if (!string.IsNullOrEmpty(sceneName))
            {
                foreach (var info in _levelInfos)
                {
                    if(info.SceneName == sceneName)
                    {
                        info.Title.SubscribeGloc();
                        info.Description.SubscribeGloc();

                        _background.sprite = info.Background;
                        _description.text = info.Description;
                        _title.text = info.Title;
                        break;
                    }
                }

                StartCoroutine(LoadLevelAsync(sceneName));
            }
        }

        private IEnumerator LoadLevelAsync(string sceneName)
        {
            yield return _fadingBackground.StartBackgroundFade(true, fadeSpeed: _fadeSpeed);
            yield return new WaitForEndOfFrame();

            AsyncOperation asyncOp = SceneManager.LoadSceneAsync(sceneName);
            asyncOp.allowSceneActivation = false;

            while (!asyncOp.isDone)
            {
                float progress = asyncOp.progress / 0.9f;
                _onProgressUpdate?.Invoke(progress);

                if (progress >= 1f) break;
                yield return null;
            }

            yield return DeserializeSavedGame();

            if (_switchManually)
            {
                _onLoadingDone?.Invoke();

                if (_switchPanels)
                {
                    yield return CanvasGroupFader.StartFade(_currentPanel, false, _switchFadeSpeed);
                    yield return CanvasGroupFader.StartFade(_newPanel, true, _switchFadeSpeed);
                }

                yield return new WaitUntil(() => InputManager.AnyKeyPressed());

                if (_fadeBackground)
                {
                    yield return _fadingBackground.StartBackgroundFade(false, fadeSpeed: _fadeSpeed);
                    yield return new WaitForEndOfFrame();
                }
            }

            asyncOp.allowSceneActivation = true;
            yield return null;
        }

        private IEnumerator DeserializeSavedGame()
        {
            if (GameLoadType == LoadType.Normal || string.IsNullOrEmpty(LoadFolderName))
                yield return null;

            string saveFolder = string.Empty;
            if(GameLoadType == LoadType.LoadGameState)
            {
                saveFolder = LoadFolderName;
            }
            else if (GameLoadType == LoadType.LoadWorldState && SerializationAsset.PreviousScenePersistency)
            {
                if (LastSceneSaves == null)
                {
                    if (_debugging) Debug.Log("[LevelManager] LastSceneSaves are empty. Trying to load the last scene saves.");
                    {
                        Task getLastScenesTask = LoadLastSceneSaves();
                        yield return new WaitToTaskComplete(getLastScenesTask);
                    }
                    if (_debugging) Debug.Log("[LevelManager] The last scene saves was successfully loaded.");
                }

                LastSceneSaves.TryGetValue(LoadSceneName, out saveFolder);
            }

            if (!string.IsNullOrEmpty(saveFolder))
            {
                if (_debugging) Debug.Log($"[LevelManager] Trying to deserialize a save with the name '{saveFolder}'.");
                {
                    Task deserializeTask = TryDeserializeGameStateAsync(saveFolder);
                    yield return new WaitToTaskComplete(deserializeTask);
                }
                if (_debugging) Debug.Log($"[LevelManager] The save was successfully deserialized. ");
            }
        }
    }
}