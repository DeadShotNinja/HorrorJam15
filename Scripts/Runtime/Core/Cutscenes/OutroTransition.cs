using System.Collections;
using HJ.Tools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HJ.Runtime
{
    public class OutroTransition : MonoBehaviour
    {
        [SerializeField] private Image _fadeBG;
        [SerializeField] private float _fadeSpeed;
        [SerializeField] private string _creditsSceneName;
        
        private void OnEnable()
        {
            StartCoroutine(StartTransition());
        }
        
        private IEnumerator StartTransition()
        {
            Color color = Color.black;
            color.a = 0f;
            _fadeBG.color = color;
            
            while (_fadeBG.color.a < 1f)
            {
                color = _fadeBG.color;
                color.a += _fadeSpeed * Time.deltaTime;
                _fadeBG.color = color;
                yield return null;
            }

            GameTools.ShowCursor(false, true);
            AudioManager.PostAudioEvent(AudioAmbience.StopMainAmbiences, AudioManager.Instance.gameObject);
            SceneManager.LoadScene(_creditsSceneName);
        }
    }
}
