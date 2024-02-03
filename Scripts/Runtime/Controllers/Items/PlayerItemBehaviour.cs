using UnityEngine;
using HJ.Tools;
using HJ.Scriptable;
using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    public abstract class PlayerItemBehaviour : MonoBehaviour, ISaveableCustom
    {
        [SerializeField] private bool _enableWallDetection = true;
        [SerializeField] private bool _enableMotionPreset = true;

        // wall detection
        [SerializeField] private LayerMask _wallHitMask;
        [SerializeField] private float _wallHitRayDistance = 0.5f;
        [SerializeField] private float _wallHitRayRadius = 0.3f;
        [SerializeField] private float _wallHitAmount = 1f;
        [SerializeField] private float _wallHitTime = 0.2f;
        [SerializeField] private Vector3 _wallHitRayOffset;
        [SerializeField] private bool _showRayGizmos = true;

        // item motion
        [SerializeField] private MotionPreset _motionPreset;

        private Animator _animator;
        private PlayerManager _playerManager;
        private PlayerStateMachine _playerStateMachine;
        private LookController _lookController;
        private ExamineController _examineController;
        private Vector3 _wallHitVel;
        private MotionBlender _motionBlender = new();
        
        private Transform _motionTransform;
        private Quaternion _defaultMotionRot;
        private Vector3 _defaultMotionPos;
        
        public MotionPreset MotionPreset => _motionPreset;
        public MotionBlender MotionBlender => _motionBlender;
        
        /// <summary>
        /// The pivot point of the item object that will be used for the motion preset effects.
        /// </summary>
        [field: SerializeField]
        public Transform MotionPivot { get; set; }

        /// <summary>
        /// The object of the item which will be enabled or disabled, usually a child object.
        /// </summary>
        [field: SerializeField]
        public GameObject ItemObject { get; set; }

        /// <summary>
        /// Animator component of the Item Object.
        /// </summary>
        public Animator Animator
        {
            get
            {
                if(_animator == null)
                    _animator = ItemObject.GetComponentInChildren<Animator>();

                return _animator;
            }
        }

        /// <summary>
        /// PlayerManager component.
        /// </summary>
        public PlayerManager PlayerManager
        {
            get
            {
                if (_playerManager == null)
                    _playerManager = transform.root.GetComponent<PlayerManager>();

                return _playerManager;
            }
        }

        /// <summary>
        /// PlayerStateMachine component.
        /// </summary>
        public PlayerStateMachine PlayerStateMachine
        {
            get
            {
                if (_playerStateMachine == null)
                    _playerStateMachine = transform.root.GetComponent<PlayerStateMachine>();

                return _playerStateMachine;
            }
        }

        /// <summary>
        /// LookController component.
        /// </summary>
        public LookController LookController
        {
            get
            {
                if (_lookController == null)
                    _lookController = transform.GetComponentInParent<LookController>();

                return _lookController;
            }
        }

        /// <summary>
        /// ExamineController component.
        /// </summary>
        public ExamineController ExamineController
        {
            get
            {
                if (_examineController == null)
                    _examineController = transform.GetComponentInParent<ExamineController>();

                return _examineController;
            }
        }

        /// <summary>
        /// PlayerItemsManager component.
        /// </summary>
        public PlayerItemsManager PlayerItems
        {
            get => PlayerManager.PlayerItems;
        }

        /// <summary>
        /// Check if the item is interactive. False, for example when the inventory is opened, object is dragged etc.
        /// </summary>
        public bool CanInteract => PlayerItems.CanInteract;

        /// <summary>
        /// The name of the item that will be displayed in the list.
        /// </summary>
        public virtual string Name => "Item";

        /// <summary>
        /// Check whether the item can be switched.
        /// </summary>
        public virtual bool IsBusy() => false;

        /// <summary>
        /// Check whether the item is equipped.
        /// </summary>
        public virtual bool IsEquipped() => ItemObject.activeSelf;

        /// <summary>
        /// Check whether the item can be combined in inventory.
        /// </summary>
        public virtual bool CanCombine() => false;

        private void Start()
        {
            if (_enableMotionPreset && _motionPreset != null)
            {
                _motionTransform = MotionPivot != null ? MotionPivot : PlayerManager.MotionController.HandsMotionTransform;
                _defaultMotionRot = _motionTransform.localRotation;
                _defaultMotionPos = _motionTransform.localPosition;
                _motionBlender.Init(_motionPreset, _motionTransform, PlayerStateMachine);
            }
        }

        private void OnDestroy()
        {
            if (_enableMotionPreset && _motionPreset != null && _motionBlender != null)
                _motionBlender.Dispose();
        }

        private void Update()
        {
            if (IsEquipped())
            {
                if (_enableWallDetection)
                {
                    Vector3 forward = PlayerItems.transform.forward;
                    Vector3 origin = PlayerItems.transform.TransformPoint(_wallHitRayOffset);

                    if (Physics.SphereCast(origin, _wallHitRayRadius, forward, out RaycastHit hit, _wallHitRayDistance, _wallHitMask))
                        OnItemBlocked(hit.distance, true);
                    else
                        OnItemBlocked(0f, false);
                }

                if (_enableMotionPreset && _motionPreset != null && _motionTransform != null)
                {
                    _motionBlender.BlendMotions(Time.deltaTime, out var position, out var rotation);
                    Vector3 newPosition = _defaultMotionPos + position;
                    Quaternion newRotation = _defaultMotionRot * rotation;
                    _motionTransform.SetLocalPositionAndRotation(newPosition, newRotation);
                }
            }

            OnUpdate();
        }

        /// <summary>
        /// Will be called when the ray going from the camera hits the wall to prevent the player item from being clipped.
        /// </summary>
        public virtual void OnItemBlocked(float hitDistance, bool blocked)
        {
            float value = GameTools.Remap(0f, _wallHitRayDistance, 0f, 1f, hitDistance);
            Vector3 backward = Vector3.back * _wallHitAmount;
            Vector3 result = Vector3.Lerp(backward, Vector3.zero, blocked ? value : 1f);
            transform.localPosition = Vector3.SmoothDamp(transform.localPosition, result, ref _wallHitVel, _wallHitTime);
        }

        /// <summary>
        /// Will be called every frame like the classic Update() function. 
        /// </summary>
        public virtual void OnUpdate() { }

        /// <summary>
        /// Will be called when a combinable item is combined with this inventory item.
        /// </summary>
        public virtual void OnItemCombine(InventoryItem combineItem) { }

        /// <summary>
        /// Will be called when PlayerItemsManager selects an item.
        /// </summary>
        public abstract void OnItemSelect();

        /// <summary>
        /// Will be called when PlayerItemsManager deselects an item.
        /// </summary>
        public abstract void OnItemDeselect();

        /// <summary>
        /// Will be called when PlayerItemsManager activates an item.
        /// </summary>
        public abstract void OnItemActivate();

        /// <summary>
        /// Will be called when PlayerItemsManager deactivates an item.
        /// </summary>
        public abstract void OnItemDeactivate();

        public virtual void OnDrawGizmosSelected()
        {
            bool selected = false;

#if UNITY_EDITOR
            selected = UnityEditor.Selection.activeGameObject == gameObject;
#endif

            if (_showRayGizmos && _enableWallDetection && selected)
            {
                Vector3 forward = PlayerItems.transform.forward;
                Vector3 origin = PlayerItems.transform.TransformPoint(_wallHitRayOffset);
                Vector3 p2 = origin + forward * _wallHitRayDistance;

                Gizmos.color = Color.yellow;
                GizmosE.DrawWireCapsule(origin, p2, _wallHitRayRadius);
            }
        }

        public virtual StorableCollection OnCustomSave()
        {
            return new();
        }

        public virtual void OnCustomLoad(JToken data) { }
    }
}