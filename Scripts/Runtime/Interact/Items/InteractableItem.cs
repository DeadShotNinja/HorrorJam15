using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json.Linq;

namespace HJ.Runtime
{
    public class InteractableItem : SaveableBehaviour
    {
        public enum InteractableTypeEnum { GenericItem, InventoryItem, ExamineItem, InventoryExpand }
        public enum MessageTypeEnum { None, Hint, Alert }
        public enum ExamineTypeEnum { None, GenericObject, CustomObject }
        public enum ExamineRotateEnum { Static, Horizontal, Vertical, Both }
        public enum DisableTypeEnum { None, Deactivate, Destroy }

        [SerializeField] private InteractableTypeEnum _interactableType = InteractableTypeEnum.GenericItem;
        [SerializeField] private MessageTypeEnum _messageType = MessageTypeEnum.None;
        [SerializeField] private ExamineTypeEnum _examineType = ExamineTypeEnum.None;
        [SerializeField] private ExamineRotateEnum _examineRotate = ExamineRotateEnum.Static;
        [SerializeField] private DisableTypeEnum _disableType = DisableTypeEnum.None;

        [SerializeField] private ItemProperty _pickupItem;
        [SerializeField] private ItemCustomData _itemCustomData;

        [SerializeField] private GString _interactTitle;
        [SerializeField] private GString _examineTitle;

        [SerializeField] private GString _paperText;
        [SerializeField] private GString _hintMessage;

        [SerializeField] private float _messageTime = 3f;

        [SerializeField] private ushort _quantity = 1;
        [SerializeField] private ushort _slotsToExpand = 1;
        [SerializeField] private bool _expandRows;

        [SerializeField] private float _examineDistance = 0.4f;
        [SerializeField] private MinMax _examineZoomLimits = new(0.3f, 0.4f);

        [SerializeField] private bool _useExamineZooming = true;
        [SerializeField] private bool _useInventoryTitle = true;
        [SerializeField] private bool _examineInventoryTitle = true;
        [SerializeField] private bool _showExamineTitle = true;

        [SerializeField] private bool _showFloatingIcon = false;
        [SerializeField] private bool _takeFromExamine = false;
        [SerializeField] private bool _allowCursorExamine = false;
        [SerializeField] private bool _isPaper = false;

        [SerializeField] private bool _useFaceRotation;
        [SerializeField] private Vector3 _faceRotation;

        [SerializeField] private bool _useControlPoint;
        [SerializeField] private Vector3 _controlPoint = new(0, 0.1f, 0);

        [SerializeField] private List<Collider> _collidersEnable = new();
        [SerializeField] private List<Collider> _collidersDisable = new();
        [SerializeField] private Hotspot _examineHotspot = new();

        [SerializeField] private UnityEvent _onTakeEvent;
        [SerializeField] private UnityEvent _onExamineStartEvent;
        [SerializeField] private UnityEvent _onExamineEndEvent;

        [SerializeField] private bool _isExamined;

        #region Properties
        public InteractableTypeEnum InteractableType => _interactableType;
        public MessageTypeEnum MessageType => _messageType;
        public ExamineTypeEnum ExamineType => _examineType;
        public ExamineRotateEnum ExamineRotate => _examineRotate;
        public DisableTypeEnum DisableType
        {
            get => _disableType;
            set => _disableType = value;
        }
        public ItemProperty PickupItem => _pickupItem;
        public ItemCustomData ItemCustomData => _itemCustomData;
        public GString InteractTitle => _interactTitle;
        public GString ExamineTitle => _examineTitle;
        public GString PaperText => _paperText;
        public GString HintMessage => _hintMessage;
        public float MessageTime => _messageTime;

        public ushort Quantity
        {
            get => _quantity;
            set => _quantity = value;
        }
        public ushort SlotsToExpand => _slotsToExpand;
        public bool ExpandRows => _expandRows;
        public float ExamineDistance => _examineDistance;
        public MinMax ExamineZoomLimits => _examineZoomLimits;
        public bool UseExamineZooming => _useExamineZooming;
        public bool UseInventoryTitle => _useInventoryTitle;
        public bool ExamineInventoryTitle => _examineInventoryTitle;
        public bool ShowExamineTitle => _showExamineTitle;
        public bool ShowFloatingIcon => _showFloatingIcon;
        public bool TakeFromExamine => _takeFromExamine;
        public bool AllowCursorExamine => _allowCursorExamine;
        public bool IsPaper => _isPaper;
        public bool UseFaceRotation => _useFaceRotation;
        public Vector3 FaceRotation => _faceRotation;
        public bool UseControlPoint => _useControlPoint;
        public Vector3 ControlPoint => _controlPoint;
        public List<Collider> CollidersEnable => _collidersEnable;
        public List<Collider> CollidersDisable => _collidersDisable;
        public Hotspot ExamineHotspot => _examineHotspot;
        public UnityEvent OnTakeEvent => _onTakeEvent;
        public UnityEvent OnExamineStartEvent => _onExamineStartEvent;
        public UnityEvent OnExamineEndEvent => _onExamineEndEvent;

        public bool IsExamined
        {
            get => _isExamined;
            set => _isExamined = value;
        }
        
        public bool IsCustomExamine => _interactableType != InteractableTypeEnum.GenericItem && _examineType == ExamineTypeEnum.CustomObject;
        #endregion

        /// <summary>
        /// Name of the item.
        /// </summary>
        public string ItemName
        {
            get
            {
                string title = _interactTitle;

                if (_interactableType == InteractableTypeEnum.InventoryItem && _useInventoryTitle)
                {
                    title = _pickupItem.GetItem().Title;
                }

                return title;
            }
        }

        private void Awake()
        {
            if(IsCustomExamine)
            {
                foreach (var col in _collidersEnable)
                {
                    col.enabled = false;
                }

                foreach (var col in _collidersDisable)
                {
                    col.enabled = true;
                }
            }
        }

        private void Start()
        {
            if (_interactableType != InteractableTypeEnum.InventoryItem || !_useInventoryTitle)
                _interactTitle.SubscribeGloc();

            if(_examineType != ExamineTypeEnum.None)
                _examineTitle.SubscribeGloc();

            if(_examineType != ExamineTypeEnum.None && _isPaper)
                _paperText.SubscribeGloc();

            if(_messageType == MessageTypeEnum.Hint)
                _hintMessage.SubscribeGloc();
        }

        public void OnInteract()
        {
            if (_interactableType == InteractableTypeEnum.ExamineItem)
                return;

            AudioManager.PostAudioEvent(AudioItems.ItemPickup, gameObject);
            _onTakeEvent?.Invoke();

            if (_disableType != DisableTypeEnum.None)
                EnabledState(false);
        }

        /// <summary>
        /// Use this method to interact with the object as if you were pressing the USE button on it.
        /// </summary>
        public void InteractWithObject()
        {
            if (_interactableType == InteractableTypeEnum.ExamineItem)
                return;

            Debug.Log("Interact");
            PlayerPresenceManager.Instance.PlayerManager.InteractController.Interact(gameObject);
        }

        public void EnabledState(bool enabled)
        {
            if (!enabled && SaveGameManager.HasReference) 
                SaveGameManager.RemoveSaveable(gameObject);

            if(_disableType == DisableTypeEnum.Deactivate)
                gameObject.SetActive(enabled);
            else if(_disableType == DisableTypeEnum.Destroy && !enabled)
                Destroy(gameObject);
        }

        public override StorableCollection OnSave()
        {
            return new StorableCollection()
            {
                { "position", transform.position.ToSaveable() },
                { "rotation", transform.eulerAngles.ToSaveable() },
                { "quantity", _quantity },
                { "enabledState", gameObject.activeSelf },
                { "hotspotEnabled", _examineHotspot.Enabled },
                { "customData", _itemCustomData.GetJson() }
            };
        }

        public override void OnLoad(JToken data)
        {
            transform.position = data["position"].ToObject<Vector3>();
            transform.eulerAngles = data["rotation"].ToObject<Vector3>();

            _quantity = (ushort)data["quantity"];
            EnabledState((bool)data["enabledState"]);
            _examineHotspot.Enabled = (bool)data["hotspotEnabled"];

            _itemCustomData.JsonData = data["customData"].ToString();
        }
    }
}