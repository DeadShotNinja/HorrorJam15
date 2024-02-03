using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;
using HJ.Tools;

namespace HJ.Runtime
{
    public class JumpscareTrigger : MonoBehaviour, ISaveable
    {
        public enum JumpscareTypeEnum { Direct, Indirect, Audio }
        public enum DirectTypeEnum { Image, Model }
        public enum TriggerTypeEnum { Event, TriggerEnter, TriggerExit }

        [SerializeField] private JumpscareTypeEnum _jumpscareType = JumpscareTypeEnum.Direct;
        [SerializeField] private DirectTypeEnum _directType = DirectTypeEnum.Image;
        [SerializeField] private TriggerTypeEnum _triggerType = TriggerTypeEnum.Event;

        [SerializeField] private Sprite _jumpscareImage;
        [SerializeField] private string _jumpscareModelID = "scare_monster";

        [SerializeField] private Animator _animator;
        [SerializeField] private string _animatorStateName = "Jumpscare";
        [SerializeField] private string _animatorTrigger = "Jumpscare";

        [SerializeField] private bool _influenceFear;
        [SerializeField, Range(0f, 1f)] private float _tentaclesIntensity = 0f;
        [SerializeField, Range(0.1f, 3f)] private float _tentaclesSpeed = 1f;
        [SerializeField, Range(0f, 1f)] private float _vignetteStrength = 0f;

        [SerializeField] private bool _lookAtJumpscare;
        [SerializeField] private Transform _lookAtTarget;
        [SerializeField] private float _lookAtDuration;
        [SerializeField] private bool _lockPlayer;
        [SerializeField] private bool _endJumpscareWithEvent;

        [SerializeField] private bool _influenceWobble;
        [SerializeField] private float _wobbleAmplitudeGain = 1f;
        [SerializeField] private float _wobbleFrequencyGain = 1f;

        [SerializeField] private float _wobbleDuration = 0.2f;
        [SerializeField] private float _directDuration = 1f;
        [SerializeField] private float _fearDuration = 1f;

        [SerializeField] private UnityEvent _triggerEnter;
        [SerializeField] private UnityEvent _triggerExit;

        [SerializeField] private UnityEvent _onJumpscareStarted;
        [SerializeField] private UnityEvent _onJumpscareEnded;

        private bool _jumpscareStarted;
        private bool _triggerEntered;

        private JumpscareManager _jumpscareManager;

        #region Properties

        public JumpscareTypeEnum JumpscareType => _jumpscareType;
        public DirectTypeEnum DirectType => _directType;
        public TriggerTypeEnum TriggerType => _triggerType;
        public bool InfluenceWobble => _influenceWobble;
        public float WobbleAmplitudeGain => _wobbleAmplitudeGain;
        public float WobbleFrequencyGain => _wobbleFrequencyGain;
        public Sprite JumpscareImage => _jumpscareImage;
        public string JumpscareModelID => _jumpscareModelID;
        public bool InfluenceFear => _influenceFear;
        public float TentaclesIntensity => _tentaclesIntensity;
        public float TentaclesSpeed => _tentaclesSpeed;
        public float VignetteStrength => _vignetteStrength;
        public bool LookAtJumpscare => _lookAtJumpscare;
        public Transform LookAtTarget => _lookAtTarget;
        public float LookAtDuration => _lookAtDuration;
        public bool LockPlayer => _lockPlayer;
        public float DirectDuration => _directDuration;
        public float FearDuration => _fearDuration;
        
        #endregion

        private void Awake()
        {
            _jumpscareManager = JumpscareManager.Instance;
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_triggerType == TriggerTypeEnum.Event)
                return;

            if (other.CompareTag("Player") && !_jumpscareStarted && !_triggerEntered)
            {
                _triggerEnter?.Invoke();

                if (_triggerType == TriggerTypeEnum.TriggerEnter)
                {
                    TriggerJumpscare();
                }
                else if (_triggerType == TriggerTypeEnum.TriggerExit)
                {
                    _triggerEntered = true;
                }
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (_triggerType == TriggerTypeEnum.Event)
                return;

            if (other.CompareTag("Player") && !_jumpscareStarted && _triggerEntered)
            {
                _triggerExit?.Invoke();

                if (_triggerType == TriggerTypeEnum.TriggerExit)
                {
                    TriggerJumpscare();
                }
            }
        }

        public void TriggerJumpscare()
        {
            if (_jumpscareStarted)
                return;

            _onJumpscareStarted?.Invoke();

            if(_jumpscareType == JumpscareTypeEnum.Indirect)
            {
                _animator.SetTrigger(_animatorTrigger);
                StartCoroutine(IndirectJumpscare());
            }

            _jumpscareManager.StartJumpscareEffect(this);
            AudioManager.PostAudioEvent(AudioEnvironment.Jumpscare, gameObject);

            _jumpscareStarted = true;
        }

        public void TriggerJumpscareEnded()
        {
            if (_endJumpscareWithEvent) _jumpscareManager.EndJumpscareEffect();
        }

        private IEnumerator IndirectJumpscare()
        {
            yield return new WaitForAnimatorClip(_animator, _animatorStateName);
            if (!_endJumpscareWithEvent) _jumpscareManager.EndJumpscareEffect();
            _onJumpscareEnded?.Invoke();
        }

        public StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { nameof(_jumpscareStarted), _jumpscareStarted }
            };
        }

        public void OnLoad(JToken data)
        {
            _jumpscareStarted = (bool)data[nameof(_jumpscareStarted)];
        }
    }
}