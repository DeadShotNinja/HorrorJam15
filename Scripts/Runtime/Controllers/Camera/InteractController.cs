using UnityEngine;
using HJ.Input;
using HJ.Tools;
using static HJ.Runtime.InteractableItem;

namespace HJ.Runtime
{
    public class InteractController : PlayerComponent
    {
        [Header("Raycast")]
        [SerializeField] private float _raycastRange = 3f;
        [SerializeField] private float _holdDistance = 4f;
        [SerializeField] private float _holdPointCreationTime = 0.5f;
        [SerializeField] private LayerMask _cullLayers;
        [SerializeField] private Layer _interactLayer;

        [Header("Settings")]
        [SerializeField] private bool _showLootedText;
        [SerializeField] private bool _showDefaultPickupIcon;
        [SerializeField] private Sprite _defaultPickupIcon;

        [Header("Interact Settings")]
        [SerializeField] private InputReference _useAction;
        [SerializeField] private InputReference _examineAction;

        [Header("Interact Texts")]
        [SerializeField] private GString _interactText;
        [SerializeField] private GString _examineText;
        [SerializeField] private GString _lootText;

        private Inventory _inventory;
        private GameManager _gameManager;
        private PlayerStateMachine _player;

        private GameObject _raycastObject;
        private GameObject _interactableObject;
        private Transform _holdPointObject;

        private bool _isPressed;
        private bool _isHolding;
        private bool _isDynamic;
        private bool _isTimed;
        private bool _showInteractInfo = true;

        private IInteractTimed _timedInteract;
        private float _reqHoldTime;
        private float _holdTime;

        private bool _isHoldPointCreated;
        private float _holdPointCreateTime;
        private Vector3 _localHitpoint;

        public Layer InteractLayer => _interactLayer;
        
        public GameObject RaycastObject => _raycastObject;
        public Vector3 LocalHitpoint => _localHitpoint;

        private void Awake()
        {
            _inventory = Inventory.Instance;
            _gameManager = GameManager.Instance;
            _player = PlayerCollider.GetComponent<PlayerStateMachine>();
        }

        private void Start()
        {
            _interactText.SubscribeGlocMany();
            _examineText.SubscribeGlocMany();
            _lootText.SubscribeGloc();
        }

        public void EnableInteractInfo(bool state)
        {
            if (!state) _gameManager.InteractInfoPanel.HideInfo();
            _showInteractInfo = state;
        }

        private void Update()
        {
            if (!_isEnabled && !_isHolding) return;

            Ray playerAim = MainCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            if(GameTools.Raycast(playerAim, out RaycastHit hit, _raycastRange, _cullLayers, _interactLayer))
            {
                _raycastObject = hit.collider.gameObject;
                if (!_isHolding)
                {
                    _interactableObject = _raycastObject;
                    _localHitpoint = _interactableObject.transform.InverseTransformPoint(hit.point);
                }

                if (_showInteractInfo) OnInteractGUI();
            }
            else
            {
                if (_isTimed)
                {
                    _gameManager.ShowInteractProgress(false, 0f);
                    _timedInteract = null;
                    _reqHoldTime = 0f;
                    _holdTime = 0f;
                    _isTimed = false;
                }

                _gameManager.InteractInfoPanel.HideInfo();
                _raycastObject = null;
            }

            if (InputManager.ReadButton(Controls.USE))
            {
                _isHolding = true;

                if (_interactableObject)
                {
                    if (!_isPressed)
                    {
                        foreach (var interactStartPlayer in _interactableObject.GetComponents<IInteractStartPlayer>())
                        {
                            interactStartPlayer.InteractStartPlayer(transform.root.gameObject);
                        }

                        foreach (var interactStart in _interactableObject.GetComponents<IInteractStart>())
                        {
                            interactStart.InteractStart();
                        }

                        if(_interactableObject.TryGetComponent(out IInteractTimed timedInteract))
                        {
                            if (!timedInteract.NoInteract)
                            {
                                this._timedInteract = timedInteract;
                                _reqHoldTime = timedInteract.InteractTime;
                                _isTimed = true;
                            }
                        }

                        if(_interactableObject.TryGetComponent(out IStateInteract stateInteract))
                        {
                            StateParams stateParams = stateInteract.OnStateInteract();
                            if(stateParams != null) _player.ChangeState(stateParams.StateKey, stateParams.StateData);
                        }

                        if (_interactableObject.TryGetComponent(out DynamicObject dynamicObject))
                        {
                            if ((dynamicObject.InteractTypeEnum == DynamicObject.InteractType.Mouse || dynamicObject.LockPlayer) && !dynamicObject.IsLocked)
                            {
                                _gameManager.FreezePlayer(true);
                                _isDynamic = true;
                            }
                        }

                        Interact(_interactableObject);
                        _isPressed = true;
                    }

                    if (!_isHoldPointCreated)
                    {
                        if (_holdPointCreateTime >= _holdPointCreationTime)
                        {
                            if (RaycastObject != null)
                            {
                                GameObject holdPointObj = new GameObject("HoldPoint");
                                holdPointObj.transform.parent = RaycastObject.transform;
                                holdPointObj.transform.position = RaycastObject.transform.TransformPoint(_localHitpoint);
                                _holdPointObject = holdPointObj.transform;
                            }
                            _isHoldPointCreated = true;
                        }
                        _holdPointCreateTime += Time.deltaTime;
                    }

                    foreach (var interactHold in _interactableObject.GetComponents<IInteractHold>())
                    {
                        interactHold.InteractHold(hit.point);
                    }
                }
            }
            else if (_isPressed)
            {
                if (_isTimed)
                {
                    _gameManager.ShowInteractProgress(false, 0f);
                    _timedInteract = null;
                    _reqHoldTime = 0f;
                    _holdTime = 0f;
                }

                if (_interactableObject)
                {
                    foreach (var interactStop in _interactableObject.GetComponents<IInteractStop>())
                    {
                        interactStop.InteractStop();
                    }
                }

                _isTimed = false;
                _isPressed = false;
            }
            else
            {
                if (_isDynamic)
                {
                    _gameManager.FreezePlayer(false);
                    _isDynamic = false;
                }

                if (_holdPointObject)
                {
                    Destroy(_holdPointObject.gameObject);
                    _holdPointObject = null;
                }

                _holdPointCreateTime = 0;
                _interactableObject = null;
                _isHoldPointCreated = false;
                _isHolding = false;
            }

            if(_isPressed && _isTimed)
            {
                if (_holdTime < _reqHoldTime)
                {
                    _holdTime += Time.deltaTime;
                    float progress = Mathf.InverseLerp(0f, _reqHoldTime, _holdTime);
                    _gameManager.ShowInteractProgress(true, progress);
                }
                else
                {
                    _gameManager.ShowInteractProgress(false, 0f);
                    _timedInteract.InteractTimed();
                    _timedInteract = null;
                    _reqHoldTime = 0f;
                    _holdTime = 0f;
                    _isTimed = false;
                }
            }

            if(_isPressed && _holdPointObject && _interactableObject)
            {
                float distance = Vector3.Distance(MainCamera.transform.position, _holdPointObject.position);
                if (distance > _holdDistance)
                {
                    if (_interactableObject)
                    {
                        foreach (var interactStop in _interactableObject.GetComponents<IInteractStop>())
                        {
                            interactStop.InteractStop();
                        }
                    }

                    _interactableObject = null;
                    _isPressed = false;
                }
            }
        }

        private void OnInteractGUI()
        {
            if (_interactableObject == null)
                return;

            string titleText = null;
            string button1Text = null;
            string button2Text = null;

            InputReference button1Action = _useAction;
            InputReference button2Action = _examineAction;

            if (_interactableObject.TryGetComponent(out IInteractTitle interactMessage))
            {
                TitleParams messageParams = interactMessage.InteractTitle();
                titleText = messageParams.Title ?? null;
                button1Text = messageParams.Button1 ?? null;
                button2Text = messageParams.Button2 ?? null;
            }

            if (_interactableObject.TryGetComponent(out InteractableItem interactable))
            {
                if (interactable.InteractableType == InteractableTypeEnum.InventoryItem)
                {
                    if (interactable.UseInventoryTitle)
                    {
                        Item item = interactable.PickupItem.GetItem();
                        titleText ??= item.Title;
                    }
                    else
                    {
                        titleText ??= interactable.InteractTitle;
                    }

                    button1Text ??= _interactText;
                    button2Text ??= interactable.ExamineType != ExamineTypeEnum.None ? _examineText : null;
                }
                else if (interactable.InteractableType == InteractableTypeEnum.GenericItem || interactable.InteractableType == InteractableTypeEnum.InventoryExpand)
                {
                    titleText ??= interactable.InteractTitle;
                    button1Text ??= _interactText;
                    button2Text ??= interactable.ExamineType != ExamineTypeEnum.None ? _examineText : null;
                }
                else if (interactable.InteractableType == InteractableTypeEnum.ExamineItem)
                {
                    titleText ??= interactable.InteractTitle;
                    button1Text ??= _examineText;
                    button1Action = _examineAction;
                }
            }

            titleText ??= _interactableObject.name;
            button1Text ??= _interactText;

            InteractContext button1 = null;
            if (!string.IsNullOrEmpty(button1Text))
            {
                button1 = new()
                {
                     InputAction = button1Action,
                     InteractName = button1Text
                };
            }

            InteractContext button2 = null;
            if (!string.IsNullOrEmpty(button2Text))
            {
                button2 = new()
                {
                    InputAction = button2Action,
                    InteractName = button2Text
                };
            }

            _gameManager.InteractInfoPanel.ShowInfo(new()
            {
                 ObjectName = titleText,
                 Contexts = new[] { button1, button2 }
            });
        }

        public void Interact(GameObject interactObj)
        {
            if(interactObj.TryGetComponent(out InteractableItem interactable))
            {
                bool isAddedToInventory = false;

                if(interactable.InteractableType == InteractableTypeEnum.InventoryItem)
                {
                    isAddedToInventory = _inventory.AddItem(interactable.PickupItem.GUID, interactable.Quantity, interactable.ItemCustomData);
                }

                if (interactable.InteractableType == InteractableTypeEnum.InventoryExpand)
                {
                    _inventory.ExpandInventory(interactable.SlotsToExpand, interactable.ExpandRows);
                    isAddedToInventory = true;
                }

                if (isAddedToInventory)
                {
                    if (interactable.MessageType == MessageTypeEnum.Alert)
                    {
                        string pickupText = _showLootedText ? _lootText + " " + interactable.ItemName : interactable.ItemName;

                        if (_showDefaultPickupIcon)
                        {
                            _gameManager.ShowItemPickupMessage(pickupText, _defaultPickupIcon, interactable.MessageTime);
                        }
                        else
                        {
                            var pickupIcon = interactable.PickupItem.GetItem().Icon;
                            _gameManager.ShowItemPickupMessage(pickupText, pickupIcon, interactable.MessageTime);
                        }
                    }
                    else if (interactable.MessageType == MessageTypeEnum.Hint)
                    {
                        _gameManager.ShowHintMessage(interactable.HintMessage, interactable.MessageTime);
                    }
                }

                if(isAddedToInventory || interactable.InteractableType == InteractableTypeEnum.GenericItem)
                {
                    interactable.OnInteract();
                }
            }
        }

        private void OnDrawGizmos()
        {
            if(_interactableObject != null && _isHoldPointCreated)
            {
                Vector3 pointPos = _interactableObject.transform.TransformPoint(_localHitpoint);
                Gizmos.color = Color.red.Alpha(0.5f);
                Gizmos.DrawSphere(pointPos, 0.03f);
            }
        }
    }
}