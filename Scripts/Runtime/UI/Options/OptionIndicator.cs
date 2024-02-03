using UnityEngine;
using UnityEngine.UI;

namespace HJ.Runtime
{
    public class OptionIndicator : MonoBehaviour
    {
        [SerializeField] private Image[] _indicators;

        [Header("Colors")]
        [SerializeField] private Color _enabledColor = Color.white;
        [SerializeField] private Color _disabledColor = Color.white;

        public void SetIndicator(int index)
        {
            for (int i = 0; i < _indicators.Length; i++)
            {
                var indicator = _indicators[i];
                indicator.color = i == index
                    ? _enabledColor : _disabledColor;
            }
        }
    }
}