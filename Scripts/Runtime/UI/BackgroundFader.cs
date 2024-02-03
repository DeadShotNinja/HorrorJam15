using System.Collections;
using UnityEngine;
using UnityEngine.UI;

namespace HJ.Runtime
{
    public class BackgroundFader : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private Image _background;

        public IEnumerator StartBackgroundFade(bool fadeOut, float waitTime = 0, float fadeSpeed = 3)
        {
            yield return new WaitForEndOfFrame();

            if (fadeOut)
            {
                _background.enabled = true;

                yield return new WaitForSecondsRealtime(waitTime);
                Color color = _background.color;

                while (color.a > 0)
                {
                    color.a = Mathf.MoveTowards(color.a, 0, Time.deltaTime * fadeSpeed);
                    _background.color = color;
                    yield return null;
                }

                _background.enabled = false;
            }
            else
            {
                Color color = _background.color;
                color.a = 0;

                _background.color = color;
                _background.enabled = true;

                while (color.a < 1)
                {
                    color.a = Mathf.MoveTowards(color.a, 1, Time.deltaTime * fadeSpeed);
                    _background.color = color;
                    yield return null;
                }

                yield return new WaitForSecondsRealtime(waitTime);
            }
        }
    }
}