using UnityEngine;
using UnityEngine.UI;

namespace HJ.Runtime
{
    public class FloatingIcon : MonoBehaviour
    {
        private float _fadeTime;
        private Image _iconImage;

        private float _targetFade = -1f;
        private float _fadeVelocity;

        private void Awake()
        {
            _iconImage = GetComponent<Image>();
            _targetFade = -1f;
        }

        public void FadeIn(float fadeTime)
        {
            if (!_iconImage) return;

            _fadeTime = fadeTime;
            _targetFade = 1f;

            Color color = _iconImage.color;
            color.a = 0f;
            _iconImage.color = color;
        }

        public void FadeOut(float fadeTime)
        {
            _fadeTime = fadeTime;
            _targetFade = 0f;
        }

        private void Update()
        {
            if (_targetFade >= 0f && _iconImage)
            {
                Color color = _iconImage.color;
                color.a = Mathf.SmoothDamp(color.a, _targetFade, ref _fadeVelocity, _fadeTime);
                _iconImage.color = color;

                if (color.a < 0.01f && _targetFade == 0f)
                    Destroy(gameObject);
            }
        }
    }
}