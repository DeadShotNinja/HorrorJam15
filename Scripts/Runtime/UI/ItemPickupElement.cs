using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace HJ.Runtime
{
    public class ItemPickupElement : MonoBehaviour
    {
        [Header("Dependencies")]
        [SerializeField] private TMP_Text _pickupText;
        [SerializeField] private Image _pickupIcon;

        [Header("Fit Settings")]
        [SerializeField] private bool _fitIcon = true;
        [SerializeField] private float _fitSize = 50f;

        [Header("Animation")]
        [SerializeField] private Animator _animator;
        [SerializeField] private string _showAnimation = "Show";
        [SerializeField] private string _hideAnimation = "Hide";

        public void ShowItemPickup(string text, Sprite icon, float time)
        {
            _pickupText.text = text;
            _pickupIcon.sprite = icon;

            Vector2 slotSize = Vector2.one * _fitSize;
            Vector2 iconSize = icon.rect.size;

            Vector2 scaleRatio = slotSize / iconSize;
            float scaleFactor = Mathf.Min(scaleRatio.x, scaleRatio.y);
            _pickupIcon.rectTransform.sizeDelta = iconSize * scaleFactor;

            StartCoroutine(OnShowPickupElement(time));
        }

        IEnumerator OnShowPickupElement(float time)
        {
            _animator.SetTrigger(_showAnimation);
            yield return new WaitForAnimatorClip(_animator, _showAnimation);

            yield return new WaitForSeconds(time);

            _animator.SetTrigger(_hideAnimation);
            yield return new WaitForAnimatorClip(_animator, _hideAnimation);

            yield return new WaitForEndOfFrame();
            Destroy(gameObject);
        }
    }
}