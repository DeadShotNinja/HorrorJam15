using System;
using Cinemachine;
using UnityEngine;
using UnityEngine.UI;
using HJ.Rendering;
using static HJ.Runtime.JumpscareTrigger;

namespace HJ.Runtime
{
    public class JumpscareManager : Singleton<JumpscareManager>
    {
        [SerializeField] private Image _directImage;

        [Header("Direct Jumpscare Settings")]
        [SerializeField, Range(1f, 2f)] private float _imageMaxScale = 2f;
        [SerializeField, Range(0f, 1f)] private float _imageScaleTime = 0.5f;

        [Header("Fear Effect Settings")]
        [SerializeField, Range(0f, 1f)] private float _fearIntensityDuration = 0.2f;
        [SerializeField, Range(0f, 1f)] private float _fearSpeedDuration = 0.2f;

        [Header("Tentacles Default Settings")]
        [SerializeField, Range(0.1f, 3f)] private float _tentaclesDefaultSpeed = 1f;
        [SerializeField, Range(-0.2f, 0.2f)] private float _tentaclesDefaultPosition = 0f;

        [Header("Tentacles Animation Settings")]
        [SerializeField] private float _tentaclesMoveSpeed = 1f;
        [SerializeField] private float _tentaclesAnimationSpeed = 1f;
        [SerializeField] private float _tentaclesFadeInSpeed = 1f;
        [SerializeField] private float _tentaclesFadeOutSpeed = 1f;

        [Header("Camera Wobble Settings")]
        [SerializeField] private float _wobbleLossRate = 0.5f;

        private PlayerPresenceManager _playerPresence;
        private LookController _lookController;
        private JumpscareDirect _jumpscareDirect;
        private GameManager _gameManager;

        private CinemachineBasicMultiChannelPerlin _cinemachineBasicMultiChannelPerlin;
        private FearTentancles _fearTentancles;
        private GameObject _directModel;

        private bool _isDirectJumpscare;
        private bool _isPlayerLocked;
        private bool _influenceFear;
        private bool _tentaclesFaded;
        private bool _showTentacles;

        private float _directDuration;
        private float _directTimer;
        private float _fearDuration;
        private float _fearTimer;
        private float _wobbleTimer;


        private void Awake()
        {
            _playerPresence = GetComponent<PlayerPresenceManager>();
            _lookController = _playerPresence.LookController;
            _jumpscareDirect = _playerPresence.PlayerManager.GetComponent<JumpscareDirect>();
            _gameManager = GetComponent<GameManager>();

            _fearTentancles = _gameManager.GetStack<FearTentancles>();
            _cinemachineBasicMultiChannelPerlin = _playerPresence.PlayerVirtualCamera.
                GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        }

        private void Update()
        {
            if (_isDirectJumpscare)
            {
                _directTimer -= Time.deltaTime;

                if (_directTimer <= 0f)
                {
                    if(_directModel != null)
                    {
                        _directModel.SetActive(false);
                        _directModel = null;
                    }
                    else
                    {
                        _directImage.gameObject.SetActive(false);
                        _directImage.rectTransform.localScale = Vector3.one;
                    }

                    _isDirectJumpscare = false;
                }
                else if(_directModel == null)
                {
                    float directTimeOffset = _directDuration * _imageScaleTime;
                    float directValue = Mathf.InverseLerp(_directDuration - directTimeOffset, 0f, _directTimer);
                    float directScale = Mathf.Lerp(1f, _imageMaxScale, directValue);
                    _directImage.rectTransform.localScale = Vector3.one * directScale;
                }
            }

            float amplitudeGain = _cinemachineBasicMultiChannelPerlin.m_AmplitudeGain;
            if (_wobbleTimer > 0)
            {
                _wobbleTimer -= Time.deltaTime;
            }
            else if (amplitudeGain > 0f)
            {
                _cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = Mathf.MoveTowards(amplitudeGain, 0f, Time.deltaTime * _wobbleLossRate);
            }

            if (_influenceFear && _showTentacles)
            {
                if (_fearTentancles.EffectFade.value < 1f && !_tentaclesFaded)
                {
                    float fade = _fearTentancles.EffectFade.value;
                    _fearTentancles.EffectFade.value = Mathf.MoveTowards(fade, 1f, Time.deltaTime * _tentaclesFadeInSpeed);
                }
                else if(_fearTimer > 0f)
                {
                    float fearSpeedOffset = _fearDuration - _fearDuration * _fearSpeedDuration;
                    float fearIntensityOffset = _fearDuration - _fearDuration * _fearIntensityDuration;
                    _fearTimer -= Time.deltaTime;

                    if(_fearTimer <= fearSpeedOffset)
                    {
                        float speed = _fearTentancles.TentaclesSpeed.value;
                        _fearTentancles.TentaclesSpeed.value = Mathf.Lerp(speed, _tentaclesDefaultSpeed, Time.deltaTime * _tentaclesAnimationSpeed);
                    }

                    if (_fearTimer <= fearIntensityOffset)
                    {
                        float position = _fearTentancles.TentaclesPosition.value;
                        _fearTentancles.TentaclesPosition.value = Mathf.Lerp(position, _tentaclesDefaultPosition, Time.deltaTime * _tentaclesMoveSpeed);
                    }

                    _tentaclesFaded = true;
                }
                else if(_tentaclesFaded)
                {
                    if(_fearTentancles.EffectFade.value > 0f)
                    {
                        float fade = _fearTentancles.EffectFade.value;
                        _fearTentancles.EffectFade.value = Mathf.MoveTowards(fade, 0f, Time.deltaTime * _tentaclesFadeOutSpeed);
                    }
                    else
                    {
                        _fearTimer = 0f;
                        _fearDuration = 0f;
                        _fearTentancles.EffectFade.value = 0f;
                        _tentaclesFaded = false;
                        _showTentacles = false;
                        _influenceFear = false;
                    }
                }
            }
        }

        public void StartJumpscareEffect(JumpscareTrigger jumpscare)
        {
            if (jumpscare.InfluenceWobble)
            {
                _cinemachineBasicMultiChannelPerlin.m_AmplitudeGain = jumpscare.WobbleAmplitudeGain;
                _cinemachineBasicMultiChannelPerlin.m_FrequencyGain = jumpscare.WobbleFrequencyGain;
            }

            if(jumpscare.JumpscareType == JumpscareTypeEnum.Direct)
            {
                if (jumpscare.DirectType == DirectTypeEnum.Image)
                {
                    _directImage.sprite = jumpscare.JumpscareImage;
                    _directImage.gameObject.SetActive(true);

                    _directDuration = jumpscare.DirectDuration;
                    _directTimer = jumpscare.DirectDuration;
                    _isDirectJumpscare = true;
                }
                else if (jumpscare.DirectType == DirectTypeEnum.Model)
                {
                    _jumpscareDirect.ShowDirectJumpscare(jumpscare.JumpscareModelID, jumpscare.DirectDuration);
                }
            }
            else if((jumpscare.JumpscareType == JumpscareTypeEnum.Indirect || jumpscare.JumpscareType == JumpscareTypeEnum.Audio) && jumpscare.LookAtJumpscare)
            {
                _isPlayerLocked = jumpscare.LockPlayer;
                _lookController.LerpRotation(jumpscare.LookAtTarget, jumpscare.LookAtDuration, _isPlayerLocked);
                if (_isPlayerLocked) _gameManager.FreezePlayer(true);
            }

            if (jumpscare.InfluenceFear)
            {
                _fearTentancles.TentaclesPosition.value = Mathf.Lerp(0f, 0.2f, jumpscare.TentaclesIntensity);
                _fearTentancles.TentaclesSpeed.value = jumpscare.TentaclesSpeed;
                _fearTentancles.VignetteStrength.value = jumpscare.VignetteStrength;
                _fearDuration = jumpscare.FearDuration;
                _fearTimer = jumpscare.FearDuration;
                _tentaclesFaded = false;
                _showTentacles = true;
                _influenceFear = true;
            }
        }

        public void EndJumpscareEffect()
        {
            if (!_isPlayerLocked)
                return;

            _gameManager.FreezePlayer(false);
            _lookController.LookLocked = false;
            _isPlayerLocked = false;
        }
    }
}