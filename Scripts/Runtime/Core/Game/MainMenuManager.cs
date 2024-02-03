using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HJ.Runtime
{
    public class MainMenuManager : MonoBehaviour
    {
        [SerializeField] private BackgroundFader _backgroundFader;
        [SerializeField] private string _newGameSceneName;

        private void Start()
        {
            PlayerPrefs.SetInt("IntroCutscenePlayed", 0);
            PlayerPrefs.SetInt("EndGame", 0);
        }

        public void NewGame()
        {
            if (string.IsNullOrEmpty(_newGameSceneName))
                throw new System.NullReferenceException("The new game scene name field is empty!");

            StartCoroutine(LoadNewGame());
        }

        IEnumerator LoadNewGame()
        {
            yield return _backgroundFader.StartBackgroundFade(false);
            yield return new WaitToTaskComplete(SaveGameManager.RemoveAllSaves());
            SaveGameManager.LoadSceneName = _newGameSceneName;
            SceneManager.LoadScene(SaveGameManager.LMS);
        }

        public void QuitGame()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
             Application.Quit();
#endif
        }
    }
}