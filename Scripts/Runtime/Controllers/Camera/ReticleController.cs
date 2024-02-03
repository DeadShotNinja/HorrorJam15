using System;
using UnityEngine;
using UnityEngine.UI;
using Object = UnityEngine.Object;
//using HJ.Attributes;
using Sirenix.OdinInspector;

namespace HJ.Runtime
{
    [Serializable]
    public sealed class Reticle
    {
        public Sprite Sprite;
        public Color Color = Color.white;
        public Vector2 Size = Vector2.one;
    }

    [RequireComponent(typeof(InteractController))]
    public class ReticleController : MonoBehaviour
    {
        [Header("Interact")]
        [SerializeField] private Reticle _defaultReticle;
        [SerializeField] private Reticle _interactReticle;
        [SerializeField] private bool _dynamicReticle = true;
        [SerializeField] private float _changeTime = 0.05f;

        [Header("Custom Reticles")]
        // TODO: figure out bug with this validation
        //[RequireInterface(typeof(IReticleProvider))]
        [InfoBox("These MUST implement IReticleProvider interface!")]
        public Object[] ReticleProviders;

        private InteractController _interactController;
        private RectTransform _crosshairRect;
        private Image _crosshairImage;

        private CustomInteractReticle _holdReticle;
        private Vector2 _crosshairChangeVel;
        private bool _resetReticle;

        private void Awake()
        {
            _interactController = GetComponent<InteractController>();
            GameManager gameManager = GameManager.Instance;
            _crosshairImage = gameManager.ReticleImage;
            _crosshairRect = gameManager.ReticleImage.rectTransform;
        }

        private void Update()
        {
            if(_interactController.RaycastObject != null || _holdReticle != null)
            {
                GameObject raycastObject = _interactController.RaycastObject;
                OnChangeReticle(raycastObject);
            }
            else
            {
                OnChangeReticle(null);
            }
        }

        private void OnChangeReticle(GameObject raycastObject)
        {
            CustomInteractReticle customReticle = null;
            if (raycastObject != null && raycastObject.TryGetComponent(out customReticle) || _holdReticle != null)
            {
                CustomInteractReticle reticleProvider = _holdReticle != null ? _holdReticle : customReticle;

                IReticleProvider customProvider = _holdReticle != null ? _holdReticle : customReticle;
                var (_, reticle, hold) = customProvider.OnProvideReticle();

                if (hold) _holdReticle = reticleProvider;
                else _holdReticle = null;

                ChangeReticle(reticle);
                _resetReticle = true;
                return;
            }

            bool customReticleFlag = false;
            foreach (var provider in ReticleProviders)
            {
                IReticleProvider reticleProvider = provider as IReticleProvider;
                var (targetType, reticle, hold) = reticleProvider.OnProvideReticle();

                if(targetType == null || reticle == null)
                    continue;

                if (raycastObject != null && raycastObject.TryGetComponent(targetType, out _) || hold)
                {
                    ChangeReticle(reticle);
                    customReticleFlag = true;
                    break;
                }
            }

            if (!customReticleFlag)
            {
                if (_resetReticle)
                {
                    _crosshairImage.color = Color.white;
                    _crosshairRect.sizeDelta = _defaultReticle.Size;
                    _resetReticle = false;
                }

                if(raycastObject != null)
                {
                    if (_dynamicReticle)
                    {
                        _crosshairImage.sprite = _interactReticle.Sprite;
                        _crosshairImage.color = _interactReticle.Color;
                        _crosshairRect.sizeDelta = Vector2.SmoothDamp(_crosshairRect.sizeDelta, _interactReticle.Size, ref _crosshairChangeVel, _changeTime);
                    }
                    else
                    {
                        ChangeReticle(_interactReticle);
                    }
                }
                else
                {
                    if (_dynamicReticle)
                    {
                        _crosshairImage.sprite = _defaultReticle.Sprite;
                        _crosshairImage.color = _defaultReticle.Color;
                        _crosshairRect.sizeDelta = Vector2.SmoothDamp(_crosshairRect.sizeDelta, _defaultReticle.Size, ref _crosshairChangeVel, _changeTime);
                    }
                    else
                    {
                        ChangeReticle(_defaultReticle);
                    }
                }
            }
            else
            {
                _resetReticle = true;
            }
        }

        private void ChangeReticle(Reticle reticle)
        {
            if (reticle != null)
            {
                _crosshairImage.sprite = reticle.Sprite;
                _crosshairImage.color = reticle.Color;
                _crosshairRect.sizeDelta = reticle.Size;
            }
            else
            {
                _crosshairImage.sprite = null;
                _crosshairImage.color = Color.white;
                _crosshairRect.sizeDelta = Vector2.zero;
            }
        }
    }
}