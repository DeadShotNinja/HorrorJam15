using System;
using HJ.Input;
using UnityEngine;
using UnityEngine.UI;

namespace HJ.Runtime
{
    public class HotspotKey : MonoBehaviour
    {
        [Header("Setup")]
        [SerializeField] private InputReference _useKey;
        [SerializeField] private Image _hotspotSprite;

        private IDisposable _disposable;

        private void Awake()
        {
            if (!InputManager.HasReference)
                return;

            _disposable = InputManager.GetBindingPath(_useKey.ActionName, _useKey.BindingIndex)
                .GlyphSpriteObservable.Subscribe(icon => _hotspotSprite.sprite = icon);
        }

        private void OnDestroy()
        {
            _disposable.Dispose();
        }
    }
}