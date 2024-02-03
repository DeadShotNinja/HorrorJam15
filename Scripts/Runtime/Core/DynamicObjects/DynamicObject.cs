using UnityEngine;
using UnityEngine.Events;
using HJ.Input;
using HJ.Tools;
using Newtonsoft.Json.Linq;
using Sirenix.OdinInspector;

namespace HJ.Runtime
{
    public enum DynamicSoundType { Open, Close, Locked, Unlock }

    public class DynamicObject : MonoBehaviour, IInteractStartPlayer, IInteractHold, IInteractStop, ISaveable
    {
        public enum DynamicType { Openable, Pullable, Switchable, Rotable }
        public enum InteractType { Dynamic, Mouse, Animation }
        public enum DynamicStatus { Normal, Locked }
        public enum StatusChange { InventoryItem, CustomScript, None }

        // enums
        [SerializeField] private DynamicType _dynamicType = DynamicType.Openable;
        [SerializeField] private DynamicStatus _dynamicStatus = DynamicStatus.Normal;
        [SerializeField] private InteractType _interactType = InteractType.Dynamic;
        [SerializeField] private StatusChange _statusChange = StatusChange.InventoryItem;

        // general
        [SerializeField] private Transform _target;
        [SerializeField] private Animator _animator;
        [SerializeField] private HingeJoint _joint;
        [SerializeField] private new Rigidbody _rigidbody;
        [SerializeField] private Inventory _inventory;
        [SerializeField] private GameManager _gameManager;

        // items
        // TODO: Figure out bug with this validation
        //[RequireInterface(typeof(IDynamicUnlock))]
        [InfoBox("These MUST implement IDynamicUnlock interface!")]
        [SerializeField] private MonoBehaviour _unlockScript;
        public bool KeepUnlockItem;
        [SerializeField] private ItemGuid _unlockItem;
        [SerializeField] private bool _showLockedText;
        [SerializeField] private GString _lockedText;

        [SerializeField] private Collider[] _ignoreColliders;
        [SerializeField] private string _useTrigger1 = "Open";
        [SerializeField] private string _useTrigger2 = "Close";
        [SerializeField] private string _useTrigger3 = "OpenSide";

        // dynamic types
        [SerializeField] private DynamicOpenable _openable = new DynamicOpenable();
        [SerializeField] private DynamicPullable _pullable = new DynamicPullable();
        [SerializeField] private DynamicSwitchable _switchable = new DynamicSwitchable();
        [SerializeField] private DynamicRotable _rotable = new DynamicRotable();

        // events
        [SerializeField] private UnityEvent _useEvent1;
        [SerializeField] private UnityEvent _useEvent2;
        [SerializeField] private UnityEvent<float> _onValueChange;
        [SerializeField] private UnityEvent _lockedEvent;
        [SerializeField] private UnityEvent _unlockedEvent;

        // hidden variables
        [Tooltip("Lock player controls when interacting with a dynamic object.")]
        [SerializeField] private bool _lockPlayer;
        [SerializeField] private bool _isLocked;
        [SerializeField] private bool _isInteractLocked;

        #region Properties

        public DynamicType DynamicTypeEnum => _dynamicType;
        public DynamicStatus DynamicStatusEnum => _dynamicStatus;
        public InteractType InteractTypeEnum => _interactType;
        public StatusChange StatusChangeEnum => _statusChange;
        public Animator Animator => _animator;
        public HingeJoint Joint => _joint;
        public Rigidbody Rigidbody => _rigidbody;
        public Inventory Inventory => _inventory;
        public GameManager GameManager => _gameManager;
        public MonoBehaviour UnlockScript => _unlockScript;
        public ItemGuid UnlockItem => _unlockItem;
        public bool ShowLockedText => _showLockedText;
        public GString LockedText => _lockedText;
        public string UseTrigger1 => _useTrigger1;
        public string UseTrigger2 => _useTrigger2;
        public string UseTrigger3 => _useTrigger3;
        public DynamicOpenable Openable => _openable;
        public DynamicPullable Pullable => _pullable;
        public DynamicSwitchable Switchable => _switchable;
        public DynamicRotable Rotable => _rotable;
        public UnityEvent UseEvent1 => _useEvent1;
        public UnityEvent UseEvent2 => _useEvent2;
        public UnityEvent<float> OnValueChange => _onValueChange;
        public UnityEvent LockedEvent => _lockedEvent;
        public UnityEvent UnlockedEvent => _unlockedEvent;
        public bool LockPlayer => _lockPlayer;
        public bool IsLocked => _isLocked;
        public Transform Target => _target;

        #endregion

        public DynamicObjectType CurrentDynamic
        {
            get => _dynamicType switch
            {
                DynamicType.Openable => _openable,
                DynamicType.Pullable => _pullable,
                DynamicType.Switchable => _switchable,
                DynamicType.Rotable => _rotable,
                _ => null,
            };
        }

        public bool IsOpened => CurrentDynamic.IsOpened;

        public bool IsHolding => CurrentDynamic.IsHolding;

        private void OnValidate()
        {
            _openable.DynamicObject = this;
            _pullable.DynamicObject = this;
            _switchable.DynamicObject = this;
            _rotable.DynamicObject = this;
        }

        private void Awake()
        {
            _inventory = Inventory.Instance;
            _gameManager = GameManager.Instance;

            if (_dynamicStatus == DynamicStatus.Locked)
                _isLocked = true;

            CurrentDynamic?.OnDynamicInit();
        }

        private void Start()
        {
            if(_interactType == InteractType.Mouse)
            {
                Collider collider = GetComponent<Collider>();
                foreach (var col in _ignoreColliders)
                {
                    Physics.IgnoreCollision(collider, col);
                }
            }

            if (_showLockedText)
                _lockedText.SubscribeGloc();
        }

        private void Update()
        {
            if (!_isLocked) CurrentDynamic?.OnDynamicUpdate();
        }

        public void InteractStartPlayer(GameObject player)
        {
            if (_isInteractLocked) return;
            PlayerManager playerManager = player.GetComponent<PlayerManager>();
            CurrentDynamic?.OnDynamicStart(playerManager);
        }

        public void InteractHold(Vector3 point)
        {
            Vector2 delta = InputManager.ReadInput<Vector2>(Controls.POINTER_DELTA);
            if (!_isLocked) CurrentDynamic?.OnDynamicHold(delta);
        }

        public void InteractStop()
        {
            if (!_isLocked) CurrentDynamic?.OnDynamicEnd();
        }

        /// <summary>
        /// Set dynamic object locked status.
        /// </summary>
        public void SetLockedStatus(bool locked)
        {
            _isLocked = locked;
            if (!locked) _isInteractLocked = false;
        }

        /// <summary>
        /// Set dynamic object open state.
        /// </summary>
        /// <remarks>
        /// The dynamic object opens as if you were interacting with it. If the dynamic interaction type is mouse, nothing happens.
        /// <br>This function is good for calling from an event.</br>
        /// </remarks>
        public void SetOpenState()
        {
            if (_interactType == InteractType.Mouse || _dynamicStatus == DynamicStatus.Locked)
                return;

            CurrentDynamic?.OnDynamicOpen();
        }

        /// <summary>
        /// Set dynamic object close state.
        /// </summary>
        /// <remarks>
        /// The dynamic object opens as if you were interacting with it. If the dynamic interaction type is mouse, nothing happens.
        /// <br>This function is good for calling from an event.</br>
        /// </remarks>
        public void SetCloseState()
        {
            if (_interactType == InteractType.Mouse || _dynamicStatus == DynamicStatus.Locked)
                return;

            CurrentDynamic?.OnDynamicClose();
        }

        /// <summary>
        /// Play Dynamic Object Sound.
        /// </summary>
        public void PlaySound(DynamicSoundType soundType)
        {
            switch (soundType)
            {
                case DynamicSoundType.Open: AudioManager.PostAudioEvent(AudioEnvironment.DynamicOpen, gameObject); break;
                case DynamicSoundType.Close: AudioManager.PostAudioEvent(AudioEnvironment.DynamicClose, gameObject); break;
                case DynamicSoundType.Locked: AudioManager.PostAudioEvent(AudioEnvironment.DynamicLocked, gameObject); break;
                case DynamicSoundType.Unlock: AudioManager.PostAudioEvent(AudioEnvironment.DynamicUnlock, gameObject); break;
            }
        }

        /// <summary>
        /// The result of using the custom unlock script function.
        /// <br>Call this function after using the OnTryUnlock() function.</br>
        /// </summary>
        public void TryUnlockResult(bool unlocked)
        {
            if (unlocked)
            {
                _unlockedEvent?.Invoke();
                PlaySound(DynamicSoundType.Unlock);
            }
            else
            {
                _lockedEvent?.Invoke();
                PlaySound(DynamicSoundType.Locked);
            }

            SetLockedStatus(!unlocked);
        }

        private void OnDrawGizmosSelected()
        {
            if(CurrentDynamic.ShowGizmos)
                CurrentDynamic?.OnDrawGizmos();
        }

        public StorableCollection OnSave()
        {
            StorableCollection saveableBuffer = new StorableCollection();

            switch (_dynamicType)
            {
                case DynamicType.Openable:
                    saveableBuffer = _openable.OnSave();
                    break;
                case DynamicType.Pullable:
                    saveableBuffer = _pullable.OnSave();
                    break;
                case DynamicType.Switchable:
                    saveableBuffer = _switchable.OnSave();
                    break;
                case DynamicType.Rotable:
                    saveableBuffer = _rotable.OnSave();
                    break;
                default:
                    break;
            }

            saveableBuffer.Add("isLocked", _isLocked);
            return saveableBuffer;
        }

        public void OnLoad(JToken data)
        {
            switch (_dynamicType)
            {
                case DynamicType.Openable:
                    _openable.OnLoad(data);
                    break;
                case DynamicType.Pullable:
                    _pullable.OnLoad(data);
                    break;
                case DynamicType.Switchable:
                    _switchable.OnLoad(data);
                    break;
                case DynamicType.Rotable:
                    _rotable.OnLoad(data);
                    break;
            }

            _isLocked = (bool)data["isLocked"];
        }
    }
}