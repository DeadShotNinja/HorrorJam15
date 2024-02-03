using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HJ
{
    public class SceneLoader : MonoBehaviour
    {
        [SerializeField] private GameObject _mainMenu;
        [SerializeField] private GameObject _loadingScreen;

        [SerializeField] private Slider _slider;

        public void LoadSceneAsync(string sceneName)
        {
            _loadingScreen.SetActive(true);
            _mainMenu.SetActive(false);

            StartCoroutine(Load(sceneName));
        }

        private IEnumerator Load(string sceneName)
        {
            var loading = SceneManager.LoadSceneAsync(sceneName);

            while (!loading.isDone)
            {
                _slider.value = loading.progress;
                yield return null;
            }
        }
    }
}
