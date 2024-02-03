using System.Collections;
using UnityEngine;
using HJ.Tools;
using TMPro;

namespace HJ.Runtime
{
    public class TipsManager : MonoBehaviour
    {
        [SerializeField] private GString[] _tipsList;

        [Header("References")]
        [SerializeField] private CanvasGroup _tipsGroup;
        [SerializeField] private TMP_Text _tipText;

        [Header("Settings")]
        [SerializeField] private float _tipTime = 5f;
        [SerializeField] private float _tipChangeSpeed = 1f;

        private int _lastTip;

        private void Awake()
        {
            for (int i = 0; i < _tipsList.Length; i++)
            {
                _tipsList[i].SubscribeGloc();
            }
        }

        public void StopTips()
        {
            StopAllCoroutines();
        }

        IEnumerator Start()
        {
            if (_tipsList.Length == 1)
            {
                _tipText.text = _tipsList[0];
                _tipsGroup.alpha = 1f;
            }
            else if (_tipsList.Length > 1)
            {
                _tipsGroup.alpha = 0f;

                while (true)
                {
                    _lastTip = GameTools.RandomUnique(0, _tipsList.Length, _lastTip);
                    _tipText.text = _tipsList[_lastTip];

                    yield return CanvasGroupFader.StartFade(_tipsGroup, true, _tipChangeSpeed);
                    yield return new WaitForSeconds(_tipTime);
                    yield return CanvasGroupFader.StartFade(_tipsGroup, false, _tipChangeSpeed);
                }
            }
        }
    }
}