using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace HJ.Runtime
{
    public class CreditsController : MonoBehaviour
    {
        [Header("Credits")]
        [SerializeField] private RectTransform _creditsHolder;
        [SerializeField] private float _scrollSpeed;
        
        [Header("Fading")]
        [SerializeField] private Image _fadeImage;
        [SerializeField] private float _fadeSpeed;

        [Header("Scene Transition")]
        [SerializeField] private string _mainMenuScene;

        private float _initialPosY;
        private float _endPosY;
        
        private void Start()
        {
            float canvasHeight = _creditsHolder.parent.GetComponent<RectTransform>().rect.height;

            _initialPosY = -canvasHeight / 2 - _creditsHolder.rect.height / 2;
            _creditsHolder.anchoredPosition = new Vector2(_creditsHolder.anchoredPosition.x, _initialPosY);
            _endPosY = canvasHeight / 2 + _creditsHolder.rect.height / 2;

            Color startingColor = _fadeImage.color;
            startingColor.a = 1f;
            _fadeImage.color = startingColor;
            
            ScrollCredits();
        }
        
        private void ScrollCredits()
        {
            StartCoroutine(FadeScreen(false));
        }
        
        private IEnumerator FadeScreen(bool fadeIn)
        {
            Color color = _fadeImage.color;
            float targetAlpha = fadeIn ? 1f : 0f;
            float fadeStep = fadeIn ? _fadeSpeed : -_fadeSpeed;
            
            while ((fadeIn && _fadeImage.color.a < 1f) || (!fadeIn && _fadeImage.color.a > 0f))
            {
                color.a += fadeStep * Time.deltaTime;
                _fadeImage.color = color;
                yield return null;
            }

            color.a = targetAlpha;
            _fadeImage.color = color;

            if (!fadeIn)
                StartCoroutine(Scroll_Coroutine());
            else
                GoToMainMenu();
        }
        
        private IEnumerator Scroll_Coroutine()
        {
            while (_creditsHolder.anchoredPosition.y < _endPosY)
            {
                _creditsHolder.anchoredPosition += new Vector2(
                    0f, _scrollSpeed * Time.deltaTime);
                yield return null;
            }

            StartCoroutine(FadeScreen(true));
        }
        
        private void GoToMainMenu()
        {
            SceneManager.LoadScene(_mainMenuScene);
        }
    }
}
