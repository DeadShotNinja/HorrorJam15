using System.Collections;
using UnityEngine;
using TMPro;

namespace HJ.Runtime
{
    public class ObjectiveNotification : MonoBehaviour
    {
        [SerializeField] private Animator _animator;
        [SerializeField] private TMP_Text _title;

        [Header("Animation")]
        [SerializeField] private string _showTrigger = "Show";
        [SerializeField] private string _hideTrigger = "Hide";
        [SerializeField] private string _hideState = "Hide";

        private bool _isShowed;

        public void ShowNotification(string title, float duration)
        {
            if (_isShowed)
                return;

            _title.text = title;
            _animator.SetTrigger(_showTrigger);
            StartCoroutine(OnShowNotification(duration));
            _isShowed = true;
        }

        IEnumerator OnShowNotification(float duration)
        {
            yield return new WaitForSeconds(duration);
            _animator.SetTrigger(_hideTrigger);
            yield return new WaitForAnimatorStateExit(_animator, _hideState);
            _isShowed = false;
        }
    }
}